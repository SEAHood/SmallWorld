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
        Grassland,
        Prarie
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
            case AreaBiome.Grassland:
                _color = new Color(25f / 255f, 0, 100f/255f);
                break;
            case AreaBiome.Prarie:
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
            //area.GetComponent<SpriteRenderer>().color = Color.yellow;
        }
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
