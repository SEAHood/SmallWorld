namespace Assets.Helper
{
    public static class AreaResolver
    {
        public static bool CanUseArea(PlayerBehaviour player, MapArea mapArea)
        {
            if (player.HasTokensInPlay)
            {
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
    }
}
