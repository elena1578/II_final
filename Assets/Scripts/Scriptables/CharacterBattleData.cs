using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCharacterBattleData", menuName = "Scriptable Objects/Battle/Character Battle Data")]
public class CharacterBattleData : ScriptableObject
{
    public CharacterName characterName;
    
    [Header("Visuals")]
    public Sprite battleSprite;

    // characters will use their level 14 stats
    [Header("Base Stats")]
    public int baseHP;
    public int baseJuice;
    public int baseAttack;
    public int baseDefense;
    public int baseSpeed;
    [Range(2, 3)] public int maxEmotionTier = 2;  // everyone 2 except omori w/ 3

    [Header("Actions")]
    public BattleActionData defaultAttack;
    public BattleActionData universalGuardAction;
    public List<BattleActionData> allActions;
    public List<BattleActionData> attackActions;
    public List<BattleActionData> healActions;
    public List<BattleActionData> emotionActions;
}
