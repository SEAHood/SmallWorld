using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TextGrower : MonoBehaviour
{
    private TextMeshProUGUI _text;
    private float _timeToGrow = 0.3f;
    private float _timeToWait = 2f;
    private float _timeToShrink = 0.3f;
    private Vector3 _originalScale;
    private bool _animating;

    void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _originalScale = transform.localScale;
        _text.enabled = false;
    }

    public void ShowText(string text, UnityAction onFinish)
    {
        if (_animating) return;
        _animating = true;
        _text.text = text;
        StartCoroutine(StartAnimate(onFinish));
    }

    IEnumerator StartAnimate(UnityAction onFinish)
    {
        transform.localScale = Vector3.zero;
        _text.enabled = true;
        for (float t = 0; t < 1; t += Time.deltaTime / _timeToGrow)
        {
            transform.localScale = Vector3.Lerp(Vector3.zero, _originalScale, t);
            yield return null;
        }
        StartCoroutine(Wait(onFinish));
    }

    IEnumerator Wait(UnityAction onFinish)
    {
        yield return new WaitForSeconds(_timeToWait);
        StartCoroutine(Shrink(onFinish));
    }

    IEnumerator Shrink(UnityAction onFinish)
    {
        for (float t = 0; t < 1; t += Time.deltaTime / _timeToGrow)
        {
            transform.localScale = Vector3.Lerp(_originalScale, Vector3.zero, t);
            yield return null;
        }
        _text.enabled = false;
        _animating = false;

        if (onFinish != null)
            onFinish.Invoke();
    }

}
