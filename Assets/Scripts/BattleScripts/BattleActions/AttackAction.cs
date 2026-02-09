using UnityEngine;
using System.Collections.Generic;


public class AttackAction : IBattleAction
{
    private BattleActionData data;

    public AttackAction(BattleActionData data)
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
            Debug.LogWarning("[AttackAction] No valid targets");
            return BattleActionResult.None(actor);
        }

        // damage calc & application
        int totalDamage = 0;  

        foreach (var target in targets)
        {
            if (target == null || !target.isAlive)
                continue;

            int damage = data.CalculateDamage(actor, target, data);
            target.TakeDamage(damage);
            totalDamage += damage;
        }

        // result for battle log
        return new BattleActionResult
        {
            actor = actor,
            targets = targets,
            damage = totalDamage
        };
    }
}
