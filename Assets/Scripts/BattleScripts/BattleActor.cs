using UnityEngine;

public abstract class BattleActor
{
    public CharacterName name { get; protected set; }
    public int maxHP { get; protected set; }
    public int currentHP { get; protected set; }
    public float hpPercent => (float)currentHP / maxHP;

    public int maxJuice { get; protected set; }
    public int currentJuice { get; protected set; }

    public int atk { get; protected set; }
    public int def { get; protected set; }
    public int speed { get; protected set; }

    public EmotionType currentEmotion { get; protected set; }
    public bool isAlive => currentHP > 0;
    protected float damageMultiplier = 1f;

    public BattleActorUI ui { get; set; }
    protected bool hasHasNotSuccumbed = false;

    public virtual void InitializeFromData(
        CharacterName name,
        int maxHP,
        int maxJuice,
        int atk,
        int def,
        int speed,
        EmotionType startingEmotion
    )
    {
        this.maxHP = maxHP;
        this.currentHP = maxHP;

        this.maxJuice = maxJuice;
        this.currentJuice = maxJuice;

        this.atk = atk;
        this.def = def;
        this.speed = speed;

        currentEmotion = startingEmotion;
        this.name = name;
    }

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

        currentHP = Mathf.Max(0, currentHP - hpDamage);
        currentJuice = Mathf.Max(0, currentJuice - juiceDamage);

        Debug.Log($"[BattleActor - Damage] {GetType().Name} took {hpDamage} HP and {juiceDamage} juice");

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
                Debug.Log("Omori has not succumbed!");
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
        Debug.Log($"[BattleActor] {GetType().Name} is toast!");
        ui?.SetToastAnimation();
    }

    public virtual void Heal(int amount)
    {
        if (!isAlive)
            return;

        int healAmount = Mathf.Max(0, amount);
        currentHP = Mathf.Min(maxHP, currentHP + healAmount);
        ui?.UpdateAll();
    }

    public virtual bool SpendJuice(int amount)
    {
        if (amount <= 0)
            return true;

        if (currentJuice < amount)
            return false;

        currentJuice -= amount;
        ui?.UpdateAll();
        return true;
    }

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

    public void EnableGuard(float multiplier)
    {
        damageMultiplier = multiplier;
    }

    public abstract BattleActionData DecideAction(BattleContext context);
}

