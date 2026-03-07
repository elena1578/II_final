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

    public BattleActionResult UseAction(BattleContext context, BattleActor actor, List<BattleActor> targets)
    {
        // juice cost        
        if (!actor.SpendJuice(data.juiceCost))
            return BattleActionResult.None(actor);

        if (targets.Count == 0)
        {
            Debug.LogWarning("[HealAction] No valid targets");
            return BattleActionResult.None(actor);
        }

        // battle log
        BattleActionResult result = new BattleActionResult
        {
            actor = actor,
            targets = targets,
            emotion = null
        };

        // apply heal
        foreach (var target in targets)
        {          
            // check for consume exception that uses a flat 170 HP heal
            if (data.actionName == BattleActionData.BattleActionName.Consume)
            {
                int consumeHeal = 170;
                int consumeActualHeal = Mathf.Min(consumeHeal, target.maxHP - target.currentHP);  // can't heal beyond max HP

                target.Heal(consumeActualHeal);
                result.heal += consumeActualHeal;
                result.emotion = EmotionType.Happy;  // also apply happy emotion to self

                continue;  // skip regular heal calc
            }

            // standard heal: calc heal as a % of target's base HP
            int healValue = Mathf.RoundToInt(target.maxHP * data.healPercentage);
            int actualHeal = Mathf.Min(healValue, target.maxHP - target.currentHP);  // can't heal beyond max HP

            target.Heal(actualHeal);
            result.heal += actualHeal;
            result.emotion = null;  // no emotion change for standard heals
        }

        return result;
    }
}
