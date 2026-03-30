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

    public BattleActionResult UseAction(BattleContext context, BattleActor actor, List<BattleActor> targets)
    {
        // juice cost
        if (!actor.SpendJuice(data.juiceCost))
            return BattleActionResult.None(actor);

        if (targets.Count == 0)
        {
            Debug.LogWarning("[AttackAction] No valid targets");
            return BattleActionResult.None(actor);
        }

        // damage calc & application
        int totalDamage = 0;
        bool crit = false;

        foreach (var target in targets)
        {
            if (target == null || !target.isAlive)
                continue;

            // check for crit, if yes, apply crit multiplier
            bool didCritForHit;
            int damage = data.CalculateDamage(actor, target, out didCritForHit);

            if (didCritForHit)
                crit = true;

            // check for power hit case (ignores def (done through dmg calc) & drops def by 30% for 3 turns)
            if (data.actionName == BattleActionData.BattleActionName.PowerHit)
                target.SetStatChange(BattleActionData.StatChangeType.DefenseDown, 0.7f, 3);

            target.TakeDamage(damage);
            totalDamage += damage;
        }

        // check if moving first in turn queue (for actions that should always go first)
        bool moveFirst = data.alwaysMoveFirst;

        // result for battle log
        return new BattleActionResult
        {
            actor = actor,
            targets = targets,
            damage = totalDamage,
            moveFirst = moveFirst,
            didCrit = crit,
            statChange = data.statChangeType,
            statMultiplier = data.statChangeMultiplier,
            statChangeDuration = data.statChangeDuration
        };
    }
}
