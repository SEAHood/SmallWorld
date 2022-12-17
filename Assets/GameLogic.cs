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

    [Networked(OnChanged = nameof(UiUpdateRequired))] public GameState State { get; set; }
    [Networked(OnChanged = nameof(UiUpdateRequired))] public int Turn { get; set; }
    [Networked(OnChanged = nameof(UiUpdateRequired))] public TurnState TurnStage { get; set; }
    [Networked(OnChanged = nameof(UiUpdateRequired))] public int PlayerTurn { get; set; } // A players turn within a game turn
    [Networked] public NetworkString<_128> PlayerTurnId { get; set; } // A players turn within a game turn

    // This may need to be a NetworkLinkedList or NetworkArray for order to work
    [Networked(OnChanged = nameof(UiUpdateRequired)), Capacity(6)] public NetworkDictionary<NetworkString<_128>, Card> Cards => default;
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
    private CardRepo _cards = new CardRepo();

/*
    private GameObject _map;

    private float lastInvoked;
    private float interval = 1f;
    private int lastPlayerTurnIndex;
*/

    // Vars for FixedNetworkUpdate - monitors turn changes and controls who's turn it is
    private int _lastPlayerTurn { get; set; }

    private void Awake()
    {
        //_map = GameObject.Find("Map");
        //State = GameState.PlayersJoining;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    internal void RPC_ClaimCombo(PlayerBehaviour playerBehaviour, Card combo)
    {
        Debug.Log("RPC_ClaimCombo hit");
        if (!playerBehaviour.HasCombo && IsPlayerTurn(playerBehaviour.Id.ToString()))
        {
            Debug.Log("Serverside verification OK");
            Debug.Log($"Serverside combos: {string.Join(", ", Cards.Select(x => x.Key))}");
            Debug.Log($"Requested combo: {combo.Id}");

            var comboExists = Cards.TryGet(combo.Id.ToString(), out var serverCombo);
            if (!comboExists) return;
            Debug.Log("Combo exists");

            Cards.Remove(combo.Id.ToString());
            playerBehaviour.ActiveCombo = combo;
            playerBehaviour.HasCombo = true;

            var tokenStack = new TokenStack
            {
                Power = combo.Power,
                Race = combo.Race,
                Count = combo.TotalTokens,
                Team = playerBehaviour.Team,
                Interactable = true,
                OwnerId = playerBehaviour.Id
            };
            playerBehaviour.Tokens.Add(combo.Race.Name, tokenStack);

            var newCard = _cards.GetCards(1)[0];
            Cards.Add(newCard.Id, newCard);
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
            else if (TurnStage == TurnState.Redeploy)
            {
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

            if (playerToken.Count < tokensForSuccess)
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

    public void StartGame()
    {
        if (Runner != null && Runner.IsServer)
        {
            InitialiseGame();
        }
    }

    public override void FixedUpdateNetwork()
    {
    }

    private void InitialiseGame()
    {
        Debug.Log($"StartGame IsServer: {Runner.IsServer}");
        if (!Runner.IsServer) return;
        PlayerCount = Runner.ActivePlayers.Count();
        Turn = 0;
        _players = Runner.ActivePlayers.ToList();
        GeneratePlayerTurnOrder();
        GenerateCards();
        SpawnMap();
        IncrementPlayerTurn();
        State = GameState.GameStarted;
        Utility.UiUpdateRequired();
    }

    private void GenerateCards()
    {
        var cards = _cards.GetCards(6);
        Debug.Log(string.Join(',', cards.Select(x => $"{x.Power.Name} {x.Race.Name}")));
        var i = 0;
        foreach (var card in cards)
        {
            Cards.Add(card.Id.ToString(), card);
            i++;
        }

        Debug.Log($"{_cards.powerRepo.AvailablePowers.Count} powers left: {string.Join(',', _cards.powerRepo.AvailablePowers.Select(x => x.Name))}");
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
        PlayerTurnId = player.Id;
        Debug.Log($"[SERVER] Player turn order: {string.Join(", ", _playerTurnOrder.Keys)}");
        Debug.Log($"[SERVER] Incremented player turn to {PlayerTurn} ({PlayerTurnId}) - {TurnStage}");
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
