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
        damageMultiplier = 1f;  // reset after hit

        currentHP = Mathf.Max(0, currentHP - finalDamage);

        Debug.Log($"[HP CHANGE] {GetType().Name}: {currentHP}/{maxHP}");
        ui?.PlayHurtAnimation();
        ui?.UpdateAll();

        // check for death + special succumb condition for Omori
        if (currentHP <= 0)
        {
            if (name == CharacterName.Omori && !hasHasNotSuccumbed)
            {
                hasHasNotSuccumbed = true;
                currentHP = 1; 
                SetEmotion(EmotionType.Neutral);  // reset any applied emotions
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
        Debug.Log($"{GetType().Name} is toast!");
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

        Debug.Log($"{GetType().Name} emotion set to {emotion}");
    }

    public void EnableGuard(float multiplier)
    {
        damageMultiplier = multiplier;
    }

    public abstract BattleActionData DecideAction(BattleContext context);
}

