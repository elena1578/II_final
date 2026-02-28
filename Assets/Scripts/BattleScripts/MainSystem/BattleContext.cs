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
    }


    #region Targeting
    public List<BattleActor> GetActionTargets(BattleActor actor, BattleActionData action)
    {
        switch (action.targetType)
        {
            case ActionTargetType.Self:
                return new List<BattleActor> { actor };

            case ActionTargetType.SingleEnemy:
                return Wrap(GetRandomAlive(GetEnemiesOf(actor)));

            // .Where(a => a.isAlive) filters out dead targets, so if all enemies are dead this will return an empty list vs. list w/ null target
            case ActionTargetType.AllEnemies:
                return GetEnemiesOf(actor).Where(a => a.isAlive).ToList();

            case ActionTargetType.SingleAlly:
                return Wrap(GetRandomAlive(GetAlliesOf(actor)));

            case ActionTargetType.AllAllies:
                return GetAlliesOf(actor).Where(a => a.isAlive).ToList();

            default:
                return new List<BattleActor>();
        }
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
