using UnityEngine;
using System.Collections.Generic;


public class BattleInitializer : MonoBehaviour
{
    [Header("Party Members")]
    public List<CharacterBattleData> defaultParty;  // fixed for now (main 4 members)

    private void Start()
    {
        var transition = BattleTransitionManager.instance;

        if (transition == null)
        {
            Debug.LogError("[BattleInitializer] BattleTransitionManager not found!");
            return;
        }

        if (transition.currentEnemy == null)
        {
            Debug.LogError("[BattleInitializer] No enemy passed into battle!");
            return;
        }

        // convert battle data from overworld to battle scene format
        EnemyBattleData enemyBattleData = transition.currentEnemy.correspondingBattleData;
        Debug.Log($"[BattleInitializer] Starting battle with enemy: {enemyBattleData.characterName}");

        if (enemyBattleData == null)
        {
            Debug.LogError("[BattleInitializer] EnemyOverworldData has no corresponding BattleData!");
            return;
        }

        List<EnemyBattleData> enemies = new()
        {
            enemyBattleData
        };

        if (defaultParty == null || defaultParty.Count == 0)
        {
            Debug.LogError("[BattleInitializer] No party members assigned in BattleInitializer!");
            return;
        }

        BattleManager.instance.StartBattle(defaultParty, enemies);
    }
}