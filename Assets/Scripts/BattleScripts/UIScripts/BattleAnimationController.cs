using UnityEngine;
using System.Collections.Generic;


public class BattleAnimationController : MonoBehaviour
{
    public static BattleAnimationController instance;

    private void Awake()
    {
        instance = this;
    }

    public float PlayAction(BattleActor actor, BattleActionData action, List<BattleActor> targets)
    {
        float duration = action.animationDuration;

        // actor animation
        if (action.animationTarget == BattleActionData.AnimationTarget.Self ||
            action.animationTarget == BattleActionData.AnimationTarget.Both)
        {
            actor.ui?.PlayActionAnimation(action.animationTrigger);
            Debug.Log($"Playing animation {action.animationTrigger} for ACTOR {actor.name}");
        }

        // target animations
        if (action.animationTarget == BattleActionData.AnimationTarget.Target ||
            action.animationTarget == BattleActionData.AnimationTarget.Both)
        {
            foreach (var target in targets)
            {
                if (target == null || !target.isAlive)
                    continue;

                target.ui?.PlayActionAnimation(action.animationTrigger);
                Debug.Log($"Playing animation {action.animationTrigger} for TARGET {target.name}");
            }
        }

        return duration;
    }
}
