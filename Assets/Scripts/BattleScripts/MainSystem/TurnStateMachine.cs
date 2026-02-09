using UnityEngine;
using System;
using System.Collections.Generic;

public class TurnStateMachine
{
    public BattleState CurrentState { get; private set; }

    private readonly Action<BattleState> onStateEntered;

    // dictionary defining linear state flow
    private readonly Dictionary<BattleState, BattleState> stateFlow =
        new Dictionary<BattleState, BattleState>
        {
            { BattleState.Start, BattleState.ActorTurn },
            { BattleState.ActorTurn, BattleState.ResolveTurn },
            { BattleState.ResolveTurn, BattleState.End }
        };

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
    public void EnterState(BattleState state)
    {
        SetState(state);
    }
}
