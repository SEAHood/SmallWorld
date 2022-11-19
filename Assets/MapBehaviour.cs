using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapBehaviour : MonoBehaviour
{
    private TextMeshPro _text;
    private GameLogic _logic;

    // Start is called before the first frame update
    void Start()
    {
        _text = transform.Find("Text").GetComponent<TextMeshPro>();
        _logic = FindObjectOfType<GameLogic>();
    }

    // Update is called once per frame
    void Update()
    {
        _text.text = _logic.State.ToString();
    }
}
