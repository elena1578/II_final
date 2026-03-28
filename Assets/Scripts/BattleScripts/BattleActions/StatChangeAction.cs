using UnityEngine;
using System.Collections.Generic;


public class StatChangeAction : IBattleAction
{
    private BattleActionData data;

    public StatChangeAction(BattleActionData data) => this.data = data;

    public bool CanUseAction(BattleContext context)
    {
        return context.currentActor.currentJuice >= data.juiceCost;
    }

    public BattleActionResult UseAction(BattleContext context, BattleActor actor, List<BattleActor> targets)
    {
        // check juice
        if (!actor.SpendJuice(data.juiceCost))
            return BattleActionResult.None(actor);

        // check for valid target(s)
        if (targets.Count == 0)
        {
            Debug.LogWarning("[StatChangeAction] No valid targets");
            return BattleActionResult.None(actor);
        }
        
        BattleActor target = targets[0];
        target.SetStatChange(data.statChangeType, data.statChangeMultiplier, data.statChangeDuration);

        // check if moving first in turn queue (for actions that should always go first)
        bool moveFirst = data.alwaysMoveFirst;

        return new BattleActionResult
        {
            actor = actor,
            targets = targets,
            moveFirst = moveFirst,
            statChange = data.statChangeType,
            statMultiplier = data.statChangeMultiplier,
            statChangeDuration = data.statChangeDuration
        };
    }
}
