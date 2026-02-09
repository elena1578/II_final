using UnityEngine;

[CreateAssetMenu(fileName = "NewBattleAction", menuName = "Scriptable Objects/BattleActionData")]
public class BattleActionData : ScriptableObject
{
    [Header("Action Definition")]
    public BattleActionName actionName;
    public ActionType actionType;
    public ActionTargetType targetType;
    [TextArea]
    public string battleLogText;
    [TextArea]
    public string descriptionText;

    [Header("Action Stats")]
    public int juiceCost;
    [Tooltip("Heal percentage of target's base HP, e.g., 0.2 = 20%")]
    public float healPercentage; 
    public EmotionType emotionEffect;
    [Tooltip("Damage reduction multiplier applied to the target for the next turn, e.g., 0.2 = 20% damage reduction")]
    public float damageReductionMultiplier;

    [Header("Damage")]
    public DamageFormula damageFormula;
    public int basePower;
    public float variance = 0.1f;
    public float critChance = 0.1f;
    public float critMultiplier = 1.5f;

    [Header("Animation")]
    public string animationTrigger;  // just the name of the attack, e.g., "headbutt", "sad_poem", etc.
    public float animationDuration = 1.5f;
    public AnimationTarget animationTarget;


    #region Enums
    public enum BattleActionName
    {
        Headbutt,
        Cook,
        Charm,
        Guard,
        DoNothing,
        RunAround,
        Attack,
        Trip,
        Explode,
        Scuttle,
        SadPoem,
        Stab,
        BreadSlice,
        PepTalk,
        Counter
    }

    public enum ActionType
    {
        Attack,
        Heal,
        Emotion,
        Guard,
        None
    }

    public enum AnimationTarget
    {
        Self,
        Target,
        Both
    }

    public enum DamageFormula
    {
        Flat,
        BasicAttack,
        Stab,
        BreadSlice,
        Headbutt
    }
    #endregion


    #region Damage Calcs
    public int CalculateDamage(BattleActor actor, BattleActor target, BattleActionData data)
    {
        int damage = 0;

        switch (data.damageFormula)
        {
            case DamageFormula.Flat:
                damage = data.basePower;
                break;

            // also used for counter for aubrey
            case DamageFormula.BasicAttack:
                damage = actor.atk * 2 - target.def;
                break;

            case DamageFormula.Stab:
                damage = actor.atk * 2;
                break;

            case DamageFormula.BreadSlice:
                damage = Mathf.RoundToInt(actor.atk * 2.5f) - target.def;
                break;

            case DamageFormula.Headbutt:
                damage = actor.atk * 3 - target.def;
                break;
        }

        // variance
        float variance = Random.Range(1f - data.variance, 1f + data.variance);
        damage = Mathf.RoundToInt(damage * variance);

        // crit
        if (Random.value < data.critChance)
        {
            damage = Mathf.RoundToInt(damage * data.critMultiplier);
        }

        return Mathf.Max(0, damage);
    }
    #endregion
}
