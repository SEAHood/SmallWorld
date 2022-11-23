using Assets.Helper;
using Assets.Model;
using Fusion;
using System;
using TMPro;
using UnityEngine;

public class PlayerBehaviour : NetworkBehaviour
{
    [Networked] public NetworkString<_128> Id { get; set; }
    [Networked] public NetworkString<_16> Name { get; set; }
    [Networked] public bool IsTurnActive { get; set; }
    [Networked(OnChanged = nameof(UiUpdateRequired)), Capacity(2)] public NetworkDictionary<NetworkString<_16>, int> Tokens => default;
    [Networked(OnChanged = nameof(UiUpdateRequired))] public Card ActiveCombo { get; set; }
    [Networked]public NetworkBool HasCombo { get; set; }

    public TextMeshPro TurnText;

    private GameLogic _gameLogic;

    void Start()
    {
        Id = Guid.NewGuid().ToString();
        TurnText = GetComponent<TextMeshPro>();
        TurnText.enabled = HasInputAuthority;
        Name = Guid.NewGuid().ToString().Substring(0, 5);
        _gameLogic = FindObjectOfType<GameLogic>();
    }

    void Update()
    {
        if (HasInputAuthority)
        {
            
        }
    }

    public bool IsLocal()
    {
        return HasInputAuthority;
    }

    public void TryAcquireCombo(Card combo)
    {
        Debug.Log($"Attempting to acquire {combo.Power.Name} {combo.Race.Name}: HasCombo({HasCombo}), IsOwnTurn({_gameLogic.IsPlayerTurn(Id.ToString())})");
        if (!HasCombo && _gameLogic != null && _gameLogic.IsPlayerTurn(Id.ToString()))
        {
            Debug.Log("Clientside verification OK");
            _gameLogic.RPC_ClaimCombo(this, combo);
        }
    }

    private static void UiUpdateRequired(Changed<PlayerBehaviour> changed)
    {
        Utility.UiUpdateRequired();
    }
}
