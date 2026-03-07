using UnityEngine;
using System.Collections.Generic;


public interface IBattleAction
{
    bool CanUseAction(BattleContext context);
    BattleActionResult UseAction(BattleContext context, BattleActor actor, List<BattleActor> targets);
}

