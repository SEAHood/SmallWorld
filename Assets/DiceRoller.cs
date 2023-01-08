using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DiceRoller : MonoBehaviour
{
    public Image Dice;

    private Sprite _dice0;
    private Sprite _dice1;
    private Sprite _dice2;
    private Sprite _dice3;
    private Sprite[] _dice = new Sprite[4];
    private bool _hasLanded;

    // Rolling animation
    private int _diceAnimIndex;
    private float _timeLastChanged;
    public float _timeBetweenChanges = 0.1f;
    private float _timeToStop = 1f;
    private float _timeToWait = 1.5f;
    private float _timeToLeave = 0.5f;
    private Vector3 _centerPos;
    private Vector3 _rightPos;
    private Vector3 _leftPos;

    // Start is called before the first frame update
    void Start()
    {
        Dice.enabled = false;
        _centerPos = Dice.transform.position;
        _rightPos = new Vector3(Screen.width + 300, _centerPos.y, _centerPos.z);
        _leftPos = new Vector3(-300, _centerPos.y, _centerPos.z);
        _dice[0] = Resources.Load<Sprite>("Dice/dice0");
        _dice[1] = Resources.Load<Sprite>("Dice/dice1");
        _dice[2] = Resources.Load<Sprite>("Dice/dice2");
        _dice[3] = Resources.Load<Sprite>("Dice/dice3");
    }

    public void Initialise(int targetRoll)
    {
        _hasLanded = false;
        StartCoroutine(StartAnimate(targetRoll));
    }

    IEnumerator StartAnimate(int targetRoll)
    {
        Dice.transform.position = _rightPos;
        Dice.enabled = true;
        for (float t = 0; t < 1; t += Time.deltaTime / _timeToStop)
        {
            Dice.transform.position = Vector3.Lerp(_rightPos, _centerPos, t);
            yield return null;
        }
        StartCoroutine(RevealAndWait(targetRoll));
    }

    IEnumerator RevealAndWait(int targetRoll)
    {
        Dice.sprite = _dice[targetRoll];
        _hasLanded = true;
        yield return new WaitForSeconds(_timeToWait);
        StartCoroutine(LeaveScreen());
    }

    IEnumerator LeaveScreen()
    {
        for (float t = 0; t < 1; t += Time.deltaTime / _timeToLeave)
        {
            Dice.transform.position = Vector3.Lerp(_centerPos, _leftPos, t);
            yield return null;
        }
        Dice.enabled = true;
    }


    // Update is called once per frame
    void Update()
    {
        if (!_hasLanded && _timeLastChanged + _timeBetweenChanges < Time.fixedTime)
        {
            Dice.sprite = _dice[_diceAnimIndex];
            _diceAnimIndex++;
            if (_diceAnimIndex >= _dice.Count())
                _diceAnimIndex = 0;
            _timeLastChanged = Time.fixedTime;
        }
    }
}
