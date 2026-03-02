using UnityEngine;
using System;
using System.Collections.Generic;


public class TurnStateMachine
{
    public BattleState CurrentState { get; private set; }
    private readonly Action<BattleState> onStateEntered;

    // dictionary defining linear bat state flow
    // i.e., state which state comes next after current state when advancing turn
    // (e.g., start -> actor turn, then actor turn -> resolve turn)
    private readonly Dictionary<BattleState, BattleState> stateFlow =
        new Dictionary<BattleState, BattleState>
        {
            { BattleState.Start, BattleState.ActorTurn },
            { BattleState.ActorTurn, BattleState.ResolveTurn },
            { BattleState.ResolveTurn, BattleState.End }
        };

    /// <summary>
    /// take callback to invoke on state entry so BattleManager can react to state changes
    /// </summary>
    /// <param name="onStateEnteredCallback"></param>
    public TurnStateMachine(Action<BattleState> onStateEnteredCallback) => onStateEntered = onStateEnteredCallback;

    /// <summary>
    /// sets current battle state, invokes callback
    /// </summary>
    /// <param name="newState"></param>
    public void SetState(BattleState newState)
    {
        CurrentState = newState;
        onStateEntered?.Invoke(CurrentState);
    }

    /// <summary>
    /// advance to next state in linear flow
    /// </summary>
    public void AdvanceState()
    {
        if (!stateFlow.TryGetValue(CurrentState, out BattleState nextState))
            return;

        SetState(nextState);
    }

    /// <summary>
    /// helper to enter a specific state
    /// </summary>
    /// <param name="state"></param>
    public void EnterState(BattleState state) => SetState(state);
}
