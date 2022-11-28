using Assets.Enum;
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
    [Networked] public Team Team { get; set; }
    [Networked] public bool IsTurnActive { get; set; }
    [Networked(OnChanged = nameof(UiUpdateRequired)), Capacity(2)] public NetworkDictionary<NetworkString<_16>, TokenStack> Tokens => default;
    [Networked(OnChanged = nameof(UiUpdateRequired))] public Card ActiveCombo { get; set; }
    [Networked] public NetworkBool HasCombo { get; set; }
    public TokenStack? ActiveTokenStack { get; set; }

    private GameLogic _gameLogic;

    void Start()
    {
        Id = Guid.NewGuid().ToString();        
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

    public void TryConquerMapArea(MapArea area)
    {
        Debug.Log($"Attempting to conquer {area.name}: HasCombo({HasCombo}), IsOwnTurn({_gameLogic.IsPlayerTurn(Id.ToString())})");
        if (HasCombo && _gameLogic != null && _gameLogic.IsPlayerTurn(Id.ToString()) && ActiveTokenStack != null)
        {
            Debug.Log("Clientside verification OK");
            _gameLogic.RPC_ConquerArea(this, area.Id, ActiveTokenStack.Value);
        }
    }

    private static void UiUpdateRequired(Changed<PlayerBehaviour> changed)
    {
        Utility.UiUpdateRequired();
    }
}
