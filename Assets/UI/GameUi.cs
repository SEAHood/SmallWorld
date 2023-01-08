using Assets.Helper;
using Assets.Model;
using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameUi : MonoBehaviour
{
    public List<Transform> PlayerSlots;
    public Transform LocalPlayerSlot;
    public Transform LocalPlayerCombo;
    public Transform Player2Slot;
    public Transform Player3Slot;
    public Transform Player4Slot;
    public Transform Player5Slot;
    public TextMeshProUGUI TurnText;
    public Button DeclineButton;
    public Button ActionButton;
    public Transform AvailableCombos;
    public Transform Tokens;
    public TextMeshProUGUI CoinsText;
    public Transform CoinTarget;
    public Transform TotalCoinsAnchor;
    public GameObject MouseOverCoin;
    public TextMeshProUGUI MouseOverCoinValue;
    public TextGrower CenterTextGrower;

    public GameObject PlayerSlotPrefab;
    public GameObject ComboPrefab;
    public GameObject TokenStackPrefab;
    public GameObject CoinPrefab;
    public GameObject TotalCoinsPrefab;

    public GameLogic GameLogic;
    public DescriptionUi DescriptionUi;
    public EndScreenUi EndScreenUi;

    private PlayerBehaviour _localPlayer;
    private List<PlayerBehaviour> _otherPlayers;
    private bool _actionInProgress;

    public void Initialise()
    {
        _localPlayer = Utility.FindLocalPlayer();
        _otherPlayers = Utility.FindOtherPlayers();
        if (_localPlayer == null) throw new Exception("[CRITICAL] There is no local player, cannot proceed with GameUi initialisation");
        DescriptionUi.gameObject.SetActive(false);
        EndScreenUi.gameObject.SetActive(false);
    }

    public void RefreshUi(bool newTurn, bool newPlayerTurn)
    {
        if (_localPlayer == null)
            Initialise();

        SetLocalPlayer(_localPlayer);
        SetOtherPlayers();
        SetPlayerCombo(_localPlayer);
        SetPlayerTokens(_localPlayer);
        SetButtons(_localPlayer);
        SetTurnText();
        SetOtherPlayers();
        SetAvailableCombos();

        if (newTurn)
        {
            CenterTextGrower.ShowText(GameLogic.Turn == 1 ? "Game Start" : "New Turn", () => CenterTextGrower.ShowText($"{GameLogic.GetCurrentPlayerTurn().Name}", null));
        }
        else if (newPlayerTurn)
        {            
            CenterTextGrower.ShowText($"{GameLogic.GetCurrentPlayerTurn().Name}", null);
        }
    }

    private void SetLocalPlayer(PlayerBehaviour localPlayer)
    {
        var gameLogic = FindObjectOfType<GameLogic>();
        LocalPlayerSlot.GetComponent<PlayerSlotUi>().Populate(localPlayer.Name.ToString(), localPlayer.ActiveCombo, localPlayer.Team, 
                                                              gameLogic.IsPlayerTurn(localPlayer.Id.ToString()));
        CoinsText.text = localPlayer.Coins.ToString();
    }

    private void SetOtherPlayers()
    {
        // oh my god
        var player2 = _otherPlayers.ElementAtOrDefault(0);
        var player3 = _otherPlayers.ElementAtOrDefault(1);
        var player4 = _otherPlayers.ElementAtOrDefault(2);
        var player5 = _otherPlayers.ElementAtOrDefault(3);

        if (Player2Slot != null)
        {
            if (player2 != null)
            {
                var playerSlotUi = Player2Slot.GetComponent<PlayerSlotUi>();
                playerSlotUi.Populate(player2.Name.ToString(), player2.ActiveCombo, player2.Team, GameLogic.IsPlayerTurn(player2.Id.ToString()));
            }
            else
                Player2Slot.gameObject.SetActive(false);
        }
        if (Player3Slot != null)
        {
            if (player3 != null)
            {
                var playerSlotUi = Player3Slot.GetComponent<PlayerSlotUi>();
                playerSlotUi.Populate(player3.Name.ToString(), player3.ActiveCombo, player3.Team, GameLogic.IsPlayerTurn(player3.Id.ToString()));
            }
            else
                Player3Slot.gameObject.SetActive(false);
        }
        if (Player4Slot != null)
        {
            if (player4 != null)
            {
                var playerSlotUi = Player4Slot.GetComponent<PlayerSlotUi>();
                playerSlotUi.Populate(player4.Name.ToString(), player4.ActiveCombo, player4.Team, GameLogic.IsPlayerTurn(player4.Id.ToString()));
            }
            else
                Player2Slot.gameObject.SetActive(false);
        }
        if (Player5Slot != null)
        {
            if (player5 != null)
            {
                var playerSlotUi = Player5Slot.GetComponent<PlayerSlotUi>();
                playerSlotUi.Populate(player5.Name.ToString(), player5.ActiveCombo, player5.Team, GameLogic.IsPlayerTurn(player5.Id.ToString()));
            }
            else
                Player5Slot.gameObject.SetActive(false);
        }
    }

    private void SetButtons(PlayerBehaviour localPlayer)
    {
        _actionInProgress = false;
        var isOwnTurn = GameLogic.IsPlayerTurn(localPlayer.Id.ToString());
        var actionImage = ActionButton.GetComponent<Image>();
        var actionText = ActionButton.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        var declineImage = DeclineButton.GetComponent<Image>();
        if (isOwnTurn)
        {
            switch (GameLogic.TurnStage)
            {
                case GameLogic.TurnState.Conquer:
                    actionImage.sprite = Resources.Load<Sprite>("Buttons/ActionRedeploy");
                    actionText.text = "Redeploy";
                    break;
                case GameLogic.TurnState.Redeploy:
                    actionImage.sprite = Resources.Load<Sprite>("Buttons/ActionDone");
                    actionText.text = "Done";
                    break;
                default:
                    actionImage.sprite = Resources.Load<Sprite>("Buttons/ActionRedeploy");
                    actionText.text = "Redeploy";
                    break;
            }
            declineImage.sprite = Resources.Load<Sprite>("Buttons/DeclineOn");
        }
        else
        {
            actionImage.sprite = Resources.Load<Sprite>("Buttons/ActionDisabled");
            declineImage.sprite = Resources.Load<Sprite>("Buttons/DeclineOff");
            actionText.text = "";
        }
    }

    private void SetTurnText()
    {
        TurnText.text = $"{GameLogic.Turn}/10";
    }

    private void SetAvailableCombos()
    {
        var localPlayer = Utility.FindLocalPlayer();
        AvailableCombos.Clear();
        foreach (var combo in GameLogic.Combos)
        {
            var comboPanel = Instantiate(ComboPrefab, AvailableCombos);
            var comboPanelUi = comboPanel.GetComponent<ComboPanelUi>();
            comboPanelUi.Populate(combo);
            comboPanelUi.OnClicked.AddListener(HandleComboClick);
            if (localPlayer.HasCombo)
                comboPanelUi.Disable();
            else
                comboPanelUi.Enable();

        }
    }

    private void HandleComboClick(Combo combo)
    {
        DescriptionUi.gameObject.SetActive(true);
        DescriptionUi.Populate(combo);
    }

    public void ShowMouseOverCoin(int value)
    {
        MouseOverCoin.SetActive(true);
        var cost = value < 0 ? value.ToString() : $"+{value}";
        MouseOverCoinValue.text = cost;
    }

    public void HideMouseOverCoin()
    {
        MouseOverCoin.SetActive(false);
    }

    public void ShowCoinAnimation(MapArea area, int value, bool ownCoins, UnityAction callbackWhenDone)
    {
        var coin = Instantiate(CoinPrefab, transform);
        coin.transform.position = area.transform.position;
        coin.GetComponent<CoinUi>().Initialise(value, CoinTarget.position, ownCoins, callbackWhenDone);
    }

    public void ShowTotalRoundCoins(int value)
    {
        var coins = Instantiate(TotalCoinsPrefab, TotalCoinsAnchor);
        coins.GetComponent<TotalCoinBehaviour>().Initialise(value);
    }

    public void ApplyTempComboCoins(ComboPanelUi comboPanelUi)
    {
        var combos = AvailableCombos.GetComponentsInChildren<ComboPanelUi>();

        var selectedComboReached = false;
        foreach (var combo in combos)
        {
            if (combo == comboPanelUi)
                selectedComboReached = true;

            var activeCombo = combo.Combo;
            activeCombo.CoinsPlaced = GameLogic.Combos.First(x => x.Id == activeCombo.Id).CoinsPlaced;
            if (!selectedComboReached)
                activeCombo.CoinsPlaced += 1;

            combo.Populate(activeCombo);
        }
    }

    public void RemoveTempComboCoins()
    {
        var combos = AvailableCombos.GetComponentsInChildren<ComboPanelUi>();
        foreach (var combo in combos)
        {
            var activeCombo = combo.Combo;
            activeCombo.CoinsPlaced = GameLogic.Combos.First(x => x.Id == activeCombo.Id).CoinsPlaced;

            combo.Populate(activeCombo);
        }
    }

    private void SetPlayerCombo(PlayerBehaviour localPlayer)
    {
        LocalPlayerCombo.Clear();
        if (localPlayer.HasCombo)
        {
            var combo = localPlayer.ActiveCombo;
            var comboPanel = Instantiate(ComboPrefab, LocalPlayerCombo);
            comboPanel.GetComponent<ComboPanelUi>().Populate(combo);
            comboPanel.GetComponent<ComboPanelUi>().OnClicked.AddListener(HandleComboClick);
        }
    }

    private void SetPlayerTokens(PlayerBehaviour localPlayer)
    {
        foreach (Transform token in Tokens)
        {
            var tokenUi = token.GetComponent<TokenStackUi>();
            if (localPlayer.Tokens.TryGet(tokenUi.Race, out var playerToken))
            {
                var playerTokenCount = playerToken.Count;
                if (playerTokenCount > 0) 
                {
                    tokenUi.Token = playerToken;
                    tokenUi.Count = playerTokenCount;
                }
                else
                    Destroy(tokenUi.gameObject);
            }
            else
                Destroy(tokenUi.gameObject);
        }

        var tokenUis = Tokens.Cast<Transform>().Select(x => x.GetComponent<TokenStackUi>());
        foreach (var token in localPlayer.Tokens)
        {
            if (tokenUis.Any(x => x.Race == token.Key.ToString()))
                continue;
            
            var tokenStack = Instantiate(TokenStackPrefab, Tokens).GetComponent<TokenStackUi>();
            tokenStack.Populate(token.Value);
        }
    }

    public void CreateMouseAttachedTokenStack(TokenStack token)
    {
        var tokenStack = Instantiate(TokenStackPrefab, Tokens).GetComponent<TokenStackUi>();
        tokenStack.Populate(token);
        tokenStack.AttachToMouse();
    }

    public void ShowEndOfGame()
    {
        var leaderboard = FindObjectsOfType<PlayerBehaviour>().OrderByDescending(x => x.Coins);
        EndScreenUi.gameObject.SetActive(true);
        EndScreenUi.Populate(leaderboard);
    }

    public void ActionClicked()
    {
        if (!_actionInProgress)
        {
            _actionInProgress = true;
            ActionButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Buttons/ActionDisabled");
            _localPlayer.TryAction();
        }
    }

    public void DeclineClicked()
    {
        _localPlayer.TryDecline();
    }
}
