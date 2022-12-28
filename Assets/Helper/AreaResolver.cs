using System.Linq;
using UnityEngine;

namespace Assets.Helper
{
    public static class AreaResolver
    {
        public static bool CanUseArea(PlayerBehaviour player, MapArea mapArea, GameLogic.TurnState turnStage)
        {
            Debug.Log($"[CLIENT] CanUseArea: {mapArea.Biome}");

            if (turnStage == GameLogic.TurnState.Conquer)
            {
                if (mapArea.OccupyingForce.OwnerId == player.Id) // Can't conquer own land
                    return false;

                if (mapArea.Biome == MapArea.AreaBiome.Sea)
                    return player.ActiveCombo.Power.Name == "Seafaring";

                if (player.HasTokensInPlay)
                {
                    var adjacentAreas = mapArea.AdjacentAreas;

                    // Add caverns as adjacent areas if player has Underworld power
                    if (player.ActiveCombo.Power.Name == "Underworld" && mapArea.HasCavern)
                        adjacentAreas.AddRange(GameObject.FindObjectsOfType<MapArea>().Where(x => x.Id != mapArea.Id && x.HasCavern).ToList());

                    foreach (var area in mapArea.AdjacentAreas)
                    {
                        // TODO: Handle powers and races here, i.e. triton, dragon on area, etc
                        if (area.OccupyingForce.OwnerId == player.Id)
                            return true;
                    }

                    return false;
                }
                else
                {
                    return mapArea.IsBorderArea;
                }
            }
            else if (turnStage == GameLogic.TurnState.Redeploy)
            {
                return mapArea.OccupyingForce.OwnerId == player.Id;
            }
            else
            { 
                return false; // This won't happen
            }
        }
    }
}
