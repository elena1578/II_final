using UnityEngine;
using System.Collections.Generic;


public class EmotionSystem : MonoBehaviour
{
    // https://www.omori.wiki/Battle_system#Emotions
    // emotion triangle:
    // angry > sad
    // sad > happy
    // happy > angry

    public static float GetDamageMultiplier(BattleActor attacker, BattleActor target)
    {
        // neutral = default dmg
        if (attacker.currentEmotion == EmotionType.Neutral ||
            target.currentEmotion == EmotionType.Neutral)
            return 1f;

        // set strong emotions (e.g., angry > sad)
        bool attackerStrong =
            (attacker.currentEmotion == EmotionType.Angry && target.currentEmotion == EmotionType.Sad) ||
            (attacker.currentEmotion == EmotionType.Sad && target.currentEmotion == EmotionType.Happy) ||
            (attacker.currentEmotion == EmotionType.Happy && target.currentEmotion == EmotionType.Angry);

        // set weak emotions (e.g., angry < happy)
        bool attackerWeak =
            (target.currentEmotion == EmotionType.Angry && attacker.currentEmotion == EmotionType.Sad) ||
            (target.currentEmotion == EmotionType.Sad && attacker.currentEmotion == EmotionType.Happy) ||
            (target.currentEmotion == EmotionType.Happy && attacker.currentEmotion == EmotionType.Angry);

        // use tier of attacker's emotion to get appropriate multiplier
        int tier = attacker.currentEmotionTier;

        if (attackerStrong)
            return 1f + GetWeaknessBonus(tier);
        if (attackerWeak)
            return 1f - GetResistanceBonus(tier);

        return 1f;
    }

    // * Emotion Weakness: Deals 50% / 100% / 150% more damage to the weaker emotion.
    private static float GetWeaknessBonus(int tier)
    {
        // 50% / 100% / 150%
        return tier switch
        {
            1 => 0.5f,
            2 => 1.0f,
            3 => 1.5f,
            _ => 0.5f  // default to 50%
        };
    }

    // * Emotion Resistance: Takes 20% / 35% / 50% less damage from the weaker emotion.
    private static float GetResistanceBonus(int tier)
    {
        // 20% / 35% / 50%
        return tier switch
        {
            1 => 0.20f,
            2 => 0.35f,
            3 => 0.50f,
            _ => 0.20f  // default to 20%
        };
    }

    private static int GetEmotionTier(EmotionType emotion)
    {
        return emotion switch
        {
            EmotionType.Happy => 1,
            EmotionType.Sad => 1,
            EmotionType.Angry => 1,
            EmotionType.Afraid => 1,
            EmotionType.Toast => 1,  // treating as 1 to reset multipliers & built-up tiers
            EmotionType.Ecstatic => 2,
            EmotionType.Depressed => 2,
            EmotionType.Enraged => 2,
            EmotionType.Manic => 3,
            EmotionType.Miserable => 3,
            EmotionType.Furious => 3,
            _ => 1
        };
    }


    #region Multiplier Helpers
    public static float GetAttackMultiplier(EmotionType emotion)
    {
        return emotion switch
        {
            // angry
            EmotionType.Angry => 1.3f,
            EmotionType.Enraged => 1.5f,
            EmotionType.Furious => 2f,
            _ => 1f
        };
    }

    public static float GetDefenseMultiplier(EmotionType emotion)
    {
        return emotion switch
        {
            // sad
            EmotionType.Sad => 1.25f,
            EmotionType.Depressed => 1.35f,
            EmotionType.Miserable => 1.5f,
            // angry
            EmotionType.Angry => 0.5f,
            EmotionType.Enraged => 0.3f,
            EmotionType.Furious => 0.15f,
            _ => 1f
        };
    }

    public static float GetSpeedMultiplier(EmotionType emotion)
    {
        return emotion switch
        {
            // happy
            EmotionType.Happy => 1.25f,
            EmotionType.Ecstatic => 1.5f,
            EmotionType.Manic => 2f,
            // sad
            EmotionType.Sad => 0.8f,
            EmotionType.Depressed => 0.65f,
            EmotionType.Miserable => 0.5f,
            _ => 1f
        };
    }

    public static float GetCritBonus(EmotionType emotion)
    {
        return emotion switch
        {
            // in base game this is luck so to compensate just giving a flat crit bonus
            // happy
            EmotionType.Happy => 0.1f,
            EmotionType.Ecstatic => 0.15f,
            EmotionType.Manic => 0.2f,
            _ => 0f
        };
    }
    #endregion


    #region Text Helpers
    public static string GetEmotionGroupingText(EmotionType emotion)
    {
        switch (emotion)
        {
            case EmotionType.Happy:
            case EmotionType.Ecstatic:
            case EmotionType.Manic:
                return "Happy";

            case EmotionType.Sad:
            case EmotionType.Depressed:
            case EmotionType.Miserable:
                return "Sad";

            case EmotionType.Angry:
            case EmotionType.Enraged:
            case EmotionType.Furious:
                return "Angry";

            case EmotionType.Neutral:
            default:
                return "";
        }
    }
}


#region EmotionText
public static class EmotionText
{
    /// <summary>
    /// maps each EmotionType to an array of strings rep'ing emotion @ each tier
    /// (e.g., Sad -> ["Sad", "Depressed", "Miserable"]).
    /// used to parse text correctly in BattleDialogManager when an emotion changes/is set
    /// </summary>
    private static readonly Dictionary<EmotionType, string[]> emotionNames = new()
    {
        { EmotionType.Sad, new[] { "Sad", "Depressed", "Miserable" } },
        { EmotionType.Angry, new[] { "Angry", "Enraged", "Furious" } },
        { EmotionType.Happy, new[] { "Happy", "Ecstatic", "Manic" } }
    };

    public static string Get(EmotionType emotion, int tier)
    {
        if (emotionNames.TryGetValue(emotion, out var tiers))
        {
            int index = Mathf.Clamp(tier - 1, 0, tiers.Length - 1);
            return tiers[index];
        }

        return "Neutral";
    }
}
#endregion
#endregion