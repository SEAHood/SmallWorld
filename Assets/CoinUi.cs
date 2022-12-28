using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CoinUi : MonoBehaviour
{
    public TextMeshProUGUI CoinValueText;
    private bool _active;
    private Vector3 _startPos;
    private Vector3 _targetPos;
    private Vector3 _originalScale;
    private bool _ownCoins;
    private float timeToGrow = 0.3f;
    private float timeToStartMoving = 1f;
    private float timeToReachTarget = 0.5f;
    private Coroutine _coroutine;
    private UnityAction _callback;

    public void Initialise(int coinValue, Vector3 target, bool ownCoins, UnityAction callbackWhenDone)
    {
        _startPos = transform.position;
        _targetPos = target;
        _originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        CoinValueText.text = coinValue.ToString();
        _callback = callbackWhenDone;
        _ownCoins = ownCoins;
        _active = true;
    }

    void Update()
    {
        if (!_active) return;
        if (_coroutine != null) return;
        _coroutine = StartCoroutine(StartAnimate());
    }

    IEnumerator StartAnimate()
    {
        //yield return new WaitForSeconds(Random.Range(0f, 0.7f));
        for (float t = 0; t < 1; t += Time.deltaTime / timeToGrow)
        {
            transform.localScale = Vector3.Lerp(Vector3.zero, _originalScale, t);
            yield return null;
        }
        StartCoroutine(Wait());
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(timeToStartMoving);
        if (_ownCoins)
            StartCoroutine(Move());
        else
            StartCoroutine(Shrink());
    }

    IEnumerator Move()
    {
        for (float t = 0; t < 1; t += Time.deltaTime / timeToReachTarget)
        {
            transform.position = Vector3.Lerp(_startPos, _targetPos, t);
            transform.localScale = Vector3.Lerp(_originalScale, new Vector3(0.3f, 0.3f, 0.3f), t);
            yield return null;
        }
        Finish();
    }

    IEnumerator Shrink()
    {
        for (float t = 0; t < 1; t += Time.deltaTime / timeToGrow)
        {
            transform.localScale = Vector3.Lerp(_originalScale, Vector3.zero, t);
            yield return null;
        }
        Finish();
    }

    private void Finish()
    {
        _callback.Invoke();
        Destroy(gameObject);
    }
}
