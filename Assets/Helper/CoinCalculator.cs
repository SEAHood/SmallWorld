using System.Linq;
using UnityEngine;

namespace Assets.Helper
{
    public static class CoinCalculator
    {
        public static int CalculateEndOfTurnCoins(PlayerBehaviour player)
        {
            var ownedMapAreas = GameObject.FindObjectsOfType<MapArea>().Where(x => x.OccupyingForce.OwnerId == player.Id).ToList();
            var coins = 0;
            foreach (var area in ownedMapAreas)
            {
                // TODO Power/race calculations
                coins += 1;
            }
            return coins;
        }
    }
}
