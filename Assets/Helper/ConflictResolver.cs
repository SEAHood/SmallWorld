using Assets.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Helper
{
    public static class ConflictResolver
    {
        public static int TokensForConquest(TokenStack playerTokens, MapArea mapArea)
        {
            /*2 Race tokens +1 additional Race token for each Encampment,
            Fortress, Mountain, or Troll's Lair marker + 1 additional Race token
            for each Lost Tribe or other player's Race token*/

            var cost = 2;
            cost += mapArea.OccupyingForce.Count;
            // TODO Add HasEncampmentToken, HasFortressToken, HasMountainToken, HasTrollLairToken to MapArea

            #region Powers

            if (playerTokens.Power.Name == "Mounted" && mapArea.Biome == MapArea.AreaBiome.Hills)
                cost -= 1;

            if (playerTokens.Power.Name == "Commando")
                cost -= 1;

            #endregion

            #region Races

            if (playerTokens.Race.Name == "Giant" && mapArea.HasAdjacentBiome(MapArea.AreaBiome.Mountain))
                cost -= 1;

            if (playerTokens.Race.Name == "Triton" && mapArea.HasAdjacentBiome(MapArea.AreaBiome.Sea))
                cost -= 1;

            #endregion


            return Math.Max(1, cost); // Minimum cost is 1 token
        }
    }
}
