using UnityEngine;
using System.Collections.Generic;

public class GuardAction : IBattleAction
{
    private BattleActionData data;

    public GuardAction(BattleActionData data)
    {
        this.data = data;
    }

    public bool CanUseAction(BattleContext context)
    {
        return true;  // no juice cost for guarding
    }

    public BattleActionResult UseAction(BattleContext context, BattleActor actor)
    {
        actor.EnableGuard(data.damageReductionMultiplier);

        return new BattleActionResult
        {
            actor = actor,
            targets = new List<BattleActor> { actor },
            damage = 0
        };
    }
}
