using UnityEngine;

public class EmotionSystem : MonoBehaviour
{
    // https://www.omori.wiki/Battle_system#Emotions
    // emotion triangle:
    // angry > sad
    // sad > happy
    // happy > angry

    public static float GetDamageMultiplier(BattleActor attacker, BattleActor target)
    {
        if (attacker.currentEmotion == EmotionType.Neutral ||
            target.currentEmotion == EmotionType.Neutral)
            return 1f;

        bool attackerStrong =
            (attacker.currentEmotion == EmotionType.Angry && target.currentEmotion == EmotionType.Sad) ||
            (attacker.currentEmotion == EmotionType.Sad && target.currentEmotion == EmotionType.Happy) ||
            (attacker.currentEmotion == EmotionType.Happy && target.currentEmotion == EmotionType.Angry);

        bool attackerWeak =
            (target.currentEmotion == EmotionType.Angry && attacker.currentEmotion == EmotionType.Sad) ||
            (target.currentEmotion == EmotionType.Sad && attacker.currentEmotion == EmotionType.Happy) ||
            (target.currentEmotion == EmotionType.Happy && attacker.currentEmotion == EmotionType.Angry);

        int tier = GetEmotionTier(attacker.currentEmotion);

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
            _ => 0.5f
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
            _ => 0.20f
        };
    }

    private static int GetEmotionTier(EmotionType emotion)
    {
        return emotion switch
        {
            EmotionType.Happy => 1,
            EmotionType.Sad => 1,
            EmotionType.Angry => 1,
            // might add tiered emotions here later if there's time
            _ => 1
        };
    }
}
