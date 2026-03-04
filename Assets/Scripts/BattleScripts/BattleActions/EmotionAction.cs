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

        // check for special roar case
        if (data.actionName == BattleActionData.BattleActionName.Roar)
        {
            // right now, these stat changes are applied forever even if emotion is changed
            // currently not a problem since character emotion actions can only currently be applied to self
            // but once they are applied to other targets, will need to find a way to reset these upon emotion change
            // [to anything that's not angry since that's the only time these buffs are applied]
            
            target.ApplyTemporaryStatModifier(
                attackMult: 3f,  // attack = 45 (from 15)
                defenseMult: 2f,  // def = 30 (from 15)
                speedMult: 4f // speed = 41 (from 10)
            );

            Debug.Log($"[EmotionAction] Roar applied temporary stat modifiers to {target.GetType().Name}\nNew stats - ATK: {target.atk}, DEF: {target.def}, SPD: {target.speed}");
        }

        return new BattleActionResult
        {
            actor = actor,
            targets = targets,
            emotion = data.emotionEffect
        };
    }
}
