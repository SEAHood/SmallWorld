using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TotalCoinBehaviour : MonoBehaviour
{
    public TextMeshProUGUI CoinValueText;

    private float _timeToGrow = 0.3f;
    private float _timeToWait = 2f;
    private float _timeToShrink = 0.3f;
    private Vector3 _originalScale;

    void Start()
    {
    }


    IEnumerator StartAnimate()
    {
        for (float t = 0; t < 1; t += Time.deltaTime / _timeToGrow)
        {
            transform.localScale = Vector3.Lerp(Vector3.zero, _originalScale, t);
            yield return null;
        }
        StartCoroutine(Wait());
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(_timeToWait);
        StartCoroutine(Shrink());
    }

    IEnumerator Shrink()
    {
        for (float t = 0; t < 1; t += Time.deltaTime / _timeToShrink)
        {
            transform.localScale = Vector3.Lerp(_originalScale, Vector3.zero, t);
            yield return null;
        }
        Destroy(gameObject);
    }

    public void Initialise(int value)
    {
        CoinValueText.text = $"+{value}";
        _originalScale = transform.localScale;
        StartCoroutine(StartAnimate());
    }
}
