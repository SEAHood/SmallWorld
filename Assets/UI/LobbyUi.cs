using Assets.Helper;
using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUi : MonoBehaviour
{
    public Transform Players;
    public GameObject StartButton;
    public GameObject BackButton;
    public GameObject WaitingText;
    public GameObject PlayerPanelPrefab;

    public GameLogic GameLogic;

    void Start()
    {
    }


    internal void Initialise(bool isHost)
    {
        StartButton.GetComponent<Button>().onClick.AddListener(StartClicked);
        StartButton.SetActive(isHost);
        BackButton.GetComponent<Button>().onClick.AddListener(BackClicked);
        WaitingText.SetActive(!isHost);

        InvokeRepeating("UpdatePlayers", 0f, 1f);
    }

    void StartClicked()
    {
        GameLogic.StartGame();
    }

    void BackClicked()
    {
        GameLogic.Disconnect();
    }

    public void UpdatePlayers()
    {
        foreach (Transform child in Players)
        {
            Destroy(child.gameObject);
        }

        foreach (var player in FindObjectsOfType<PlayerBehaviour>())
        {
            var playerPanel = Instantiate(PlayerPanelPrefab, Players);
            playerPanel.GetComponent<PlayerSlotUi>().Populate(player.Name.ToString(), null, player.Team, false);
        }
    }
}
