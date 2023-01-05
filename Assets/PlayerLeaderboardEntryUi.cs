using Assets.Helper;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLeaderboardEntryUi : MonoBehaviour
{
    public GameObject CrownImage;
    public TextMeshProUGUI PlacingText;
    public PlayerSlotUi PlayerSlot;
    public TextMeshProUGUI CoinsText;

    public void Populate(int position, PlayerBehaviour player)
    {
        CrownImage.SetActive(position == 1);
        PlacingText.text = position.ToOrdinal();
        PlayerSlot.Populate(player.Name.ToString(), player.ActiveCombo, player.Team, false);
        CoinsText.text = player.Coins.ToString();
    }
}
