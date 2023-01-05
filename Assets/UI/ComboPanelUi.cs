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
    private GameLogic _gameLogic;
    private bool _enabled;

    void Awake()
    {
        _originalScale = transform.localScale;
        _gameUi = FindObjectOfType<GameUi>();
        _gameLogic = FindObjectOfType<GameLogic>();
    }

    public void Populate(Combo combo)
    {
        Combo = combo;
        PowerImage.sprite = Resources.Load<Sprite>($"PowerCards/{combo.Power.Name}Power");
        RaceImage.sprite = Resources.Load<Sprite>($"RaceCards/{combo.Race.Name}Race");
        Coin.gameObject.SetActive(!combo.Claimed && combo.CoinsPlaced > 0);
        CoinValue.text = combo.CoinsPlaced.ToString();
    }

    public void Enable()
    {
        _enabled = true;
        var c = Color.white;
        c.a = 1f;
        PowerImage.color = c;
        RaceImage.color = c;
        Coin.GetComponent<Image>().color = c;
        CoinValue.color = c;
    }

    public void Disable()
    {
        _enabled = false;
        var c = Color.white;
        c.a = 0.5f;
        PowerImage.color = c;
        RaceImage.color = c;
        Coin.GetComponent<Image>().color = c;
        CoinValue.color = c;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_enabled) return;
        //if (Combo.Claimed) return;
        Debug.Log($"Clicked combo {Combo.Power.Name} {Combo.Race.Name}");
        OnClicked.Invoke(Combo);
        /*var localPlayer = Utility.FindLocalPlayer();
        localPlayer.TryAcquireCombo(Combo);*/
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_enabled) return;
        if (Combo.Claimed) return;
        transform.localScale = _originalScale * 1.2f;

        var cost = _gameLogic.GetComboCost(Combo);
        _gameUi.ApplyTempComboCoins(this);
        _gameUi.ShowMouseOverCoin(cost);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_enabled) return;
        if (Combo.Claimed) return;
        transform.localScale = _originalScale;
        _gameUi.RemoveTempComboCoins();
        _gameUi.HideMouseOverCoin();
    }
}
