using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameLogic : NetworkBehaviour
{
    public enum GameState
    {
        One,
        Two,
        Three
    }
    [Networked] public GameState State { get; set; }

    private GameObject _map;

    private float lastInvoked;
    private float interval = 1f;
    private int lastPlayerTurnIndex;


    private void Awake()
    {
        _map = GameObject.Find("Map");
    }

    public override void FixedUpdateNetwork()
    {
        if (Runner.IsServer)
        {
            if (lastInvoked + interval < Time.fixedTime)
            {
                foreach (var player in Runner.ActivePlayers)
                {
                    var obj = Runner.GetPlayerObject(player);
                    obj.GetComponent<PlayerBehaviour>().IsTurnActive = false;
                }
                var playerIndex = lastPlayerTurnIndex + 1;
                if (playerIndex > Runner.ActivePlayers.Count())
                    playerIndex = 0;
                lastPlayerTurnIndex = playerIndex;
                Runner.GetPlayerObject(Runner.ActivePlayers.ElementAt(playerIndex)).GetComponent<PlayerBehaviour>().IsTurnActive = true;

                State = (GameState)Random.Range(0, 3);
                lastInvoked = Time.fixedTime;
            }

        }
    }
}
