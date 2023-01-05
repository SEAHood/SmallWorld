using Assets.Helper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndScreenUi : MonoBehaviour
{
    public Transform LeaderboardContainer;
    public GameObject PlayerLeaderboardEntryPrefab;

    public void Populate(IEnumerable<PlayerBehaviour> leaderboard)
    {
        LeaderboardContainer.Clear();
        var position = 1;
        foreach (var player in leaderboard)
        {
            var entry = Instantiate(PlayerLeaderboardEntryPrefab, LeaderboardContainer);
            entry.GetComponent<PlayerLeaderboardEntryUi>().Populate(position, player);
            position++;
        }
    }
}
