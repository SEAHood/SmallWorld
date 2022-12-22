using Assets.Helper;
using Assets.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DescriptionUi : MonoBehaviour
{
    public TextMeshProUGUI PowerTitleText;
    public TextMeshProUGUI RaceTitleText;
    public Image PowerImage;
    public Image RaceImage;
    public Button PickButton;
    public Button CancelButton;

    internal void Populate(Combo combo)
    {
        var powerName = combo.Power.Name.ToString();
        var raceName = combo.Race.Name.ToString();

        PowerTitleText.text = combo.BeautifyPowerName();
        RaceTitleText.text = combo.BeautifyRaceName();
        PowerImage.sprite = Resources.Load<Sprite>($"Descriptions/Powers/{powerName}Power");
        RaceImage.sprite = Resources.Load<Sprite>($"Descriptions/Races/{raceName}Race");

        if (combo.Claimed || Utility.FindLocalPlayer().HasCombo)
        {
            PickButton.gameObject.SetActive(false);
        }
        else
        {
            PickButton.gameObject.SetActive(true);
            PickButton.onClick.RemoveAllListeners();
            PickButton.onClick.AddListener(() => OnPickClicked(combo));
        }

        CancelButton.onClick.RemoveAllListeners();
        CancelButton.onClick.AddListener(() => ClosePanel());
    }

    private void OnPickClicked(Combo combo)
    {
        if (combo.Claimed) return;
        var localPlayer = Utility.FindLocalPlayer();
        localPlayer.TryAcquireCombo(combo);
        ClosePanel();
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}
