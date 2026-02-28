using UnityEngine;
using System.Collections.Generic;


public class BattleAnimationController : MonoBehaviour
{
    public static BattleAnimationController instance;

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// play animation for an actor/target based on action data.
    /// returns duration of the animation to wait before next action
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="action"></param>
    /// <param name="targets"></param>
    /// <returns></returns>
    public float PlayAction(BattleActor actor, BattleActionData action, List<BattleActor> targets)
    {
        float duration = action.animationDuration;

        // actor animation
        if (action.animationTarget == BattleActionData.AnimationTarget.Self ||
            action.animationTarget == BattleActionData.AnimationTarget.Both)
        {
            actor.ui?.PlayActionAnimation(action.animationTrigger);
            Debug.Log($"[BattleAnimationController] Playing animation {action.animationTrigger} for actor {actor.name}");
        }

        // target animation
        if (action.animationTarget == BattleActionData.AnimationTarget.Target ||
            action.animationTarget == BattleActionData.AnimationTarget.Both)
        {
            foreach (var target in targets)
            {
                if (target == null || !target.isAlive)
                    continue;

                target.ui?.PlayActionAnimation(action.animationTrigger);
                Debug.Log($"[BattleAnimationController] Playing animation {action.animationTrigger} for target {target.name}");
            }
        }

        return duration;
    }
}
