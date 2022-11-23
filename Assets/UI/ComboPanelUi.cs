using Assets.Helper;
using Assets.Model;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ComboPanelUi : MonoBehaviour, IPointerClickHandler
{
    public Image PowerImage;
    public Image RaceImage;

    private Card _combo;

    public void Populate(Card combo)
    {
        _combo = combo;
        PowerImage.sprite = Resources.Load<Sprite>($"PowerCards/{combo.Power.Name}Power");
        RaceImage.sprite = Resources.Load<Sprite>($"RaceCards/{combo.Race.Name}Race");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Clicked combo {_combo.Power.Name} {_combo.Race.Name}");
        var localPlayer = Utility.FindLocalPlayer();
        localPlayer.TryAcquireCombo(_combo);
    }
}
