using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class EnemyBattleActor : BattleActor
{
    public EnemyBattleData enemyData;
    public IBattleEnemyBehaviour behavior;

    public EnemyBattleActor(EnemyBattleData data)
    {
        enemyData = data;
        maxEmotionTier = data.maxEmotionTier;

        InitializeFromData(
            name: data.characterName,   
            maxHP: data.baseHP,
            maxJuice: data.baseJuice,
            atk: data.baseAttack,
            def: data.baseDefense,
            speed: data.baseSpeed,
            startingEmotion: EmotionType.Neutral
        );

        // assign special behavior if data exists
        behavior = EnemyBehaviourFactory.Create(data.characterName);
    }

    public override BattleActionData DecideAction(BattleContext context)
    {
        var validRules = enemyData.actionAI
            .Where(r => r.action != null)
            .ToList();

        // 1. build weighted pool
        float totalWeight = 0f;
        List<(EnemyAI rule, float weight)> weighted = new();

        foreach (var rule in validRules)
        {
            if (rule.requireHpBelow && hpPercent > rule.hpBelowPercent)
                continue;

            float weight = GetEmotionWeight(rule);
            if (weight <= 0f)
                continue;

            totalWeight += weight;
            weighted.Add((rule, weight));
        }

        // 2. weighted selection
        if (totalWeight > 0f)
        {
            float roll = Random.Range(0f, totalWeight);
            float current = 0f;

            foreach (var (rule, weight) in weighted)
            {
                current += weight;
                if (roll <= current)
                    return rule.action;
            }
        }

        // 3. fallback to last (always-use) rule (if exists, if not, just default action)
        foreach (var rule in validRules)
        {
            if (!rule.alwaysUseIfReached)
                continue;

            if (rule.requireHpBelow && hpPercent > rule.hpBelowPercent)
                continue;

            return rule.action;
        }

        return enemyData.defaultAction;
    }

    private float GetEmotionWeight(EnemyAI rule)
    {
        string group = EmotionSystem.GetEmotionGroupingText(currentEmotion);

        return group switch
        {
            "Happy" => rule.happyChance,
            "Sad" => rule.sadChance,
            "Angry" => rule.angryChance,
            _ => rule.neutralChance
        };
    }
}
