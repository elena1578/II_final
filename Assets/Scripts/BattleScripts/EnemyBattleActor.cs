using UnityEngine;
using System.Collections.Generic;


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
        List<BattleActionData> possibleActions = new();

        // very important to check for nulls here, as not all enemies will have all action types
        // otherwise WILL cause crash
        var attack = enemyData.GetRandomAttackAction();
        if (attack != null) possibleActions.Add(attack);

        var heal = enemyData.GetRandomHealAction();
        if (heal != null) possibleActions.Add(heal);

        var emotion = enemyData.GetRandomEmotionAction();
        if (emotion != null) possibleActions.Add(emotion);

        var none = enemyData.GetRandomNoneAction();
        if (none != null) possibleActions.Add(none);

        if (possibleActions.Count == 0)
        {
            Debug.LogWarning($"{enemyData.name} has no valid actions! Using fallback NoneAction");
            return BattleActionLibrary.None;  // see below
        }

        return possibleActions[Random.Range(0, possibleActions.Count)];
    }

}
