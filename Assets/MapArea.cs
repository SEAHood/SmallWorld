using Assets.Enum;
using Assets.Helper;
using Assets.Model;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapArea : NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
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
    public GameObject TokenStackPrefab;
    public bool HasCavern;
    public bool HasMine;
    public bool HasMagic;
    public bool IsLostTribeSpawn;
    public bool IsBorderArea;
    public bool ConqueredThisTurn;
    public bool WasOccupied;

    [Networked(OnChanged = nameof(OccupyingForceChanged))] public TokenStack OccupyingForce { get; set; }
    [Networked] public NetworkBool IsOccupied { get; set; }
    [Networked] public NetworkString<_4> Id { get; set; }
    [Networked] public int ConquerOrder { get; set; }

    private Color _color;
    private Image _goodBorder;
    private Image _badBorder;
    private Image _highlight;
    private Transform _tokenPosition;
    private TokenStackUi _instantiatedToken;
    private bool _isHovered;

    private void Awake()
    {
        _highlight = transform.Find("Highlight").GetComponent<Image>();
        _goodBorder = transform.Find("GoodBorder").GetComponent<Image>();
        _badBorder = transform.Find("BadBorder").GetComponent<Image>();

        GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;

        _tokenPosition = transform.Find("TokenPosition");
        _tokenPosition.localScale = new Vector3(1.9f, 1.9f, 1.9f);

        _highlight.enabled = false;
        _goodBorder.enabled = false;
        _badBorder.enabled = false;
        RefreshMapArea();
    }

    private static void OccupyingForceChanged(Changed<MapArea> changed)
    {
        changed.Behaviour.RefreshMapArea();
    }

    public void RefreshMapArea()
    {
        if (_instantiatedToken == null)
            _instantiatedToken = Instantiate(TokenStackPrefab, _tokenPosition).GetComponent<TokenStackUi>();

        if (Runner != null)
        {
            if (IsOccupied)
            {
                _instantiatedToken.Populate(OccupyingForce);
            }
            else if (IsLostTribeSpawn)
            {
                //Utility.ClearTransform(_tokenPosition);
                var token = new TokenStack
                {
                    Race = new Race { Name = "LostTribe" },
                    Count = 1,
                    Interactable = false,
                    Team = Team.None,
                    OwnerId = null
                };
                _instantiatedToken.Populate(token);
                OccupyingForce = token;
                IsOccupied = true;
            }
        }

        if (_isHovered)
        {
            OnPointerEnter(new PointerEventData(FindObjectOfType<EventSystem>()));
        }
    }

    public void ResetColour()
    {
        GetComponent<SpriteRenderer>().color = _color;
    }

    public void Highlight(bool enable)
    {
        _highlight.enabled = enable;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;
        foreach (var a in AdjacentAreas)
        {
            //a.Highlight(true);
        }

        var player = Utility.FindLocalPlayer();
        var tokens = player.ActiveTokenStack;
        var gameLogic = FindObjectOfType<GameLogic>();
        _highlight.enabled = false;
        _goodBorder.enabled = false;
        _badBorder.enabled = false;
        if (tokens == null || tokens.Value.Count <= 0 || gameLogic.TurnStage == GameLogic.TurnState.Redeploy)
        {
            // No tokens in use or in redeploy, just highlight
            _highlight.enabled = true;
        }
        else
        {
            var tokensToConquer = ConflictResolver.TokensForConquest(tokens.Value, this);
            var validAreaToConquer = AreaResolver.CanUseArea(player, this, gameLogic.TurnStage);
            if (tokens.Value.Count < tokensToConquer || !validAreaToConquer)
            {
                // Not enough tokens to conquer
                _badBorder.enabled = true;
                player.HoveredAreaConquerCost = 0;
                Debug.Log($"Setting player.HoveredAreaConquestCost to 0");
            }
            else
            {
                // Enough to conquer
                _goodBorder.enabled = true;
                player.HoveredAreaConquerCost = tokensToConquer;
                Debug.Log($"Setting player.HoveredAreaConquestCost to {tokensToConquer}");
            }
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        foreach (var a in AdjacentAreas)
        {
            //a.Highlight(false);            
        }

        var player = Utility.FindLocalPlayer();
        player.HoveredAreaConquerCost = 0;
        Debug.Log($"Setting player.HoveredAreaConquestCost to 0");
        _highlight.enabled = false;
        _goodBorder.enabled = false;
        _badBorder.enabled = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Utility.FindLocalPlayer().TryAffectMapArea(this);
    }
}
