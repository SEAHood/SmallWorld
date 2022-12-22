using Assets.Helper;
using Assets.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ComboPanelUi : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image PowerImage;
    public Image RaceImage;
    public Combo Combo;
    public Transform Coin;
    public TextMeshProUGUI CoinValue;

    public UnityEvent<Combo> OnClicked = new UnityEvent<Combo>();

    private Vector3 _originalScale;
    private GameUi _gameUi;

    void Awake()
    {
        _originalScale = transform.localScale;
        _gameUi = FindObjectOfType<GameUi>();
    }

    public void Populate(Combo combo)
    {
        Combo = combo;
        PowerImage.sprite = Resources.Load<Sprite>($"PowerCards/{combo.Power.Name}Power");
        RaceImage.sprite = Resources.Load<Sprite>($"RaceCards/{combo.Race.Name}Race");
        Coin.gameObject.SetActive(!combo.Claimed && combo.CoinsPlaced > 0);
        CoinValue.text = combo.CoinsPlaced.ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //if (Combo.Claimed) return;
        Debug.Log($"Clicked combo {Combo.Power.Name} {Combo.Race.Name}");
        OnClicked.Invoke(Combo);
        /*var localPlayer = Utility.FindLocalPlayer();
        localPlayer.TryAcquireCombo(Combo);*/
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Combo.Claimed) return;
        transform.localScale = _originalScale * 1.2f;
        _gameUi.ApplyTempComboCoins(this);

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Combo.Claimed) return;
        transform.localScale = _originalScale;
        _gameUi.RemoveTempComboCoins();
    }
}
