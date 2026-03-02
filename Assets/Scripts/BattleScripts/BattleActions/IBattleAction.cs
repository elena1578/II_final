using UnityEngine;


public interface IBattleAction
{
    bool CanUseAction(BattleContext context);
    BattleActionResult UseAction(BattleContext context, BattleActor actor);
}

