using Assets.Helper;
using Assets.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class DebugUi : MonoBehaviour
{
    public Transform EntryContainer;
    public GameObject SectionHeaderPrefab;
    public GameObject EntryPrefab;

    private bool _visible = true;
    private GameLogic _gameLogic;
    private PlayerBehaviour _localPlayer;
    private List<UnityAction> _propertyUpdaters = new List<UnityAction>();

    void Start()
    {
        _gameLogic = FindObjectOfType<GameLogic>();
        _localPlayer = Utility.FindLocalPlayer();

        ConfigureHeader("GameLogic");
        ConfigureEntry("State", () => _gameLogic.GetState());
        ConfigureEntry("Turn", () => _gameLogic.GetTurn());
        ConfigureEntry("TurnStage", () => _gameLogic.GetTurnStage());
        ConfigureEntry("PlayerTurnIndex", () => _gameLogic.GetPlayerTurnIndex());
        ConfigureEntry("PlayerTurnId", () => _gameLogic.GetPlayerTurnId());
        ConfigureEntry("PlayerTurnName", () => _gameLogic.GetPlayerTurnName());
        ConfigureEntry("Combos (count)", () => _gameLogic.GetAvailableComboCount());
        ConfigureEntry("MapAreas (count)", () => _gameLogic.GetMapAreaCount());
        ConfigureEntry("Players (count)", () => _gameLogic.GetPlayerCountStr());

        ConfigureHeader("PlayerBehaviour (local)");
        ConfigureEntry("ATS (count)", () => GetPlayerActiveTokenStack().HasValue ? GetPlayerActiveTokenStack().Value.Count.ToString() : "N/A");
        ConfigureEntry("ATS (race)", () => GetPlayerActiveTokenStack().HasValue ? GetPlayerActiveTokenStack().Value.Race.Name.ToString() : "N/A");
        ConfigureEntry("HasUsedReinforcementDice", () => _localPlayer?.HasUsedReinforcementDice.ToString());
        ConfigureEntry("HoveredAreaConquerCost", () => _localPlayer?.HoveredAreaConquerCost.ToString());
        ConfigureEntry("CanUseReinforcementDice", () => _localPlayer?.CanUseReinforcementDice.ToString());
        ConfigureEntry("HoveredAreaMinDiceRoll", () => _localPlayer?.HoveredAreaMinDiceRoll.ToString());

        if (!Debug.isDebugBuild)
            Disable();
    }

    private TokenStack? GetPlayerActiveTokenStack()
    {
        if (_localPlayer == null || !_localPlayer.ActiveTokenStack.HasValue) return null;
        return _localPlayer.ActiveTokenStack;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
            ToggleVisibility();

        if (!_visible) return;

        if (_localPlayer == null)
            _localPlayer = Utility.FindLocalPlayer();

        foreach (var updater in _propertyUpdaters)
        {
            updater.Invoke();
        }
    }

    void ConfigureHeader(string title)
    {
        var sectionHeader = Instantiate(SectionHeaderPrefab, EntryContainer);
        sectionHeader.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = title;
    }

    void ConfigureEntry(string key, Func<string> valueUpdater)
    {
        var entry = Instantiate(EntryPrefab, EntryContainer);
        _propertyUpdaters.Add(() => SetEntry(entry, key, valueUpdater.Invoke()));
    }

    void SetEntry(GameObject entry, string key, string value)
    {
        entry.transform.Find("Key").GetComponent<TextMeshProUGUI>().text = $"{key}{new string('.', 100)}";
        entry.transform.Find("Val").GetComponent<TextMeshProUGUI>().text = value;
    }

    void ToggleVisibility()
    {
        var targetVisibility = !_visible;
        _visible = targetVisibility;
        transform.Find("Wrapper").gameObject.SetActive(targetVisibility);
    }

    void Disable()
    {
        _visible = false;
        transform.Find("Wrapper").gameObject.SetActive(false);
    }
}
