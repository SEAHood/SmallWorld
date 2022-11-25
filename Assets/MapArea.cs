using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    public enum AreaBiome
    {
        Sea,
        Mountain,
        Swamp,
        Forest,
        Hills,
        Farm
    }


    public List<MapArea> AdjacentAreas;
    public AreaBiome Biome;


    private Color _color;

    private SpriteRenderer _goodBorder;
    private SpriteRenderer _badBorder;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _goodBorder = transform.Find("GoodBorder").GetComponent<SpriteRenderer>();
        _badBorder = transform.Find("BadBorder").GetComponent<SpriteRenderer>();
        _spriteRenderer.enabled = false;
        _goodBorder.enabled = false;
        _badBorder.enabled = false;
    }

    void OnMouseEnter()
    {
        _spriteRenderer.enabled = true;
        _goodBorder.enabled = true;
        //GetComponent<SpriteRenderer>().color = new Color(_color.r, _color.g, _color.b, 0.5f);
        /*foreach (var area in AdjacentAreas)
        {
            area.GetComponent<SpriteRenderer>().color = Color.white;
        }*/
    }

    void OnMouseDown()
    {
        Debug.Log($"{Biome} Clicked!");
    }

    void OnMouseExit()
    {
        _spriteRenderer.enabled = false;
        _goodBorder.enabled = false;
        /*ResetColour();
        foreach (var area in AdjacentAreas)
        {
            area.ResetColour();
        }*/
    }

    public void ResetColour()
    {
        GetComponent<SpriteRenderer>().color = _color;
    }
}
