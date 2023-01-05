using Assets.Enum;
using Assets.Model;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSlotUi : MonoBehaviour
{
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI ComboText;
    public Image AvatarImage;
    public Image BannerImage;

    public void Populate(string name, Combo? combo, Team team, bool isTurnActive)
    {
        NameText.text = name;
        ComboText.text = combo.HasValue ? $"{combo}" : "";
        AvatarImage.sprite = Resources.Load<Sprite>($"Avatars/Avatar{team}3");
        BannerImage.sprite = Resources.Load<Sprite>($"Avatars/Banner{team}{(isTurnActive ? "Glow" : "")}");
    }
}
