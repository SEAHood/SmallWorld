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
    public TextMeshProUGUI TurnText;
    public Button DeclineButton;
    public Button RedeployButton;
    public Transform AvailableCombos;
    public Transform Tokens;

    public GameObject PlayerSlotPrefab;
    public GameObject ComboPrefab;
    public GameObject TokenStackPrefab;

    public GameLogic GameLogic;

    private PlayerBehaviour _localPlayer;

    public void Initialise()
    {
        _localPlayer = Utility.FindLocalPlayer();
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
            SetPlayerCombo(_localPlayer);
            SetPlayerTokens(_localPlayer);
            SetOtherPlayers();
            SetAvailableCombos();
        }
    }

    private void SetLocalPlayer(PlayerBehaviour localPlayer)
    {
        if (localPlayer != null)
            LocalPlayerSlot.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = $"{localPlayer.Name}";
        else
            LocalPlayerSlot.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = "???";
    }

    private void SetOtherPlayers()
    {

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
        Utility.ClearTransform(Tokens);
        var tokenStack = Instantiate(TokenStackPrefab, Tokens);
        var sb = new StringBuilder();
        foreach (var token in localPlayer.Tokens)
        {
            sb.AppendLine($"{token.Value} {token.Key}");
        }
        tokenStack.GetComponent<TextMeshProUGUI>().text = sb.ToString();
    }


    void Update()
    {
    }
}
