using UnityEngine;
using UnityEngine.UI;

public class EnemyBattleActorDataSetter : MonoBehaviour
{
    private EnemyOverworldData enemyOverworldData;
    private EnemyBattleData enemyBattleData;
    private BattleDebugTool battleDebugTool;
    private Image img;
    private Animator animator;

    private void Start()
    {
        battleDebugTool = FindFirstObjectByType<BattleDebugTool>();
        GetDataDebug();
    }
    
    private void GetData()
    {
        if (BattleTransitionManager.instance != null && BattleTransitionManager.instance.currentEnemy != null)
            enemyOverworldData = BattleTransitionManager.instance.currentEnemy;
        else
            Debug.LogWarning("EnemyBattleActorDataSetter: No enemy data found in BattleTransitionManager");
    }

    private void GetDataDebug()
    {
        if (battleDebugTool != null && battleDebugTool.enemies[0] != null)
            enemyBattleData = battleDebugTool.enemies[0];
        else
            Debug.LogWarning("EnemyBattleActorDataSetter: No debug enemy data found in BattleDebugTool");

        if (enemyBattleData != null)
            SetActorData();
    }

    private void SetActorData()
    {
        if (enemyBattleData != null)
        {
            img = GetComponent<Image>();
            if (img != null)
                img.sprite = enemyBattleData.battleSprite;
            else
                Debug.LogWarning("EnemyBattleActorDataSetter: No Image component found on GameObject");

        
            animator = GetComponent<Animator>();
            if (animator != null)
                animator.runtimeAnimatorController = enemyBattleData.animatorController;
            else
                Debug.LogWarning("EnemyBattleActorDataSetter: No Animator found on GameObject");
        }
    }
}
