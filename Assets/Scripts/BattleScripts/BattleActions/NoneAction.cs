using UnityEngine;

public class NoneAction : IBattleAction
{
    private BattleActionData data;

    public NoneAction(BattleActionData data)
    {
        this.data = data;
    }

    public bool CanUseAction(BattleContext context)
    {
        return true;
    }

   public BattleActionResult UseAction(BattleContext context, BattleActor actor)
    {
        Debug.Log($"{actor.name} does nothing!");

        return BattleActionResult.None(actor);
    }
}
