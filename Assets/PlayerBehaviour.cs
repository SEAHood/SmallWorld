using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBehaviour : NetworkBehaviour
{
    [Networked] public bool IsTurnActive { get; set; }

    public TextMeshPro TurnText;

    // Start is called before the first frame update
    void Start()
    {
        TurnText = GetComponent<TextMeshPro>();
        TurnText.enabled = HasInputAuthority;
    }

    // Update is called once per frame
    void Update()
    {
        if (HasInputAuthority)
        {
            TurnText.text = IsTurnActive ? "Your Turn" : "";
        }
    }
}
