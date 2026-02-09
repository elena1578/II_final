using UnityEngine;
using System;

public static class BattleActionAssignment
{
    public static IBattleAction Create(BattleActionData action)
    {
        if (action == null)
            throw new Exception("BattleActionAssignment received null action data");
        
        switch (action.actionType)
        {
            case BattleActionData.ActionType.Attack:
                return new AttackAction(action);
            case BattleActionData.ActionType.Heal:
                return new HealAction(action);
            case BattleActionData.ActionType.Emotion:
                return new EmotionAction(action);
            case BattleActionData.ActionType.Guard:
                return new GuardAction(action);
            case BattleActionData.ActionType.None:
                return new NoneAction(action);
            default:
                throw new Exception("unknown action type");
        }
    }
}
