using Assets.Helper;
using Assets.Model;
using Assets.Repo;
using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class GameLogic : NetworkBehaviour
{
    public enum GameState
    {
        PlayersJoining,
        GameStarted
    }

    public enum TurnState
    {
        Conquer,
        Redeploy
    }

    [Networked(OnChanged = nameof(UiUpdateRequired))] public GameState State { get; set; } // Current state of the game
    [Networked(OnChanged = nameof(TurnChanged))] public int Turn { get; set; } // Turn index (points to _playerTurnOrder)
    [Networked(OnChanged = nameof(UiUpdateRequired))] public TurnState TurnStage { get; set; }
    [Networked(OnChanged = nameof(PlayerTurnChanged))] public int PlayerTurn { get; set; } // A players turn within a game turn
    [Networked] public NetworkString<_64> PlayerTurnId { get; set; } // A players turn within a game turn
    [Networked(OnChanged = nameof(UiUpdateRequired)), Capacity(6)] public NetworkArray<Combo> Combos => default;
    [Networked(OnChanged = nameof(UiUpdateRequired)), Capacity(30)] public NetworkDictionary<NetworkString<_4>, MapArea> MapAreas => default;
    [Networked] public int PlayerCount { get; set; }

    public bool IsNetworkActive => Runner != null;

    public GameObject Map2PlayerPrefab;
    public GameObject Map3PlayerPrefab;
    public GameObject Map4PlayerPrefab;
    public GameObject Map5PlayerPrefab;

    public GameObject NetworkManagerPrefab;

    private List<PlayerRef> _players { get; set; }
    private Dictionary<int, PlayerRef> _playerTurnOrder { get; set; }
    private NetworkManager _networkManager;
    [HideInInspector] public int LastTurn = 0;
    private bool _gameIsOver;
    private int _maxTurns = 2;

    // Cards
    private ComboRepo _combos = new ComboRepo();

    #region RPC
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    internal void RPC_ClaimCombo(PlayerBehaviour playerBehaviour, Combo combo)
    {
        Debug.Log("RPC_ClaimCombo hit");
        if (!playerBehaviour.HasCombo && IsPlayerTurn(playerBehaviour.Id.ToString()))
        {
            Debug.Log("Serverside verification OK");
            Debug.Log($"Serverside combos: {string.Join(", ", Combos)}");
            Debug.Log($"Requested combo: {combo.Id}");

            var serverCombo = Combos.FirstOrDefault(x => x.Id == combo.Id.ToString());
            var comboIndex = Combos.IndexOf(serverCombo);
            if (comboIndex == -1) return;
            Debug.Log("Combo exists");

            // Apply coins to existing combos
            var i = 0;
            foreach (var existingCombo in Combos)
            {
                if (existingCombo.Id == combo.Id)
                    break;

                var c = existingCombo;
                c.CoinsPlaced++;
                Combos.Set(i, c);
                i++;
            }

            serverCombo.Claimed = true;
            Combos.Set(comboIndex, serverCombo);
            playerBehaviour.ActiveCombo = serverCombo;
            playerBehaviour.HasCombo = true;

            // Calculate cost of combo
            var comboCost = comboIndex;
            playerBehaviour.Coins -= comboCost;
            playerBehaviour.Coins += serverCombo.CoinsPlaced; // Collect coins previously placed on combo

            var tokenStack = new TokenStack
            {
                Power = serverCombo.Power,
                Race = serverCombo.Race,
                Count = serverCombo.TotalTokens,
                Team = playerBehaviour.Team,
                Interactable = true,
                OwnerId = playerBehaviour.Id
            };
            playerBehaviour.Tokens.Add(serverCombo.Race.Name, tokenStack);

            RefillCombos();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    internal void RPC_Action(PlayerBehaviour playerBehaviour)
    {
        // TODO Probably need more checks
        if (IsPlayerTurn(playerBehaviour.Id.ToString()))
        {
            Debug.Log($"[SERVER] Action clicked. Player {PlayerTurn} turn on turn {Turn}. Turn stage: {TurnStage}");
            if (TurnStage == TurnState.Conquer)
            {
                TurnStage = TurnState.Redeploy;
                UndeployPlayerTokens(playerBehaviour);
            }
            else if (TurnStage == TurnState.Redeploy)
            {
                var ownedMapAreas = MapAreas
                    .Where(x => x.Value.OccupyingForce.OwnerId == playerBehaviour.Id)
                    .Select(x => x.Value)
                    .OrderBy(x => x.ConquerOrder);
                var delay = 0f;
                var coinsEarned = 0;
                foreach (var ownedMapArea in ownedMapAreas)
                {
                    var value = CoinCalculator.CalculatePlayerMapAreaCoins(playerBehaviour, ownedMapArea);
                    StartCoroutine(DistributeAreaCoinsToPlayer(playerBehaviour, ownedMapArea, value, delay));
                    delay += 2f;
                    coinsEarned += value;
                }
                coinsEarned += CoinCalculator.CalculateBonusCoins(playerBehaviour, Turn, ownedMapAreas);
                StartCoroutine(ShowTotalRoundCoins(delay, playerBehaviour, coinsEarned));
                delay += 3f;
                StartCoroutine(DelayedTurnIncrement(delay));
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    internal void RPC_Decline(PlayerBehaviour playerBehaviour)
    {
        if (!IsPlayerTurn(playerBehaviour.Id.ToString())) return;

        foreach (var mapArea in MapAreas)
        {
            if (mapArea.Value.OccupyingForce.OwnerId == playerBehaviour.Id)
            {
                var occupyingForce = mapArea.Value.OccupyingForce;
                occupyingForce.Count = 1;
                occupyingForce.InDecline = true;
                mapArea.Value.OccupyingForce = occupyingForce;
                MapAreas.Set(mapArea.Key, mapArea.Value);
            }
        }

        playerBehaviour.HasCombo = false;
        playerBehaviour.Tokens.Clear();
        IncrementPlayerTurn();
        Debug.Log("[SERVER] RPC_Decline called");
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    internal void RPC_CollectToken(PlayerBehaviour playerBehaviour, NetworkString<_4> areaId)
    {
        var mapArea = MapAreas.Get(areaId);
        if (playerBehaviour.HasCombo &&
            IsPlayerTurn(playerBehaviour.Id.ToString()) &&
            mapArea.OccupyingForce.OwnerId == playerBehaviour.Id && 
            mapArea.OccupyingForce.Count > 1)
        {
            var mapTokens = mapArea.OccupyingForce;
            var singleToken = new TokenStack
            {
                Count = 1,
                InPlay = true,
                Interactable = true,
                OwnerId = mapTokens.OwnerId,
                Power = mapTokens.Power,
                Race = mapTokens.Race,
                Team = mapTokens.Team
            };
            playerBehaviour.RPC_PickUpToken(singleToken);
            playerBehaviour.Tokens.Add(singleToken.Race.Name, singleToken);
            mapTokens.Count--;
            mapArea.OccupyingForce = mapTokens;
            MapAreas.Set(areaId, mapArea);

            playerBehaviour.RPC_RefreshActiveTokenStack();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    internal void RPC_ConquerArea(PlayerBehaviour playerBehaviour, NetworkString<_4> areaId, TokenStack token)
    {
        Debug.Log($"[SERVER] Attempting to conquer {areaId}: HasCombo({playerBehaviour.HasCombo}), IsOwnTurn({IsPlayerTurn(Id.ToString())})");
        var tokenKey = token.Race.Name.ToString();
        if (playerBehaviour.HasCombo && 
            IsPlayerTurn(playerBehaviour.Id.ToString()) && 
            playerBehaviour.Tokens.TryGet(tokenKey, out var playerToken))
        {
            var mapArea = MapAreas.Get(areaId);
            var tokensForSuccess = ConflictResolver.TokensForConquest(playerToken, mapArea);
            var validAreaToConquer = AreaResolver.CanUseArea(playerBehaviour, mapArea, TurnStage);
            var isOwnedArea = mapArea.OccupyingForce.OwnerId == playerBehaviour.Id; // Can't conquer own area

            if (playerToken.Count < tokensForSuccess || !validAreaToConquer || isOwnedArea)
                return;

            playerToken.Count = playerToken.Count - tokensForSuccess;
            playerBehaviour.Tokens.Set(tokenKey, playerToken);

            mapArea.OccupyingForce = new TokenStack
            {
                Count = tokensForSuccess,
                Interactable = false,
                Power = playerBehaviour.ActiveCombo.Power,
                Race = playerToken.Race,
                Team = playerBehaviour.Team,
                InPlay = true,
                OwnerId = playerToken.OwnerId
            };
            mapArea.IsOccupied = true;
            mapArea.ConquerOrder = playerBehaviour.ConquerOrderIx;
            mapArea.ConqueredThisTurn = true;
            playerBehaviour.ConquerOrderIx++;
            MapAreas.Set(areaId, mapArea);

            if (playerToken.Count <= 0)
                playerBehaviour.Tokens.Remove(tokenKey);

            playerBehaviour.HasTokensInPlay = true;
            playerBehaviour.RPC_RefreshActiveTokenStack();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    internal void RPC_RedeployToArea(PlayerBehaviour playerBehaviour, NetworkString<_4> areaId, TokenStack token)
    {
        Debug.Log($"[SERVER] Player {playerBehaviour.Name} redeploying units {token.Race.Name} to area {areaId}");
        var tokenKey = token.Race.Name.ToString();
        var mapArea = MapAreas.Get(areaId);
        if (IsPlayerTurn(playerBehaviour.Id.ToString()) &&
            mapArea.OccupyingForce.OwnerId == playerBehaviour.Id &&
            playerBehaviour.Tokens.TryGet(tokenKey, out var playerToken))
        {
            var occupyingForce = mapArea.OccupyingForce;
            occupyingForce.Count += 1;
            //occupyingForce.Interactable = false;
            mapArea.OccupyingForce = occupyingForce;
            MapAreas.Set(areaId, mapArea);

            playerToken.Count -= 1;
            playerBehaviour.Tokens.Set(tokenKey, playerToken);

            if (playerToken.Count <= 0)
                playerBehaviour.Tokens.Remove(tokenKey);

            playerBehaviour.RPC_RefreshActiveTokenStack();
        }
    }

    #endregion

    #region Network Stuff
    private void EnsureNetworkManager()
    {
        if (_networkManager == null)
            _networkManager = Instantiate(NetworkManagerPrefab).GetComponent<NetworkManager>();
    }

    public void StartHost()
    {
        EnsureNetworkManager();
        _networkManager.HostGame();
    }

    public void StartJoin()
    {
        EnsureNetworkManager();
        _networkManager.JoinGame();
    }
    #endregion

    public void StartGame()
    {
        if (Runner != null && Runner.IsServer)
        {
            InitialiseGame();
        }
    }

    public void Disconnect()
    {
        Runner.Shutdown();
    }

    private void InitialiseGame()
    {
        Debug.Log($"StartGame IsServer: {Runner.IsServer}");
        if (!Runner.IsServer) return;
        PlayerCount = Runner.ActivePlayers.Count();
        Turn = 0;
        _players = Runner.ActivePlayers.ToList();
        DistributeCoins(7);
        GeneratePlayerTurnOrder();
        GenerateCards();
        SpawnMap();
        State = GameState.GameStarted;
        StartCoroutine(DelayedTurnIncrement(0.5f));
    }

    private void DistributeCoins(int amount)
    {
        foreach (var player in _players)
        {
            GetPlayerBehaviour(player).Coins = amount;
        }
    }

    private IEnumerator DistributeAreaCoinsToPlayer(PlayerBehaviour player, MapArea area, int value, float delay)
    {
        yield return new WaitForSeconds(delay);
        player.RPC_AwardAreaTokens(area.Id, value);
        foreach (var p in _players)
        {
            var b = GetPlayerBehaviour(p);
            if (b.Id != player.Id)
                b.RPC_NotifyAreaTokens(area.Id, value);
        }
    }

    private void GenerateCards()
    {
        var combos = _combos.GetCombos(6);
        Debug.Log($"[SERVER] Generated combos: {string.Join(',', combos.Select(x => $"{x.Power.Name} {x.Race.Name}"))}");
        var i = 0;
        foreach (var combo in combos)
        {
            Combos.Set(i, combo);
            i++;
        }

        Debug.Log($"{_combos.powerRepo.AvailablePowers.Count} powers left: {string.Join(',', _combos.powerRepo.AvailablePowers.Select(x => x.Name))}");
    }

    private void GeneratePlayerTurnOrder()
    {
        _playerTurnOrder = new Dictionary<int, PlayerRef>();

        var nums = Enumerable.Range(0, _players.Count).ToArray();
        for (var i = 0; i < nums.Length; ++i)
        {
            var randomIndex = UnityEngine.Random.Range(0, nums.Length);
            var temp = nums[randomIndex];
            nums[randomIndex] = nums[i];
            nums[i] = temp;
        }

        var playerTurn = 0;
        foreach (var i in nums)
        {
            _playerTurnOrder.Add(playerTurn, _players[i]);
            playerTurn++;
        }
    }

    private void SpawnMap()
    {
        Debug.Log("Spawning map");
        NetworkObject networkMap = Runner.Spawn(Map2PlayerPrefab);
        var i = 0;
        foreach (var area in networkMap.GetComponentsInChildren<MapArea>())
        {
            area.Id = $"{i}_{Guid.NewGuid().ToString().Replace("-", "").Substring(6)}";
            MapAreas.Add(area.Id, area);
            i++;
        }
    }

    private IEnumerator ShowTotalRoundCoins(float delay, PlayerBehaviour player, int coinsEarned)
    {
        yield return new WaitForSeconds(delay);
        player.Coins += coinsEarned;
        foreach (var p in _players)
        {
            var b = GetPlayerBehaviour(p);
            b.RPC_NotifyTotalRoundTokens(coinsEarned);
        }
    }
    
    private IEnumerator DelayedTurnIncrement(float delay)
    {
        yield return new WaitForSeconds(delay);
        IncrementPlayerTurn();
    }

    private void IncrementPlayerTurn()
    {
        if (!Runner.IsServer) return;

        //LastTurn = Turn;
        var tempTurn = Turn;
        var tempPlayerTurn = PlayerTurn;

        if (tempTurn == 0)
        {
            tempTurn = 1;
            tempPlayerTurn = 0;
        }
        else
        {
            tempPlayerTurn++;
            if (tempPlayerTurn >= _playerTurnOrder.Count)
            {
                tempTurn++;
                tempPlayerTurn = 0;
            }
        }

        if (tempTurn > _maxTurns)
        {
            TriggerEndOfTurn();
            return;
        }

        var playerTurn = _playerTurnOrder[tempPlayerTurn];
        var player = GetPlayerBehaviour(playerTurn);
        UndeployPlayerTokens(player);

        PlayerTurnId = player.Id;
        TurnStage = TurnState.Conquer;
        Turn = tempTurn;
        PlayerTurn = tempPlayerTurn;

        // Reset map area stats
        foreach (var area in MapAreas)
        {
            var newArea = area.Value;
            newArea.ConqueredThisTurn = false;
            newArea.WasOccupied = area.Value.IsOccupied;
            MapAreas.Set(area.Key, newArea);
        }

        Debug.Log($"[SERVER] Player turn order: {string.Join(", ", _playerTurnOrder.Keys)}");
        Debug.Log($"[SERVER] Incremented player turn to {PlayerTurn} ({PlayerTurnId}) - {TurnStage}");
    }

    private void TriggerEndOfTurn()
    {
        if (_gameIsOver) return;
        _gameIsOver = true;
        foreach (var player in _players)
        {
            GetPlayerBehaviour(player).RPC_NotifyEndOfGame();
        }
    }

    // Takes all but one (per map area) of a players deployed tokens and returns them to their hand
    private void UndeployPlayerTokens(PlayerBehaviour playerBehaviour)
    {
        var mapAreas = MapAreas.Where(x => x.Value.OccupyingForce.OwnerId == playerBehaviour.Id);
        var redeployTokens = new Dictionary<NetworkString<_16>, TokenStack>();
        foreach (var area in mapAreas)
        {
            var occupyingForce = area.Value.OccupyingForce;
            if (redeployTokens.TryGetValue(occupyingForce.Race.Name, out var tokens))
            {
                tokens.Count += occupyingForce.Count - 1;
                redeployTokens[occupyingForce.Race.Name] = tokens;
            }
            else
            {
                var newTokens = new TokenStack
                {
                    Count = occupyingForce.Count - 1,
                    Race = occupyingForce.Race,
                    Interactable = true,
                    OwnerId = occupyingForce.OwnerId,
                    Power = occupyingForce.Power,
                    Team = occupyingForce.Team
                };
                redeployTokens[area.Value.OccupyingForce.Race.Name] = newTokens;
            }

            occupyingForce.Count = 1;
            area.Value.OccupyingForce = occupyingForce;
            MapAreas.Set(area.Key, area.Value);
        }

        foreach (var token in redeployTokens)
        {
            if (playerBehaviour.Tokens.TryGet(token.Key, out var tokens))
            {
                tokens.Count += token.Value.Count;
                playerBehaviour.Tokens.Set(token.Key, tokens);
            }
            else
            {
                playerBehaviour.Tokens.Add(token.Key, token.Value);
            }
        }
    }

    private void RefillCombos()
    {
        var claimedCombo = Combos.First(x => x.Claimed);
        var claimedIndex = Combos.IndexOf(claimedCombo);
        Debug.Log($"Combo '{claimedCombo}' claimed at index {claimedIndex}");
        for (var i = claimedIndex; i < Combos.Length; i++)
        {
            if (i == Combos.Length - 1)
            {
                var newCard = _combos.GetCombos(1)[0];
                Combos.Set(i, newCard);
            }
            else
            {
                Combos.Set(i, Combos[i + 1]);
            }
        }
    }

    private PlayerBehaviour GetPlayerBehaviour(PlayerRef player)
    {
        if (player == null) return null;
        var obj = Runner.GetPlayerObject(player);
        if (obj == null) return null;
        return obj.GetComponent<PlayerBehaviour>();
    }

    public PlayerBehaviour GetCurrentPlayerTurn()
    {
        return GetPlayerBehaviour(Runner.ActivePlayers.FirstOrDefault(x => GetPlayerBehaviour(x).Id == PlayerTurnId));
    }

    public bool IsPlayerTurn(string id)
    {
        return PlayerTurnId == id;
    }

    private static void TurnChanged(Changed<GameLogic> changed)
    {
        Debug.Log($"TurnChanged");
        Utility.UiUpdateRequired(true, true);
    }

    private static void PlayerTurnChanged(Changed<GameLogic> changed)
    {
        Debug.Log($"PlayerTurnChanged");
        Utility.UiUpdateRequired(false, true);
        /*changed.LoadNew();
        var thisTurn = changed.Behaviour.Turn;
        var lastTurn = changed.Behaviour.LastTurn;
        Debug.Log($"thisTurn: {thisTurn}, lastTurn: {lastTurn}");
        if (lastTurn != thisTurn)
        {
            if (thisTurn == 1) // Delay the first turn to account for loading etc
                changed.Behaviour.StartCoroutine(changed.Behaviour.DelayedAction(3f, () => Utility.UiUpdateRequired(true, true)));
            else
                Utility.UiUpdateRequired(true, true);
        }
        else
        {
            Utility.UiUpdateRequired(false, true);
        }*/
    }

    private static void UiUpdateRequired(Changed<GameLogic> changed)
    {
        Utility.UiUpdateRequired();
    }

    IEnumerator DelayedAction(float delay, UnityAction action)
    {
        yield return new WaitForSeconds(delay);
        action.Invoke();
    }

    public int GetPlayerCount()
    {
        return FindObjectsOfType<PlayerBehaviour>().Count(); // This is rubbish but I can't figure out how to have the non-hosts get the player count
    }

    public int GetComboCost(Combo combo)
    {
        var comboIndex = Combos.IndexOf(combo);
        var cost = -comboIndex;
        cost += combo.CoinsPlaced;
        return cost;
    }

    #region DebugUi Hooks

    public string GetState()
    {
        if (Runner == null) return "N/A";
        return State.ToString();
    }

    public string GetTurn()
    {
        if (Runner == null) return "N/A";
        return Turn.ToString();
    }

    public string GetTurnStage()
    {
        if (Runner == null) return "N/A";
        return TurnStage.ToString();
    }

    public string GetPlayerTurnIndex()
    {
        if (Runner == null) return "N/A";
        return PlayerTurn.ToString();
    }

    public string GetPlayerTurnId()
    {
        if (Runner == null) return "N/A";
        var turnId = PlayerTurnId.ToString();
        return string.IsNullOrEmpty(turnId) ? "N/A" : turnId;
    }

    public string GetPlayerTurnName()
    {
        if (Runner == null) return "N/A";
        var turnName = GetCurrentPlayerTurn()?.Name.ToString() ?? "N/A";
        return string.IsNullOrEmpty(turnName) ? "N/A" : turnName;
    }

    public string GetAvailableComboCount()
    {
        if (Runner == null) return "N/A";
        return Combos.Count().ToString();
    }

    public string GetMapAreaCount()
    {
        if (Runner == null) return "N/A";
        return MapAreas.Count().ToString();
    }

    public string GetPlayerCountStr()
    {
        if (Runner == null) return "N/A";
        return GetPlayerCount().ToString();
    }


    #endregion
}
