using UnityEngine;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// builds & manages turn order queue based on actor speed.
/// reacts to speed changes and actor removal mid-battle [via rebuilding queue from cached list]
/// </summary>
public class TurnOrderManager
{
    private Queue<BattleActor> turnQueue = new Queue<BattleActor>();
    private List<BattleActor> cachedActors = new List<BattleActor>();

    private int actorsActedThisRound = 0;

    /// <summary>
    /// builds turn order queue from list of actors, ordered by speed.
    /// also has a check for moveFirst actions to put those actors at the front of the queue regardless of speed (e.g., Hero's Smile)
    /// </summary>
    /// <param name="actors"></param>
    public void BuildQueue(List<BattleActor> actors)
    {
        cachedActors = actors.Where(a => a.isAlive).ToList();

        // separate actors whose action should always move first from normal actors
        // then order each group by speed
        var moveFirstActors = cachedActors.Where(a => a.moveFirst).ToList();
        var normalActors = cachedActors.Where(a => !a.moveFirst).OrderByDescending(a => a.speed).ToList();

        turnQueue.Clear();  // clear existing queue before rebuilding

        // enqueue moveFirst actors first, in order of speed
        foreach (var actor in moveFirstActors.OrderByDescending(a => a.speed))
            turnQueue.Enqueue(actor);

        // then enqueue normal actors
        foreach (var actor in normalActors)
            turnQueue.Enqueue(actor);

        actorsActedThisRound = 0;

        Debug.Log("[TurnOrderManager] Built turn queue:");
        foreach (var actor in turnQueue)
            Debug.Log($" - {actor.name} (SPD {actor.speed}, moveFirst={actor.moveFirst})");
    }

    /// <summary>
    /// gets next actor in turn queue.
    /// if empty, rebuilds from cache (which should be up to date w/ any speed changes or removals)
    /// </summary>
    public BattleActor GetNextActor()
    {
        if (turnQueue.Count == 0)
            BuildQueue(cachedActors);

        BattleActor next = turnQueue.Dequeue();  // get next actor
        next.moveFirst = false;  // reset flag after it's been used to ensure it only affects the turn order for one turn
        actorsActedThisRound++;  // increment count of actors who have acted this round

        return next;
    }

    /// <summary>
    /// called when an actor's speed changes mid-battle to rebuild queue
    /// </summary>
    /// <param name="actor"></param>
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

    /// <summary>
    /// called when an actor is removed mid-battle (e.g., defeated) to rebuild queue w/o that actor
    /// </summary>
    /// <param name="actor"></param>
    public void RemoveActor(BattleActor actor)
    {
        if (!cachedActors.Remove(actor))
            return;

        BuildQueue(cachedActors);
    }

    public void SetActorToMoveFirst(BattleActor actor)
    {
        if (!cachedActors.Contains(actor))
            return;

        // move the specified actor to the front of the queue
        // (just for this turn, will be reset next turn when queue is rebuilt from cache based on speed)
        cachedActors.Remove(actor);
        cachedActors.Insert(0, actor);

        BuildQueue(cachedActors);
    }
}
