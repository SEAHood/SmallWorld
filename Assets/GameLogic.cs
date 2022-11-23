using Assets.Helper;
using Assets.Model;
using Assets.Repo;
using Fusion;
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

    [Networked] public GameState State { get; set; }
    [Networked] public int Turn { get; set; }
    [Networked] public int PlayerTurn { get; set; } // A players turn within a game turn
    //[Networked, Capacity(6)] public NetworkArray<Card> Cards => default;
    [Networked(OnChanged = nameof(UiUpdateRequired)), Capacity(6)] public NetworkDictionary<NetworkString<_128>, Card> Cards => default;
    [Networked] public int PlayerCount { get; set; }

    public bool IsNetworkActive => Runner != null;

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

    [Rpc]
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
            playerBehaviour.Tokens.Add(combo.Race.Name, combo.TotalTokens);

            var newCard = _cards.GetCards(1)[0];
            Cards.Add(newCard.Id, newCard);
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
        Turn = 1;
        PlayerTurn = 1;
        _players = Runner.ActivePlayers.ToList();
        GeneratePlayerTurnOrder();
        GenerateCards();
        State = GameState.GameStarted;
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
            var randomIndex = Random.Range(0, nums.Length);
            var temp = nums[randomIndex];
            nums[randomIndex] = nums[i];
            nums[i] = temp;
        }

        var playerTurn = 1;
        foreach (var i in nums)
        {
            _playerTurnOrder.Add(playerTurn, _players[i]);
            playerTurn++;
        }
    }

    private PlayerBehaviour GetPlayerBehaviour(PlayerRef player)
    {
        var obj = Runner.GetPlayerObject(player);
        return obj.GetComponent<PlayerBehaviour>();
    }

    public bool IsPlayerTurn(string id)
    {
        var playerTurn = _playerTurnOrder[PlayerTurn];
        var player = GetPlayerBehaviour(playerTurn);
        return player.Id == id;
    }

    private static void UiUpdateRequired(Changed<GameLogic> changed)
    {
        Utility.UiUpdateRequired();
    }
}
