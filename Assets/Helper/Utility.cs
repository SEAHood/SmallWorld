using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Helper
{
    public static class Utility
    {
        public static PlayerBehaviour FindLocalPlayer()
        {
            return GameObject.FindObjectsOfType<PlayerBehaviour>().FirstOrDefault(x => x.IsLocal());
        }

        public static PlayerBehaviour FindPlayerWithId(NetworkString<_64> id)
        {
            return GameObject.FindObjectsOfType<PlayerBehaviour>().FirstOrDefault(x => x.Id == id);
        }

        public static List<PlayerBehaviour> FindOtherPlayers()
        {
            return GameObject.FindObjectsOfType<PlayerBehaviour>().Where(x => !x.IsLocal()).ToList();
        }

        public static void UiUpdateRequired(bool newTurn = false, bool newPlayerTurn = false)
        {
            Debug.Log("UI update required...");
            foreach (var area in GameObject.FindObjectsOfType<MapArea>())
            {
                area.RefreshMapArea();
            }

            GameObject.FindObjectOfType<UiManager>().RefreshUi(newTurn, newPlayerTurn);
        }
    }
}
