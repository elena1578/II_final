using UnityEngine;
using System.Collections.Generic;


public class BattleActionResult
{
    public BattleActor actor;
    public List<BattleActor> targets = new();

    public int damage;
    public int heal;
    public EmotionType? emotion;  // nullable = option for actions that don't involve emotion changes

    public bool didDamage => damage > 0;
    public bool didHeal => heal > 0;
    public bool didEmotion => emotion.HasValue;
    public bool didCrit;

    public static BattleActionResult None(BattleActor actor)
    {
        return new BattleActionResult
        {
            actor = actor,
            targets = null,
            damage = 0,
            heal = 0,
            emotion = null,
            didCrit = false
        };
    }
}
