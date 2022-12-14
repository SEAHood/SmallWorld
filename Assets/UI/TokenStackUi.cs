using Assets.Enum;
using Assets.Helper;
using Assets.Model;
using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TokenStackUi : MonoBehaviour, IPointerDownHandler
{
    public Image MainToken;
    public Image TeamTag;
    public bool Interactable;
    public Transform Stack;
    public GameObject CountPanel;
    public TextMeshProUGUI CountText;
    public GameObject StackTokenTemplatePrefab;
    public TokenStackUi ConquerCostStack;
    public GameObject Dice;
    public bool IsConquestStack;

    public string Race;
    public Team Team;
    [Min(1)] public int Count;
    public PlayerBehaviour Owner;
    public TokenStack Token;
    public bool IsSpecialToken;

    private GameLogic _gameLogic;

    private string _lastRace;
    private int _lastCount;
    private int? _lastHoverCost;
    private Team _lastTeam;
    private bool _lastAttachedToMouse;
    private GameLogic.TurnState _lastTurnStage;

    private Vector3 _originalPosition;
    private bool _attachedToMouse;
    private bool _active;

    void Start()
    {
        _gameLogic = FindObjectOfType<GameLogic>();
        _originalPosition = transform.position;
        MainToken.enabled = false;
        TeamTag.enabled = false;
        CountPanel.SetActive(false);
        Stack.gameObject.SetActive(false);

        if (!IsConquestStack)
            Dice.SetActive(false);
    }

    void FixedUpdate()
    {
        if (!_active) return;

        if (Race != _lastRace ||
            Count != _lastCount || 
            Team != _lastTeam ||
            Owner?.HoveredAreaConquerCost != _lastHoverCost ||
            _attachedToMouse != _lastAttachedToMouse)
            Refresh();

        _lastRace = Race;
        _lastCount = Count;
        _lastTeam = Team;
        _lastHoverCost = Owner?.HoveredAreaConquerCost;
        _lastAttachedToMouse = _attachedToMouse;
    }

    void Update()
    {
        if (!Interactable) return;

        if (Input.GetMouseButtonDown(1) || _lastTurnStage != _gameLogic.TurnStage)
        {
            _attachedToMouse = false;
            GetComponent<Image>().raycastTarget = true;
            Owner.ActiveTokenStack = null;
        }
        _lastTurnStage = _gameLogic.TurnStage;

        if (_attachedToMouse)
            transform.position = Input.mousePosition;
        else
            transform.position = _originalPosition;
    }

    private void Refresh()
    {
        if (Count == 0)
        {
            MainToken.enabled = false;
            CountPanel.SetActive(false);
            TeamTag.enabled = false;
            Stack.gameObject.SetActive(false);
            if (ConquerCostStack != null)
                ConquerCostStack.gameObject.SetActive(false);
        }
        else
        {
            UpdateMainToken();
            UpdateCountText();
            UpdateTeamTag();
            UpdateStack();
            UpdateConquerCostStack();
            UpdateReinforcementDice();
            GenerateOffset();
        }
    }

    private void UpdateMainToken()
    {
        MainToken.enabled = true;
        MainToken.sprite = Token.InDecline ? Resources.Load<Sprite>($"Tokens/{Race}TokenInDecline") : Resources.Load<Sprite>($"Tokens/{Race}Token");
    }

    private void UpdateCountText()
    {
        CountPanel.SetActive(true);
        CountText.text = Count.ToString();
    }

    private void UpdateTeamTag()
    {
        if (Team != Team.None)
        {
            TeamTag.enabled = true;
            TeamTag.sprite = Resources.Load<Sprite>($"Tokens/Overlays/Overlay{Team}");
        }
        else
            TeamTag.enabled = false;
    }

    private void UpdateStack()
    {
        Stack.gameObject.SetActive(true);
        Stack.Clear();
        var stackHeight = Count <= 4 ? Count : 4;
        for (var i = 0; i < stackHeight - 1; i++)
        {
            var stackItem = Instantiate(StackTokenTemplatePrefab, Stack);
            stackItem.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Tokens/{Race}Token");
        }
        StartCoroutine(GenerateOffset());
    }

    private void UpdateConquerCostStack()
    {
        if (IsConquestStack) return;

        // If we're not conquering, don't bother
        if (ConquerCostStack != null && (!_attachedToMouse || FindObjectOfType<GameLogic>().TurnStage != GameLogic.TurnState.Conquer))
        {
            ConquerCostStack.Count = 0;
            return;
        }

        Debug.Log($"UpdateConquerCostStack(): {ConquerCostStack != null}, {Owner != null}, {Owner?.HoveredAreaConquerCost}");
        if (ConquerCostStack != null && Owner != null)
        {
            ConquerCostStack.Race = Race;
            ConquerCostStack.Team = Team;
            ConquerCostStack.Count = Owner.HoveredAreaConquerCost;
            ConquerCostStack.Interactable = false;
            ConquerCostStack.Activate();
        }
    }

    private void UpdateReinforcementDice()
    {
        // Only show dice for the main stack
        if (IsConquestStack) return;

        if (_attachedToMouse) // Only show if it's attached to the mouse
        {
            Dice.SetActive(Owner != null && Owner.CanUseReinforcementDice && Count > 0);
            Dice.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Dice/dice{Owner.HoveredAreaMinDiceRoll}");
        }
        else
            Dice.SetActive(false);
    }

    IEnumerator GenerateOffset()
    {
        yield return new WaitForFixedUpdate();

        var xOff = 0f;
        var yOff = 0f;
        foreach (Transform t in Stack)
        {
            t.localPosition = new Vector3(xOff, yOff, 0f);
            xOff += 4f;
            yOff -= 4f;
        }
        MainToken.transform.localPosition = new Vector3(xOff, yOff, 0f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!Interactable) return;

        if (eventData.pointerId == -1)
        {
            AttachToMouse();
            Owner.ActiveTokenStack = Token;
        }
    }

    public void Populate(TokenStack token)
    {
        Race = token.Race.Name.ToString();
        Count = token.Count;
        Interactable = token.Interactable;
        Team = token.Team;
        Owner = Utility.FindPlayerWithId(token.OwnerId);
        Token = token;

        MainToken.enabled = true;
        TeamTag.enabled = true;
        CountPanel.SetActive(true);
        Stack.gameObject.SetActive(true);
        Activate();
        Refresh();
    }

    public void Activate()
    {
        _active = true;
    }

    public void AttachToMouse()
    {
        _attachedToMouse = true;
        GetComponent<Image>().raycastTarget = false;
    }
}
