using UnityEngine;
using System.Collections.Generic;

public class EmotionAction : IBattleAction
{
    private BattleActionData data;

    public EmotionAction(BattleActionData data) => this.data = data;

    public bool CanUseAction(BattleContext context)
    {
        return context.currentActor.currentJuice >= data.juiceCost;
    }

    public BattleActionResult UseAction(BattleContext context, BattleActor actor)
    {
        if (!actor.SpendJuice(data.juiceCost))
            return BattleActionResult.None(actor);

        List<BattleActor> targets = context.GetActionTargets(actor, data);
        if (targets.Count == 0)
        {
            Debug.LogWarning("[EmotionAction] No valid targets");
            return BattleActionResult.None(actor);
        }

        BattleActor target = targets[0];
        target.SetEmotion(data.emotionEffect);

        return new BattleActionResult
        {
            actor = actor,
            targets = targets,
            emotion = data.emotionEffect
        };
    }
}
