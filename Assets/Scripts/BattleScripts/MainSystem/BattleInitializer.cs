using UnityEngine;
using System.Collections.Generic;


public class BattleInitializer : MonoBehaviour
{
    [Header("Party Members")]
    public List<CharacterBattleData> defaultParty;  // fixed for now (4 main members)

#if UNITY_EDITOR
    private BattleDebugTool debugTool;
#endif

    private void Start()
    {
        var transition = BattleTransitionManager.instance;

        if (transition == null)
        {
            Debug.LogError("[BattleInitializer] BattleTransitionManager not found");
            return;
        }

        if (defaultParty == null || defaultParty.Count == 0)
        {
            Debug.LogError("[BattleInitializer] No party members assigned");
            return;
        }

        EnemyBattleData enemyBattleData = null;

        // standard overworld -> battle flow
        if (transition.currentEnemy != null)
        {
            enemyBattleData = transition.currentEnemy.correspondingBattleData;

            if (enemyBattleData == null)
            {
                Debug.LogError("[BattleInitializer] Overworld enemy has no corresponding battle data!");
                return;
            }

            Debug.Log($"[BattleInitializer] Starting battle from overworld: {enemyBattleData.characterName}");
        }

        // debug flow (no overworld data)
        else
        {
#if UNITY_EDITOR
            debugTool = FindFirstObjectByType<BattleDebugTool>();

            if (debugTool != null &&
                debugTool.enemies != null &&
                debugTool.enemies.Count > 0)
            {
                enemyBattleData = debugTool.enemies[0];
                Debug.Log($"[BattleInitializer] Starting DEBUG battle: {enemyBattleData.characterName}");
            }
#endif
            // error if no overworld data and no debug data
            if (enemyBattleData == null)
            {
                Debug.LogError("[BattleInitializer] No enemy available for battle (overworld or debug).");
                return;
            }
        }

        List<EnemyBattleData> enemies = new()
        {
            enemyBattleData
        };

        BattleManager.instance.StartBattle(defaultParty, enemies); 
    }
}