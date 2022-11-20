using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaHighlighter : MonoBehaviour
{
    void OnMouseEnter()
    {
        GetComponent<SpriteRenderer>().color = Color.red;
    }

    void OnMouseExit()
    {
        GetComponent<SpriteRenderer>().color = Color.white;
    }
}
