using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBehaviour : NetworkBehaviour
{
    [Networked] public bool IsTurnActive { get; set; }
    [Networked, Capacity(2)] public NetworkDictionary<NetworkString<_16>, int> Tokens => default; 

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
            foreach(var x in Tokens)
            {
                Debug.Log($"Token: {x.Key}, {x.Value}");
            }
            Debug.Log($"Cards: {string.Join(", ", FindObjectOfType<GameLogic>().Cards.Select(x => $"{x.Power.Name} {x.Race.Name}"))}");
        }
    }
}
