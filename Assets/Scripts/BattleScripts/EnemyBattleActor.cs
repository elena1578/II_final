using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class EnemyBattleActor : BattleActor
{
    public EnemyBattleData enemyData;

    public EnemyBattleActor(EnemyBattleData data)
    {
        enemyData = data;

        InitializeFromData(
            name: data.characterName,   
            maxHP: data.baseHP,
            maxJuice: data.baseJuice,
            atk: data.baseAttack,
            def: data.baseDefense,
            speed: data.baseSpeed,
            startingEmotion: EmotionType.Neutral
        );
    }

    public override BattleActionData DecideAction(BattleContext context)
    {
        // sort by priority (lowest number = highest priority)
        // for OrderBy: https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.orderby?view=net-8.0
        // in this context, result = sorted list of EnemyAI rules:
        // r = each individual EnemyAI rule, r.priorityNumber = what the list is being sorted by
        var sortedRules = enemyData.actionAI.OrderBy(r => r.priorityNumber);

        foreach (var rule in sortedRules)
        {
            if (rule.action == null)
                continue;

            // HP conditional
            if (rule.requireHpBelow)
            {
                if (hpPercent > rule.hpBelowPercent)
                    continue;
            }

            // check if last action has been reached, if so, guarantee use
            if (rule.alwaysUseIfReached)
                return rule.action;

            float chance = GetEmotionChance(rule);

            if (Random.value < chance)
                return rule.action;
        }

        return enemyData.defaultAction;  // fallback if no rules are met
    }

    private float GetEmotionChance(EnemyAI rule)
    {
        return currentEmotion switch
        {
            EmotionType.Neutral => rule.neutralChance,
            EmotionType.Happy => rule.happyChance,
            EmotionType.Sad => rule.sadChance,
            EmotionType.Angry => rule.angryChance,
            _ => rule.neutralChance  // default to neutral chance if somehow emotion is invalid
        };
    }

    // placeholder DecideAction, actions decided by random chance
    // public override BattleActionData DecideAction(BattleContext context)
    // {
    //     List<BattleActionData> possibleActions = new();

    //     // very important to check for nulls here, as not all enemies will have all action types
    //     // otherwise WILL cause crash
    //     var attack = enemyData.GetRandomAttackAction();
    //     if (attack != null) possibleActions.Add(attack);

    //     var heal = enemyData.GetRandomHealAction();
    //     if (heal != null) possibleActions.Add(heal);

    //     var emotion = enemyData.GetRandomEmotionAction();
    //     if (emotion != null) possibleActions.Add(emotion);

    //     var none = enemyData.GetRandomNoneAction();
    //     if (none != null) possibleActions.Add(none);

    //     if (possibleActions.Count == 0)
    //     {
    //         Debug.LogWarning($"{enemyData.name} has no valid actions! Using fallback defaultAction");
    //         return enemyData.defaultAction;
    //     }

    //     return possibleActions[Random.Range(0, possibleActions.Count)];
    // }
}
