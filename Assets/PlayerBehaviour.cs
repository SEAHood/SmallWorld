using Assets.Enum;
using Assets.Helper;
using Assets.Model;
using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PlayerBehaviour : NetworkBehaviour
{
    [Networked] public NetworkString<_64> Id { get; set; }
    [Networked] public NetworkString<_16> Name { get; set; }
    [Networked] public Team Team { get; set; }
    [Networked(OnChanged = nameof(UiUpdateRequired)), Capacity(2)] public NetworkDictionary<NetworkString<_16>, TokenStack> Tokens => default;
    [Networked(OnChanged = nameof(UiUpdateRequired))] public Combo ActiveCombo { get; set; }
    [Networked(OnChanged = nameof(HasComboChanged))] public NetworkBool HasCombo { get; set; }
    [Networked] public int Coins { get; set; }
    [Networked] public bool HasActiveTokensInPlay { get; set; }
    [Networked] public int ConquerOrderIx { get; set; }
    [Networked] public bool HasUsedReinforcementDice { get; set; }
    [Networked] public bool HasPerformedActionThisTurn { get; set; }
    public TokenStack? ActiveTokenStack { get; set; }
    public int HoveredAreaConquerCost { get; set; }
    public bool CanUseReinforcementDice { get; set; }
    public int HoveredAreaMinDiceRoll { get; set; }

    private GameLogic _gameLogic;

    void Start()
    {
        Id = Guid.NewGuid().ToString();
        _gameLogic = FindObjectOfType<GameLogic>();
        if (IsLocal())
            RPC_SetName(PlayerPrefs.GetString("name"));
    }

    void Update()
    {
        if (HasInputAuthority && Name != PlayerPrefs.GetString("name"))
        {
            Name = PlayerPrefs.GetString("name");
        }
    }

    public bool IsLocal()
    {
        return HasInputAuthority;
    }

    public void TryAcquireCombo(Combo combo)
    {
        Debug.Log($"Attempting to acquire {combo.Power.Name} {combo.Race.Name}: HasCombo({HasCombo}), IsOwnTurn({_gameLogic.IsPlayerTurn(Id.ToString())})");
        if (!HasCombo && IsOwnTurn())
        {
            Debug.Log("Clientside verification OK");
            _gameLogic.RPC_ClaimCombo(this, combo);
        }
    }

    public void TryAffectMapArea(MapArea area)
    {
        if (!HasCombo || !IsOwnTurn()) return;
        Debug.Log($"ActiveTokenStack.HasValue: {ActiveTokenStack.HasValue}, {(ActiveTokenStack.HasValue ? ActiveTokenStack.Value.Count : "-")}");

        if (ActiveTokenStack.HasValue)
        {
            if (_gameLogic.TurnStage == GameLogic.TurnState.Conquer)
            {
                if (area.OccupyingForce.OwnerId == Id) return;
                Debug.Log($"[CLIENT] Attempting to conquer {area.name}");
                _gameLogic.RPC_ConquerArea(this, area.Id, ActiveTokenStack.Value);
            }
            else if (_gameLogic.TurnStage == GameLogic.TurnState.Redeploy)
            {
                if (area.OccupyingForce.OwnerId != Id) return;
                Debug.Log($"[CLIENT] Attempting to redeploy troops to {area.name}");
                _gameLogic.RPC_RedeployToArea(this, area.Id, ActiveTokenStack.Value);
            }
        }
        else if (_gameLogic.TurnStage == GameLogic.TurnState.Redeploy)
        {
            if (area.OccupyingForce.OwnerId != Id) return;
            Debug.Log($"[CLIENT] Attempting to collect token from {area.name}");
            _gameLogic.RPC_CollectToken(this, area.Id);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_RefreshActiveTokenStack()
    {
        if (ActiveTokenStack.HasValue && ActiveTokenStack.Value.Count <= 0)
            ActiveTokenStack = null;

        if (ActiveTokenStack.HasValue && Tokens.TryGet(ActiveTokenStack.Value.Race.Name, out var playerToken))
            ActiveTokenStack = playerToken;
        else
            ActiveTokenStack = null;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_PickUpToken(TokenStack tokenStack)
    {
        Debug.Log("RPC_PickUpToken");
        FindObjectOfType<GameUi>().CreateMouseAttachedTokenStack(tokenStack);
        ActiveTokenStack = tokenStack;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_AwardAreaTokens(NetworkString<_4> area, int value)
    {
        var mapArea = FindObjectsOfType<MapArea>().First(x => x.Id == area);
        FindObjectOfType<GameUi>().ShowCoinAnimation(mapArea, value, true, () => { });
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_NotifyAreaTokens(NetworkString<_4> area, int value)
    {
        var mapArea = FindObjectsOfType<MapArea>().First(x => x.Id == area);
        FindObjectOfType<GameUi>().ShowCoinAnimation(mapArea, value, false, () => { });
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_NotifyTotalRoundTokens(int value)
    {
        FindObjectOfType<GameUi>().ShowTotalRoundCoins(value);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_NotifyEndOfGame()
    {
        FindObjectOfType<GameUi>().ShowEndOfGame();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_NotifyDiceRoll(int roll)
    {
        FindObjectOfType<DiceRoller>().Initialise(roll);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_NotifyRollSuccess()
    {
        FindObjectOfType<SoundManager>().PlayByeah();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetName(string name)
    {
        Debug.Log($"[??] RPC_SetName hit - setting Name to {name}");
        Name = name;
    }

    public void TryAction()
    {
        if (IsOwnTurn())
        {
            _gameLogic.RPC_Action(this);
        }
    }

    public void TryDecline()
    {
        if (IsOwnTurn() && _gameLogic.TurnStage == GameLogic.TurnState.Conquer)
        {
            _gameLogic.RPC_Decline(this);
        }
    }

    private bool IsOwnTurn()
    {
        return _gameLogic != null && _gameLogic.IsPlayerTurn(Id.ToString());
    }

    private static void UiUpdateRequired(Changed<PlayerBehaviour> changed)
    {
        Utility.UiUpdateRequired();
    }
    private static void HasComboChanged(Changed<PlayerBehaviour> changed)
    {
        //FindObjectOfType<GameUi>().ToggleComboPanel(changed.Behaviour.HasCombo);
    }
}
