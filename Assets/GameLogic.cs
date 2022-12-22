using Assets.Helper;
using Assets.Model;
using Assets.Repo;
using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    [Networked(OnChanged = nameof(UiUpdateRequired))] public int Turn { get; set; } // Turn index (points to _playerTurnOrder)
    [Networked(OnChanged = nameof(UiUpdateRequired))] public TurnState TurnStage { get; set; }
    [Networked(OnChanged = nameof(UiUpdateRequired))] public int PlayerTurn { get; set; } // A players turn within a game turn
    [Networked] public NetworkString<_128> PlayerTurnId { get; set; } // A players turn within a game turn
    [Networked(OnChanged = nameof(UiUpdateRequired)), Capacity(6)] public NetworkArray<Combo> Combos => default;
    [Networked(OnChanged = nameof(UiUpdateRequired)), Capacity(30)] public NetworkDictionary<NetworkString<_4>, MapArea> MapAreas => default;
    [Networked] public int PlayerCount { get; set; }

    public bool IsNetworkActive => Runner != null;

    public GameObject Map2PlayerPrefab;
    public GameObject Map3PlayerPrefab;
    public GameObject Map4PlayerPrefab;
    public GameObject Map5PlayerPrefab;

    private List<PlayerRef> _players { get; set; }
    private Dictionary<int, PlayerRef> _playerTurnOrder { get; set; }

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

            // Decrement cost of combo
            var comboCost = comboIndex;
            playerBehaviour.Coins -= comboCost;

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
                var coins = CoinCalculator.CalculateEndOfTurnCoins(playerBehaviour);
                playerBehaviour.Coins += coins;
                // TODO - some kind of wait/RPC for showing the coin animation at some point
                IncrementPlayerTurn();
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    internal void RPC_Decline(PlayerBehaviour playerBehaviour)
    {
        if (IsPlayerTurn(playerBehaviour.Id.ToString()))
        {
            Debug.Log("[SERVER] RPC_Decline called");
        }
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
            var validAreaToConquer = AreaResolver.CanUseArea(playerBehaviour, mapArea);
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

    public void StartGame()
    {
        if (Runner != null && Runner.IsServer)
        {
            InitialiseGame();
        }
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
        IncrementPlayerTurn();
        State = GameState.GameStarted;
        Utility.UiUpdateRequired();
    }

    private void DistributeCoins(int amount)
    {
        foreach (var player in _players)
        {
            GetPlayerBehaviour(player).Coins = amount;
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

    private void IncrementPlayerTurn()
    {
        if (!Runner.IsServer) return;

        if (Turn == 0)
        {
            Turn = 1;
            PlayerTurn = 0;
        }
        else
        {
            PlayerTurn++;
            if (PlayerTurn >= _playerTurnOrder.Count)
            {
                Turn++;
                PlayerTurn = 0;
            }
        }
        TurnStage = TurnState.Conquer;
        var playerTurn = _playerTurnOrder[PlayerTurn];
        var player = GetPlayerBehaviour(playerTurn);
        UndeployPlayerTokens(player);
        PlayerTurnId = player.Id;
        Debug.Log($"[SERVER] Player turn order: {string.Join(", ", _playerTurnOrder.Keys)}");
        Debug.Log($"[SERVER] Incremented player turn to {PlayerTurn} ({PlayerTurnId}) - {TurnStage}");
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
        var obj = Runner.GetPlayerObject(player);
        return obj.GetComponent<PlayerBehaviour>();
    }

    public bool IsPlayerTurn(string id)
    {
        return PlayerTurnId == id;
    }

    private static void UiUpdateRequired(Changed<GameLogic> changed)
    {
        Utility.UiUpdateRequired();
    }

    public int GetPlayerCount()
    {
        return FindObjectsOfType<PlayerBehaviour>().Count(); // This is rubbish but I can't figure out how to have the non-hosts get the player count
    }
}
