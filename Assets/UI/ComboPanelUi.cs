using Assets.Helper;
using Assets.Model;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ComboPanelUi : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image PowerImage;
    public Image RaceImage;

    private Card _combo;
    private Vector3 _originalScale;

    void Awake()
    {
        _originalScale = transform.localScale;
    }

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

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = _originalScale * 1.2f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = _originalScale;
    }
}
