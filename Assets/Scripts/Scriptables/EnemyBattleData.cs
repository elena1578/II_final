using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyBattleData", menuName = "Scriptable Objects/Enemies/EnemyBattleData")]
public class EnemyBattleData : ScriptableObject
{
    [Header("Definition")]
    public CharacterName characterName;
    public Sprite battleSprite; 
    public Vector3 spriteScale = Vector3.one;
    public RuntimeAnimatorController animatorController;

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
