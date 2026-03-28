using UnityEngine;
using System.Collections.Generic;


public abstract class BattleActor
{
    public CharacterName name { get; protected set; }

    public int maxHP { get; protected set; }
    public int currentHP { get; protected set; }
    public float hpPercent => (float)currentHP / maxHP;
    protected bool hasHasNotSuccumbed = false;

    public int maxJuice { get; protected set; }
    public int currentJuice { get; protected set; }

    public int atk { get; protected set; }
    public int def { get; protected set; }
    public int speed { get; protected set; }
    protected int baseAtk;
    protected int baseDef;
    protected int baseSpeed;

    public EmotionType currentEmotion { get; protected set; }
    public bool isAlive => currentHP > 0;
    protected float damageMultiplier = 1f;
    [HideInInspector] public List<ActiveStatModifier> activeStatModifiers = new();

    [HideInInspector] public bool moveFirst = false;  // for actions that should always go first in the turn order, e.g., Hero's Smile
    public BattleActorUI ui { get; set; }

    public virtual void InitializeFromData(
        CharacterName name,
        int maxHP,
        int maxJuice,
        int atk,
        int def,
        int speed,
        EmotionType startingEmotion,
        // optional current HP/juice override parameters
        int? currentHP = null,
        int? currentJuice = null
    )
    {
        this.maxHP = maxHP;
        this.currentHP = currentHP ?? maxHP;  // use passed currentHP if not null, else maxHP

        this.maxJuice = maxJuice;
        this.currentJuice = currentJuice ?? maxJuice;  // same for juice

        baseAtk = atk;
        baseDef = def;
        baseSpeed = speed;

        currentEmotion = startingEmotion;
        this.name = name;

        // copy base stats to current stats (will be modified by temp buffs/debuffs in battle)
        this.atk = baseAtk;
        this.def = baseDef;
        this.speed = baseSpeed;

        RecalcStats();
    }


    #region HP
    public virtual void TakeDamage(int amount)
    {
        if (!isAlive)
        return;

        int finalDamage = Mathf.RoundToInt(amount * damageMultiplier);
        damageMultiplier = 1f;  // reset guard multiplier after hit

        int hpDamage = finalDamage;
        int juiceDamage = 0;

        // check if sad, if yes, 30% of damage is juice damage instead of HP damage
        if (currentEmotion == EmotionType.Sad)
        {
            juiceDamage = Mathf.RoundToInt(finalDamage * 0.3f);
            hpDamage = finalDamage - juiceDamage;
        }

        // apply dmg + juice changes, ensuring neither goes below 0
        currentHP = Mathf.Max(0, currentHP - hpDamage);
        currentJuice = Mathf.Max(0, currentJuice - juiceDamage);
        Debug.Log($"[BattleActor - Damage] {GetType().Name} took {hpDamage} HP and {juiceDamage} juice");

        // record changes to BattlePartyDataManager if player actor
        if (this is PlayerBattleActor)
            BattlePartyDataManager.instance.SetHP(name, currentHP);

        // animations + UI updates
        ui?.PlayHurtAnimation();
        ui?.UpdateAll();

        // check for death + special succumb condition for Omori
        if (currentHP <= 0)
        {
            if (name == CharacterName.Omori && !hasHasNotSuccumbed)
            {
                hasHasNotSuccumbed = true;
                currentHP = 1;  // set to 1 HP instead of 0
                ui?.SetSuccumbAnimation();
                ui?.UpdateAll();  // make sure 1 HP shows up immediately UI-wise
                Debug.Log("[BattleActor] Omori has not succumbed!");
            }
            else
            {
                currentHP = 0;
                OnDeath();
            }
        }
    }

    public virtual void OnDeath()
    {
        // Debug.Log($"[BattleActor] {GetType().Name} is toast!");
        ui?.SetToastAnimation();
    }

    public virtual void Heal(int amount)
    {
        if (!isAlive)
            return;

        int healAmount = Mathf.Max(0, amount);
        currentHP = Mathf.Min(maxHP, currentHP + healAmount);

        // record changes to BattlePartyDataManager if player actor
        if (this is PlayerBattleActor)
            BattlePartyDataManager.instance.SetHP(name, currentHP);

        ui?.UpdateAll();
    }
    #endregion


    #region Juice
    public virtual bool SpendJuice(int amount)
    {
        if (amount <= 0)
            return true;

        if (currentJuice < amount)
            return false;

        currentJuice -= amount;
        ui?.UpdateAll();

        // record changes to BattlePartyDataManager if player actor
        if (this is PlayerBattleActor)
            BattlePartyDataManager.instance.SetJuice(name, currentJuice);

        return true;
    }
    #endregion


    #region Emotion
    public virtual void SetEmotion(EmotionType emotion)
    {
        currentEmotion = emotion;
        ui?.SetEmotionAnimation(emotion);

        Debug.Log($"[BattleActor] {GetType().Name} emotion set to {emotion}");
    }

    public int GetModifiedSpeedFromEmotion()
    {
        float multiplier = 1f;

        if (currentEmotion == EmotionType.Happy)
            multiplier = 1.25f;

        if (currentEmotion == EmotionType.Sad)
            multiplier = 0.8f;

        return Mathf.RoundToInt(speed * multiplier);
    }
    #endregion


    #region Stat Mods
    /// <summary>
    /// apply temp base stat modifiers (e.g., actions like Roar) via multipliers
    /// </summary>
    /// <param name="attackMult"></param>
    /// <param name="defenseMult"></param>
    /// <param name="speedMult"></param>
    public void ApplyTemporaryStatModifier(float attackMult = 1f, float defenseMult = 1f, float speedMult = 1f)
    {
        atk = Mathf.RoundToInt(atk * attackMult);
        def = Mathf.RoundToInt(def * defenseMult);
        speed = Mathf.RoundToInt(speed * speedMult);
        ui?.UpdateAll();
    }

    public void SetStatChange(BattleActionData.StatChangeType statChangeType, float multiplier, int duration)
    {
        activeStatModifiers.Add(new ActiveStatModifier
        {
            type = statChangeType,
            multiplier = multiplier,
            remainingTurns = duration
        });

        RecalcStats();
    }

   public void RecalcStats()
    {
        float modifiedAtk = baseAtk;
        float modifiedDef = baseDef;
        float modifiedSpeed = baseSpeed;

        foreach (var mod in activeStatModifiers)
        {
            switch (mod.type)
            {
                case BattleActionData.StatChangeType.AttackUp:
                case BattleActionData.StatChangeType.AttackDown:
                    modifiedAtk *= mod.multiplier;
                    break;
                case BattleActionData.StatChangeType.DefenseUp:
                case BattleActionData.StatChangeType.DefenseDown:
                    modifiedDef *= mod.multiplier;
                    break;
                case BattleActionData.StatChangeType.SpeedUp:
                case BattleActionData.StatChangeType.SpeedDown:
                    modifiedSpeed *= mod.multiplier;
                    break;
            }
        }

        // do float calcs prior to converting to int to avoid rounding down too much and ending up w/ 0 atk/def/speed
        atk = Mathf.Max(1, Mathf.RoundToInt(modifiedAtk)); 
        def = Mathf.Max(0, Mathf.RoundToInt(modifiedDef));
        speed = Mathf.Max(1, Mathf.RoundToInt(modifiedSpeed));

        Debug.Log($"[BattleActor - RecalcStats] {GetType().Name} stats recalculated: ATK={atk}, DEF={def}, SPD={speed} with {activeStatModifiers.Count} active modifiers");
        ui?.UpdateAll();
    }

    public void CountTurnsForActiveStatMods()
    {
        for (int i = activeStatModifiers.Count - 1; i >= 0; i--)
        {
            var mod = activeStatModifiers[i];
            mod.remainingTurns--;

            if (mod.remainingTurns <= 0)
                activeStatModifiers.RemoveAt(i);
            else
                activeStatModifiers[i] = mod;
        }

        RecalcStats();
    }

    public void EnableGuard(float multiplier) => damageMultiplier = multiplier;
    public void DisableGuard() => damageMultiplier = 1f;
    #endregion

    public void CheckIfMovingFirst(BattleActionData actionData)
    {
        if (actionData.alwaysMoveFirst)
        {
            moveFirst = true;  // flag actor for TurnOrderManager
            Debug.Log($"[BattleActor] {GetType().Name} will move first with action {actionData.name}");
        }
    }

    public abstract BattleActionData DecideAction(BattleContext context);
}


#region ActiveStatModifer Struct
public class ActiveStatModifier
{
    public BattleActionData.StatChangeType type;
    public float multiplier;
    public int remainingTurns;
}
#endregion


