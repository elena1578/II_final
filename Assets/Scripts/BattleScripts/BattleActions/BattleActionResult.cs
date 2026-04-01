using UnityEngine;
using System.Collections.Generic;


public class BattleActionResult
{
    public BattleActor actor;
    public List<BattleActor> targets = new();

    public int damage;
    public int heal;
    public EmotionType? emotion;  // nullable = option for actions that don't involve emotion changes
    public EmotionType? previousEmotion;
    public int previousEmotionTier;

    public bool didDamage => damage > 0;
    public bool didHeal => heal > 0;
    public bool didEmotion => emotion.HasValue;
    public bool didCrit;
    public bool moveFirst;  // for actions that should always go first in the turn order

    // stat changes
    public BattleActionData.StatChangeType? statChange;
    public float statMultiplier;
    public int statChangeDuration;

    public static BattleActionResult None(BattleActor actor)
    {
        return new BattleActionResult
        {
            actor = actor,
            targets = new List<BattleActor>(),
            damage = 0,
            heal = 0,
            emotion = null,
            previousEmotion = null,
            previousEmotionTier = 0,
            moveFirst = false,
            didCrit = false,
            statChange = null,
            statMultiplier = 1f,
            statChangeDuration = 0
        };
    }
}
