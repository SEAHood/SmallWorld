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

    public string Race;
    public Team Team;
    [Min(1)] public int Count;
    public PlayerBehaviour Owner;
    public TokenStack Token;

    private string _lastRace;
    private int _lastCount;
    private Team _lastTeam;

    private Vector3 _originalPosition;
    private bool _attachedToMouse;

    void Start()
    {
        _originalPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (Race != _lastRace || Count != _lastCount || Team != _lastTeam)
            Refresh();

        _lastRace = Race;
        _lastCount = Count;
        _lastTeam = Team;
    }

    void Update()
    {
        if (!Interactable) return;

        if (Input.GetMouseButtonDown(1))
        {
            _attachedToMouse = false;
            GetComponent<Image>().raycastTarget = true;
            Owner.ActiveTokenStack = null;
        }

        if (_attachedToMouse)
            transform.position = Input.mousePosition;
        else
            transform.position = _originalPosition;
    }

    private void Refresh()
    {
        UpdateMainToken();
        UpdateCountText();
        UpdateTeamTag();
        UpdateStack();
        GenerateOffset();
    }

    private void UpdateMainToken()
    {
        MainToken.sprite = Resources.Load<Sprite>($"Tokens/{Race}Token");
    }

    private void UpdateCountText()
    {
        if (Count == 1)
            CountPanel.SetActive(false);
        else
        {
            CountPanel.SetActive(true);
            CountText.text = Count.ToString();
        }
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
        Utility.ClearTransform(Stack);
        var stackHeight = Count <= 4 ? Count : 4;
        for (var i = 0; i < stackHeight - 1; i++)
        {
            var stackItem = Instantiate(StackTokenTemplatePrefab, Stack);
            stackItem.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Tokens/{Race}Token");
        }
        StartCoroutine(GenerateOffset());
    }

    IEnumerator GenerateOffset()
    {
        yield return new WaitForFixedUpdate();

        var xOff = 0f;
        var yOff = 0f;
        foreach (Transform t in Stack)
        {
            Debug.Log($"Offsetting stack item by {xOff},{yOff}");
            t.localPosition = new Vector3(xOff, yOff, 0f);
            xOff += 4f;
            yOff -= 4f;
        }
        Debug.Log($"Offsetting main item by {xOff},{yOff}");
        MainToken.transform.localPosition = new Vector3(xOff, yOff, 0f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!Interactable) return;

        if (eventData.pointerId == -1)
        {
            _attachedToMouse = true;
            GetComponent<Image>().raycastTarget = false;
            Owner.ActiveTokenStack = Token;
        }
    }

    public void Populate(TokenStack token)
    {
        Race = token.Race.Name.ToString();
        Count = token.Count;
        Interactable = token.PlayerControlled;
        Team = token.Team;
        Owner = Utility.FindPlayerWithId(token.OwnerId);
        Token = token;
    }
}
