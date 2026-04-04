using UnityEngine;
using System.Collections.Generic;


public abstract class BattleActor
{
    public CharacterName name { get; protected set; }

    // HP
    public int maxHP { get; protected set; }
    public int currentHP { get; protected set; }
    public float hpPercent => (float)currentHP / maxHP;
    public bool isAlive => currentHP > 0;
    protected bool hasHasNotSuccumbed = false;

    // juice
    public int maxJuice { get; protected set; }
    public int currentJuice { get; protected set; }

    // stats
    public int atk { get; protected set; }
    public int def { get; protected set; }
    public int speed { get; protected set; }
    protected int baseAtk;
    protected int baseDef;
    protected int baseSpeed;

    // emotion
    public EmotionType currentEmotion { get; protected set; }
    public int currentEmotionTier { get; protected set; } = 1;
    public int maxEmotionTier { get; protected set; } = 1;  // set in scriptables

    // stat changes
    protected float damageMultiplier = 1f;
    [HideInInspector] public List<ActiveStatModifier> activeStatModifiers = new();

    // other
    [HideInInspector] public bool moveFirst = false;
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

        // if sad/depressed/miserable, convert portion of HP dmg to juice dmg
        if (currentEmotion == EmotionType.Sad || currentEmotion == EmotionType.Depressed || currentEmotion == EmotionType.Miserable)
            ConvertToJuiceDamage(ref hpDamage, ref juiceDamage, finalDamage);

        // apply damage
        currentHP = Mathf.Max(0, currentHP - hpDamage);
        currentJuice = Mathf.Max(0, currentJuice - juiceDamage);

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

    /// <summary>
    /// converts a portion of incoming HP damage to juice damage if actor is sad, depressed, or miserable
    /// </summary>
    private void ConvertToJuiceDamage(ref int hpDamage, ref int juiceDamage, int incomingDamage)
    {
        float juicePercent = 0f;

        // percentage based off tier (30% / 50% / 100% for sad / depressed / miserable)
        switch (currentEmotion)
        {
            case EmotionType.Sad:       
                juicePercent = 0.3f; 
                break;
            case EmotionType.Depressed: 
                juicePercent = 0.5f; 
                break;
            case EmotionType.Miserable: 
                juicePercent = 1f;   
                break;
            default:                    
                juicePercent = 0f;   
                break;
        }

        // calc juice & hp damage
        juiceDamage = Mathf.RoundToInt(incomingDamage * juicePercent);
        hpDamage = incomingDamage - juiceDamage;
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
    public virtual void SetEmotion(EmotionType newEmotion)
    {
        if (newEmotion == currentEmotion)
        {
            // if same emotion, increase tier if not at max
            if (currentEmotionTier < maxEmotionTier)
                currentEmotionTier++;
        }
        else
        {
            // if new emotion, reset to tier 1
            currentEmotion = newEmotion;
            currentEmotionTier = 1;
        }

        ui?.SetEmotionAnimation(currentEmotion, currentEmotionTier);
        Debug.Log($"[BattleActor] {name} emotion set to {currentEmotion} at tier {currentEmotionTier}");
    }

    public EmotionType SetRandomEmotion()
    {
        EmotionType randomEmotion = EmotionSystem.GetRandomEmotionGrouping();
        SetEmotion(randomEmotion);
        Debug.Log($"[BattleActor] {name} had a random emotion applied: {randomEmotion}");
        return randomEmotion;
    }

    public int GetModifiedSpeedFromEmotion()
    {
        float multiplier = EmotionSystem.GetSpeedMultiplier(currentEmotion);
        return Mathf.RoundToInt(speed * multiplier);
    }

    public bool IsMaxEmotion()
    {
        return currentEmotionTier >= maxEmotionTier;
    }

    public bool IsMaxTierOneEmotion()
    {
        return currentEmotionTier == 1 && maxEmotionTier == 1;
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
        // check if modifier already exists
        var existing = activeStatModifiers.Find(m => m.type == statChangeType);

        if (existing != null)
        {
            // refresh duration instead of stacking
            existing.remainingTurns = duration;

            // also update multiplier in case of dif val 
            // (e.g., applying Attack Up again while already under Attack Up buff should refresh duration &
            //  update multiplier to new val)
            existing.multiplier = multiplier; 

            Debug.Log($"[BattleActor - SetStatChange] Refreshed existing {statChangeType} modifier with multiplier {multiplier} for {duration} turns");
        }
        else
        {
            // add modifier if new
            activeStatModifiers.Add(new ActiveStatModifier
            {
                type = statChangeType,
                multiplier = multiplier,
                remainingTurns = duration
            });
        }

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

        Debug.Log($"[BattleActor] {name} stats recalculated: ATK={atk}, DEF={def}, SPD={speed} with {activeStatModifiers.Count} active modifiers");
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
            Debug.Log($"[BattleActor] {name} will move first with action {actionData.name}");
        }
    }

    public abstract BattleActionData DecideAction(BattleContext context);
}


#region ActiveStatModifier
public class ActiveStatModifier
{
    public BattleActionData.StatChangeType type;
    public float multiplier;
    public int remainingTurns;
}
#endregion


