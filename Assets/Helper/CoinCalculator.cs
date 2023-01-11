using System;
using System.Collections.Generic;
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
                coins += 1;
            }
            return coins;
        }

        public static int CalculatePlayerMapAreaCoins(PlayerBehaviour player, MapArea area)
        {
            var coins = 1;
            var tokenRace = area.OccupyingForce.Race.Name.ToString();
            var tokenPower = area.OccupyingForce.Power.Name.ToString();

            if (!area.OccupyingForce.InDecline)
            {
                // Race area modifiers
                switch (tokenRace)
                {
                    case "Dwarf" when area.HasMine:
                        coins += 1;
                        break;
                    case "Human" when area.Biome == MapArea.AreaBiome.Farm:
                        coins += 1;
                        break;
                    case "Wizard" when area.HasMagic:
                        coins += 1;
                        break;
                    case "Orc" when area.ConqueredThisTurn && area.WasOccupied:
                        coins += 1;
                        break;
                    default:
                        break;
                }

                // Power area modifiers
                switch (tokenPower)
                {
                    case "Forest" when area.Biome == MapArea.AreaBiome.Forest:
                        coins += 1;
                        break;
                    case "Hill" when area.Biome == MapArea.AreaBiome.Hills:
                        coins += 1;
                        break;
                    case "Swamp" when area.Biome == MapArea.AreaBiome.Swamp:
                        coins += 1;
                        break;
                    case "Pillaging" when area.ConqueredThisTurn && area.WasOccupied:
                        coins += 1;
                        break;
                    case "Merchant":
                        coins += 1;
                        break;
                    default:
                        break;
                }
            }
            else 
            {
                // Tokens are in decline
                if (tokenRace == "Dwarf" && area.HasMine)
                    coins += 1;
            }

            return coins;
        }

        public static int CalculateBonusCoins(PlayerBehaviour player, int turn, IEnumerable<MapArea> ownedAreas)
        {
            var coins = 0;

            if (player.ActiveCombo.Power.Name == "Wealthy" && turn == 1) // TODO this needs to be first turn you HAVE the combo, not turn 1
                coins += 7;

            if (player.ActiveCombo.Power.Name == "Alchemist" && ownedAreas.Count() > 0)
                coins += 2;

            return coins;
        }
    }
}
