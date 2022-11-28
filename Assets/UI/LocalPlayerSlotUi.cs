using Assets.Enum;
using Assets.Model;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocalPlayerSlotUi : MonoBehaviour
{
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI ComboText;
    public Image AvatarImage;
    public Image BannerImage;

    public void Populate(string name, Card? combo, Team team)
    {
        NameText.text = name;
        ComboText.text = combo.HasValue ? $"{combo}" : "nout lol";
        AvatarImage.sprite = Resources.Load<Sprite>($"Avatars/Avatar{team}");
        BannerImage.sprite = Resources.Load<Sprite>($"Avatars/Banner{team}");
    }
}
