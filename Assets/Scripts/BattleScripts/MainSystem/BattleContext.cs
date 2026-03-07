using UnityEngine;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// used to contextualize NOT state, but data about the current battle
/// (e.g., current party members, enemies, turn order, etc.)
/// </summary>
public class BattleContext
{
    public List<BattleActor> party { get; private set; }
    public List<BattleActor> enemies { get; private set; }

    public BattleActor currentActor { get; set; }
    public BattleActor lastActor { get; set; }

    public BattleContext(List<BattleActor> party, List<BattleActor> enemies)
    {
        this.party = party;
        this.enemies = enemies;

        // cap enemy count to 3
        if (enemies.Count > 3)
        {
            this.enemies = enemies.Take(3).ToList();  // only take first 3 of list if more try to be added
            Debug.LogWarning("[BattleContext] Enemy count capped at 3. Extra enemies have been ignored.");
        }
    }


    #region Targeting
    public List<BattleActor> GetActionTargets(BattleActor actor, BattleActionData action)
    {
        List<BattleActor> result = new();

        if (action.validTargets.HasFlag(TargetGroup.Self))
            result.Add(actor);

        // .Where(a => a.isAlive) filters out dead targets, so if all enemies are dead this will return an empty list vs. list w/ null target
        if (action.validTargets.HasFlag(TargetGroup.Allies))
            result.AddRange(GetAlliesOf(actor).Where(a => a.isAlive));

        if (action.validTargets.HasFlag(TargetGroup.Enemies))
            result.AddRange(GetEnemiesOf(actor).Where(a => a.isAlive));

        if (result.Count == 0)
            return result;

        // handle single-target actions
        if (action.multiTarget == false)
            return Wrap(GetRandomAlive(result));

        return result;
    }
    #endregion


    #region Helpers
    private List<BattleActor> GetEnemiesOf(BattleActor actor)
    {
        return actor is PlayerBattleActor ? enemies : party;
    }

    private List<BattleActor> GetAlliesOf(BattleActor actor)
    {
        return actor is PlayerBattleActor ? party : enemies;
    }

    private List<BattleActor> GetAllActors()
    {
        List<BattleActor> all = new();
        all.AddRange(party);
        all.AddRange(enemies);
        return all;
    }

    private BattleActor GetRandomAlive(List<BattleActor> list)
    {
        var alive = list.Where(a => a.isAlive).ToList();
        if (alive.Count == 0)
            return null;

        return alive[Random.Range(0, alive.Count)];
    }

    /// <summary>
    /// helper to wrap single target in a list
    /// </summary>
    /// <param name="actor"></param>
    /// <returns></returns>
    private List<BattleActor> Wrap(BattleActor actor)
    {
        return actor != null ? new List<BattleActor> { actor } : new List<BattleActor>();
    }
    #endregion
}
