using UnityEngine;

[CreateAssetMenu(fileName = "NewBattleAction", menuName = "Scriptable Objects/BattleActionData")]
public class BattleActionData : ScriptableObject
{
    [Header("Action Definition")]
    public BattleActionName actionName;
    public ActionType actionType;
    public TargetGroup validTargets;
    [Tooltip("True = target all allies/enemies, false = single-target [for self, an enemy, or an ally]")] public bool multiTarget;  // for all allies/enemies
    public bool alwaysMoveFirst = false;  // for actions that should always go first in the turn order
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
    private float variance = 0.2f;
    public float critChance = 0.1f;
    [HideInInspector] public float critMultiplier = 1.5f;

    [Header("Animation")]
    public string animationTrigger;  // just the name of the attack, e.g., "headbutt", "sad_poem", etc.
    public float animationDuration = 1.5f;
    public AnimationTarget animationTarget;  // this isn't really working right now but will fix later since it'll be needed for some actions like hero healing party member(s)

    [Header("Audio")]
    public AudioClip audioClip;
    [Range(0f, 1f)] public float clipVolume = 1f;

    [Header("Stat Change Action Specific")]
    public StatChangeType statChangeType;
    [Tooltip("Expressed as a decimal multiplier, e.g., 1.5 = 150%")] public float statChangeMultiplier;
    [Tooltip("Duration of the stat change in turns")] public int statChangeDuration;


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
        Counter,
        SuddenJump,
        Roar,
        Rebound,
        RunNGun,
        Curveball,
        Screech, 
        Crunch,
        Ram,
        Consume,
        FastFood,
        Smile
    }

    public enum ActionType
    {
        Attack,
        Heal,
        Emotion,
        Guard,
        None,
        StatChange
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
        Headbutt,
        Rebound,
        RunNGun,
        Curveball,
        Crunch,
        RunAround
    }

    public enum StatChangeType
    {
        AttackUp,
        AttackDown,
        DefenseUp,
        DefenseDown,
        SpeedUp,
        SpeedDown
    }
    #endregion


    #region Damage Calcs
    public int CalculateDamage(BattleActor actor, BattleActor target, out bool didCrit)
    {
        didCrit = false;
        int damage = 0;
        int atk = GetModifiedAttackFromEmotion(actor);
        int def = GetModifiedDefenseFromEmotion(target);

        // damage formula breakdown:
        // Final Damage = {[(damage formula) * (emotion multiplier) * (critical multiplier) * (damage variance)] 
        // + additional critical damage} * (flex multiplier).

        // emotion effects:
        // happy: Multiply luck by 2, multiply speed by 1.25, decrease hit rate by 10%.
        // sad: Multiply defense by 1.25, multiply speed by 0.8, 30% of the HP damage taken is done to juice instead.
        // angry: Multiply attack by 1.3, multiply defense by 0.5.


        switch (damageFormula)
        {
            case DamageFormula.Flat:
                damage = basePower;
                break;

            // also used for aubrey's counter
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

            case DamageFormula.Rebound:
                damage = Mathf.RoundToInt((actor.atk) * 2.5f) - target.def;
                break;

            case DamageFormula.RunNGun:
                damage = Mathf.RoundToInt(actor.speed * 1.5f) - target.def;
                break;

            case DamageFormula.Curveball:
                if (actor.currentEmotion != EmotionType.Neutral)
                    damage = actor.atk * 3 - target.def;
                else
                    damage = actor.atk * 2 - target.def;
                break;

            case DamageFormula.Crunch:
                damage = actor.atk * 3 - target.def;
                break;

            case DamageFormula.RunAround:
                damage = Mathf.RoundToInt((actor.atk * 1.5f - target.def) * 2);  // technically deals dmg twice but that'll take more work so i'll add it later
                break;
        }

        // emotion multiplier
        float emotionMultiplier = EmotionSystem.GetDamageMultiplier(actor, target);
        damage = Mathf.RoundToInt(damage * emotionMultiplier);

        // variance (base game: 80-120 damage for an attack with 100 base power; 20% variance)
        float varianceRoll = Random.Range(1f - variance, 1f + variance);
        damage = Mathf.RoundToInt(damage * varianceRoll);

        // crit roll
        if (Random.value < critChance)
        {
            didCrit = true;
            damage = Mathf.RoundToInt((damage * critMultiplier) + 2);  
        }

        return Mathf.Max(0, damage);
    }

    private int GetModifiedAttackFromEmotion(BattleActor actor)
    {
        float multiplier = 1f;

        if (actor.currentEmotion == EmotionType.Angry)
            multiplier = 1.3f;

        return Mathf.RoundToInt(actor.atk * multiplier);
    }

    private int GetModifiedDefenseFromEmotion(BattleActor actor)
    {
        float multiplier = 1f;

        if (actor.currentEmotion == EmotionType.Sad)
            multiplier = 1.25f;

        if (actor.currentEmotion == EmotionType.Angry)
            multiplier = 0.5f;

        return Mathf.RoundToInt(actor.def * multiplier);
    }
    #endregion
}
