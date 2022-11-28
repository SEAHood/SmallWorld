using Fusion;
using System.Linq;
using UnityEngine;

namespace Assets.Helper
{
    public static class Utility
    {
        public static void ClearTransform(Transform t)
        {
            foreach (Transform child in t)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        public static PlayerBehaviour FindLocalPlayer()
        {
            return GameObject.FindObjectsOfType<PlayerBehaviour>().FirstOrDefault(x => x.IsLocal());
        }

        public static PlayerBehaviour FindPlayerWithId(NetworkString<_128> id)
        {
            return GameObject.FindObjectsOfType<PlayerBehaviour>().FirstOrDefault(x => x.Id == id);
        }

        public static void UiUpdateRequired()
        {
            GameObject.FindObjectOfType<UiManager>().RefreshUi();

            foreach (var area in GameObject.FindObjectsOfType<MapArea>())
            {
                area.RefreshMapArea();
            }
        }
    }
}
