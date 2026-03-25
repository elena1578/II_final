using UnityEngine;
using UnityEngine.UI;


public class EnemyBattleActorDataSetter : MonoBehaviour
{
    private EnemyOverworldData enemyOverworldData;
    private EnemyBattleData enemyBattleData;

    public EnemyBattleData defaultEnemyBattleData;  // fallback battle data if no overworld or debug data is found
#if UNITY_EDITOR
    private BattleDebugTool battleDebugTool;
#endif

    private Image img;
    private Animator animator;

    private void Awake()
    {
        img = GetComponent<Image>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
#if UNITY_EDITOR
        battleDebugTool = FindFirstObjectByType<BattleDebugTool>();
#endif
        if (enemyBattleData == null)  // only attempt to find if it hasn't been externally assigned
        {
            bool dataFound = false;

            // try overworld data first
            if (TryGetBattleTransitionData())
                dataFound = true;

            // otherwise, try debug data (only in editor)
            else if (TryGetDebugData())
                dataFound = true;

            // otherwise, if default data is set, use that as a fallback (editor & build)
            else if (defaultEnemyBattleData != null)
            {
                enemyBattleData = defaultEnemyBattleData;
                dataFound = true;
                Debug.LogWarning("[EnemyBattleActorDataSetter] Using default enemy battle data.");
            }

            if (dataFound)
                ApplyVisuals();
            else
                Debug.LogWarning("[EnemyBattleActorDataSetter] No enemy data available to apply.");
        }
    }

    /// <summary>
    /// try to get battle data via BattleTransitionManager's currentEnemy ref (which contains a 
    /// ref to the overworld data, which in turn contains a ref to the battle data)
    /// </summary>
    /// <returns></returns>
    private bool TryGetBattleTransitionData()
    {
        if (BattleTransitionManager.instance == null)
            return false;

        if (BattleTransitionManager.instance.currentEnemy == null)
            return false;

        enemyOverworldData = BattleTransitionManager.instance.currentEnemy;

        if (enemyOverworldData.correspondingBattleData == null)
        {
            if (defaultEnemyBattleData != null)
            {
                enemyBattleData = defaultEnemyBattleData;
                Debug.LogWarning("[EnemyBattleActorDataSetter] No corresponding battle data found, using default.");
                return true;
            }

            return false;
        }

        enemyBattleData = enemyOverworldData.correspondingBattleData;
        return true;
    }

    private bool TryGetDebugData()
    {
#if UNITY_EDITOR
        if (battleDebugTool == null)
            return false;

        if (battleDebugTool.enemies == null || battleDebugTool.enemies.Count == 0)
            return false;

        enemyBattleData = battleDebugTool.enemies[0];
        return enemyBattleData != null;
#else
        return false;
#endif
    }

    /// <summary>
    /// set visuals (sprite + animator) based on provided battle data (if not null)
    /// </summary>
    private void ApplyVisuals()
    {
        if (enemyBattleData == null)
            return;

        if (img != null)
        {
            img.sprite = enemyBattleData.battleSprite;

            if (enemyBattleData.setCustomWidthHeight)
            {
                RectTransform rt = img.GetComponent<RectTransform>();
                if (rt != null)
                    rt.sizeDelta = new Vector2(enemyBattleData.customWidth, enemyBattleData.customHeight);
            }

            if (enemyBattleData.setCustomTransformPosition)
                img.transform.localPosition = enemyBattleData.customTransformPosition;
        }

        if (animator != null)
            animator.runtimeAnimatorController = enemyBattleData.animatorController;
    }


    #region Helpers
    public void SetBattleData(EnemyBattleData data)
    {
        enemyBattleData = data;
        ApplyVisuals();
    }
    #endregion
}