using Assets.Helper;
using Assets.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
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

    public GameObject PlayerSlotPrefab;
    public GameObject ComboPrefab;
    public GameObject TokenStackPrefab;

    public TextMeshProUGUI DebugTurn;

    public GameLogic GameLogic;

    private PlayerBehaviour _localPlayer;
    private List<PlayerBehaviour> _otherPlayers;

    public void Initialise()
    {
        _localPlayer = Utility.FindLocalPlayer();
        _otherPlayers = Utility.FindOtherPlayers();
        if (_localPlayer == null) throw new Exception("Critical: There is no local player, cannot proceed with GameUi initialisation");
        RefreshUi();
    }

    public void RefreshUi()
    {
        if (_localPlayer == null)
            Initialise();
        else
        {
            SetLocalPlayer(_localPlayer);
            SetOtherPlayers();
            SetPlayerCombo(_localPlayer);
            SetPlayerTokens(_localPlayer);
            SetButtons(_localPlayer);
            SetTurnText();
            SetOtherPlayers();
            SetAvailableCombos();
            DebugTurn.text = Utility.FindPlayerWithId(GameLogic.PlayerTurnId).GetComponent<PlayerBehaviour>().Name.ToString();
        }
    }

    private void SetLocalPlayer(PlayerBehaviour localPlayer)
    {
        /*if (localPlayer != null)
            LocalPlayerSlot.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = $"{localPlayer.Name}";
        else
            LocalPlayerSlot.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = "???";*/

        LocalPlayerSlot.GetComponent<PlayerSlotUi>().Populate(localPlayer.Name.ToString(), localPlayer.ActiveCombo, localPlayer.Team);
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
                Player2Slot.GetComponent<PlayerSlotUi>().Populate(player2.Name.ToString(), player2.ActiveCombo, player2.Team);
            else
                Player2Slot.gameObject.SetActive(false);
        }
        if (Player3Slot != null)
        {
            if (player3 != null)
                Player3Slot.GetComponent<PlayerSlotUi>().Populate(player3.Name.ToString(), player3.ActiveCombo, player3.Team);
            else
                Player3Slot.gameObject.SetActive(false);
        }
        if (Player4Slot != null)
        {
            if (player4 != null)
                Player4Slot.GetComponent<PlayerSlotUi>().Populate(player4.Name.ToString(), player4.ActiveCombo, player4.Team);
            else
                Player2Slot.gameObject.SetActive(false);
        }
        if (Player5Slot != null)
        {
            if (player5 != null)
                Player5Slot.GetComponent<PlayerSlotUi>().Populate(player5.Name.ToString(), player5.ActiveCombo, player5.Team);
            else
                Player5Slot.gameObject.SetActive(false);
        }
    }

    private void SetButtons(PlayerBehaviour localPlayer)
    {
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
        Utility.ClearTransform(AvailableCombos);
        foreach (var comboKv in GameLogic.Cards)
        {
            var combo = comboKv.Value;
            var comboPanel = Instantiate(ComboPrefab, AvailableCombos);
            comboPanel.GetComponent<ComboPanelUi>().Populate(combo);
        }
    }

    private void SetPlayerCombo(PlayerBehaviour localPlayer)
    {
        Utility.ClearTransform(LocalPlayerCombo);
        if (localPlayer.HasCombo)
        {
            var combo = localPlayer.ActiveCombo;
            var comboPanel = Instantiate(ComboPrefab, LocalPlayerCombo);
            comboPanel.GetComponent<ComboPanelUi>().Populate(combo);
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
                    tokenUi.Count = playerTokenCount;
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

    public void ActionClicked()
    {
        _localPlayer.TryAction();
    }

    public void DeclineClicked()
    {
        _localPlayer.TryDecline();
    }
}
