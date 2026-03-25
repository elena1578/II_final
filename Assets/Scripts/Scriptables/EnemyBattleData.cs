using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyBattleData", menuName = "Scriptable Objects/Enemies/EnemyBattleData")]
public class EnemyBattleData : ScriptableObject
{
    [Header("Definition")]
    public CharacterName characterName;
    public Sprite battleSprite; 
    public RuntimeAnimatorController animatorController;

    [Header("Optional Sprite Settings")]
    public bool setCustomWidthHeight = false;
    public float customWidth;
    public float customHeight;
    public bool setCustomTransformPosition = false;
    public Vector3 customTransformPosition;

    [Header("Optional Music Settings")]
    public AudioClip music;
    [Range(0f, 1f)] public float musicVolume = 1f;

    [Header("Base Stats")]
    public int baseHP;
    public int baseJuice;
    public int baseAttack;
    public int baseDefense;
    public int baseSpeed;
    // public int baseLuck;
    // this probably won't be added but leaving it here just in case

    [Header("Rewards")]
    public int expReward;
    public int clamReward;

    [Header("Actions")]
    public List<BattleActionData> attackActions;
    public List<BattleActionData> healActions;
    public List<BattleActionData> emotionActions;
    public List<BattleActionData> noneActions;
    public BattleActionData defaultAction;  // mainly used for fallback

    [Header("Action AI")]
    public List<EnemyAI> actionAI;

    [Header("Other Conditionals")]
    [Tooltip("For King Crawler: the mole that gets spawned when no moles exist. For other enemies, this can be left null or set to a different enemy data if they spawn an enemy mid-battle")] public EnemyBattleData enemyToSpawn;


    #region Action Calls
    public BattleActionData GetRandomAttackAction()
    {
        if (attackActions.Count == 0) return null;
        int index = Random.Range(0, attackActions.Count);
        return attackActions[index];
    }

    public BattleActionData GetRandomHealAction()
    {
        if (healActions.Count == 0) return null;
        int index = Random.Range(0, healActions.Count);
        return healActions[index];
    }

    public BattleActionData GetRandomEmotionAction()
    {
        if (emotionActions.Count == 0) return null;
        int index = Random.Range(0, emotionActions.Count);
        return emotionActions[index];
    }

    public BattleActionData GetRandomNoneAction()
    {
        if (noneActions.Count == 0) return null;
        int index = Random.Range(0, noneActions.Count);
        return noneActions[index];
    }
    #endregion
}


#region Enemy AI
[System.Serializable]
public class EnemyAI
{
    public BattleActionData action;

    [Header("Priority Order")]
    [Tooltip("The lower the number, the higher the priority")] public int priorityNumber;

    [Header("Emotion Chances")]
    public float neutralChance = 1f;
    public float happyChance = 1f;
    public float sadChance = 1f;
    public float angryChance = 1f;

    [Header("Conditionals")]
    [Range(0f, 1f)] public float hpBelowPercent = 1f; 
    public bool requireHpBelow;  // just for explode for now
    [Tooltip("If true, this action will always be used if reached. This should be the lowest (highest #) priority action")] public bool alwaysUseIfReached;
}
#endregion


