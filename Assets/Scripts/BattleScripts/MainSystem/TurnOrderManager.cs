using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Builds and manages the turn order queue based on speed.
/// Reacts to speed changes and actor removal mid-battle.
/// </summary>
public class TurnOrderManager
{
    private Queue<BattleActor> turnQueue = new Queue<BattleActor>();
    private List<BattleActor> cachedActors = new List<BattleActor>();

    private int actorsActedThisRound = 0;

    /// <summary>
    /// builds turn order queue from list of actors, ordered by speed.
    /// highest speed goes first
    /// </summary>
    public void BuildQueue(List<BattleActor> actors)
    {
        cachedActors = actors
            .Where(a => a.isAlive)
            .OrderByDescending(a => a.speed)
            .ToList();

        turnQueue.Clear();
        foreach (var actor in cachedActors)
            turnQueue.Enqueue(actor);

        // Debug.Log("[TURN ORDER]");
        // foreach (var actor in cachedActors)
        //     Debug.Log($" - {actor.GetType().Name} (SPD {actor.speed})");

        actorsActedThisRound = 0;
    }

    /// <summary>
    /// gets next actor in turn queue.
    /// if empty, rebuilds from cache (which should be up to date w/ any speed changes or removals)
    /// </summary>
    public BattleActor GetNextActor()
    {
        if (turnQueue.Count == 0)
        {
            BuildQueue(cachedActors);
        }

        BattleActor next = turnQueue.Dequeue();
        actorsActedThisRound++;

        return next;
    }

    public void OnSpeedChanged(BattleActor actor)
    {
        if (!cachedActors.Contains(actor))
            return;

        BuildQueue(cachedActors);
    }

    /// <summary>
    /// return true if all actors have acted this round.
    /// then, new round is started + turn order rebuilt
    /// </summary>
    public bool IsFullTurnOver
    {
        get
        {
            return actorsActedThisRound >= cachedActors.Count;
        }
    }

    public void RemoveActor(BattleActor actor)
    {
        if (!cachedActors.Remove(actor))
            return;

        BuildQueue(cachedActors);
    }
}
