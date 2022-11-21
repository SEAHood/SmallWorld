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
    [Networked, Capacity(6)] public NetworkArray<Card> Cards => default;

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
        State = GameState.PlayersJoining;
    }
    
    private void OnGUI()
    {
        if (Runner != null && Runner.IsServer)
        {
            if (GUI.Button(new Rect(0, 40, 200, 40), "Start Game"))
            {
                StartGame();
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Runner.IsServer)
        {
            if (_lastPlayerTurn != PlayerTurn)
            {
                foreach (var player in _players)
                {
                    GetPlayerBehaviour(player).IsTurnActive = false;
                }
                var currentPlayer = _playerTurnOrder[PlayerTurn];
                GetPlayerBehaviour(currentPlayer).IsTurnActive = true;
                _lastPlayerTurn = PlayerTurn;
            }
        }
    }

    private void StartGame()
    {
        Debug.Log($"StartGame IsServer: {Runner.IsServer}");
        if (!Runner.IsServer) return;

        State = GameState.GameStarted;
        Turn = 1;
        PlayerTurn = 1;
        _players = Runner.ActivePlayers.ToList();
        GeneratePlayerTurnOrder();
        GenerateCards();
        TestTokens();
    }

    private void GenerateCards()
    {
        var cards = _cards.GetCards(6);
        Debug.Log(string.Join(',', cards.Select(x => $"{x.Power.Name} {x.Race.Name}")));
        var i = 0;
        foreach (var x in cards)
        {
            Cards.Set(i, x);
            i++;
        }

        Debug.Log($"{_cards.powerRepo.AvailablePowers.Count} powers left: {string.Join(',', _cards.powerRepo.AvailablePowers.Select(x => x.Name))}");
    }

    private void TestTokens() 
    {

        foreach (var x in _players)
        {
            var quantity = Random.Range(1,10);
            GetPlayerBehaviour(x).Tokens.Add("ratmen", quantity);
         
        }
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
}
