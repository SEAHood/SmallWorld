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

    private void Awake()
    {
        switch (Biome)
        {
            case AreaBiome.Sea:
                _color = Color.cyan;
                break;
            case AreaBiome.Mountain:
                _color = Color.gray;
                break;
            case AreaBiome.Swamp:
                _color = new Color(128f / 255f, 64f / 255f, 0f);
                break;
            case AreaBiome.Forest:
                _color = Color.green;
                break;
            case AreaBiome.Hills:
                _color = Color.red;
                break;
            case AreaBiome.Farm:
                _color = Color.yellow;
                break;
            default:
                _color = Color.white;
                break;
        }

        GetComponent<SpriteRenderer>().color = _color;
    }

    void OnMouseEnter()
    {
        GetComponent<SpriteRenderer>().color = new Color(_color.r, _color.g, _color.b, 0.5f);
        foreach (var area in AdjacentAreas)
        {
            area.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    void OnMouseDown()
    {
        Debug.Log($"{Biome} Clicked!");
    }

    void OnMouseExit()
    {
        ResetColour();
        foreach (var area in AdjacentAreas)
        {
            area.ResetColour();
        }
    }

    public void ResetColour()
    {
        GetComponent<SpriteRenderer>().color = _color;
    }
}
