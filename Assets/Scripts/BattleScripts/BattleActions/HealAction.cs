using UnityEngine;
using System.Collections.Generic;

public class HealAction : IBattleAction
{
    private BattleActionData data;

    public HealAction(BattleActionData data)
    {
        this.data = data;
    }

    public bool CanUseAction(BattleContext context)
    {
        return context.currentActor.currentJuice >= data.juiceCost;
    }

    public BattleActionResult UseAction(BattleContext context, BattleActor actor)
    {
        // juice cost        
        if (!actor.SpendJuice(data.juiceCost))
            return BattleActionResult.None(actor);

        // target resolution
        List<BattleActor> targets = context.GetActionTargets(actor, data);
        if (targets.Count == 0)
        {
            Debug.LogWarning("[HealAction] No valid targets");
            return BattleActionResult.None(actor);
        }

        // battle log
        BattleActionResult result = new BattleActionResult
        {
            actor = actor,
            targets = targets
        };

        // apply heal
        foreach (var target in targets)
        {
            // calc heal as a % of target's base HP
            int healValue = Mathf.RoundToInt(target.maxHP * data.healPercentage);
            int actualHeal = Mathf.Min(healValue, target.maxHP - target.currentHP);  // can't heal beyond max HP

            target.Heal(actualHeal);
            result.heal += actualHeal;
        }

        return result;
    }
}
