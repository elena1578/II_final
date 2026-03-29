using UnityEngine;
using System.Collections.Generic;

public class PlayerBattleActor : BattleActor
{
    public CharacterBattleData characterData;
    public List<BattleActionData> AllActions => characterData.allActions;
    public BattleActionData DefaultAttack => characterData.defaultAttack;


    public PlayerBattleActor(CharacterBattleData data)
    {
        characterData = data;
        maxEmotionTier = data.maxEmotionTier;

        // pull starting HP and juice from BattlePartyDataManager
        // which tracks runtime HP/juice for party members (vs. base HP/juice in CharacterBattleData)
        int startingHP = BattlePartyDataManager.instance.GetHP(data.characterName);
        int startingJuice = BattlePartyDataManager.instance.GetJuice(data.characterName);

        InitializeFromData(
            name: data.characterName,
            maxHP: data.baseHP,
            maxJuice: data.baseJuice,
            atk: data.baseAttack,
            def: data.baseDefense,
            speed: data.baseSpeed,
            startingEmotion: EmotionType.Neutral,
            currentHP: startingHP,
            currentJuice: startingJuice
        );

        Debug.Log($"[PlayerBattleActor Created] {characterData.characterName} with {currentHP}/{maxHP} HP, {currentJuice}/{maxJuice} juice, {def} DEF, {speed} SPD.");
    }

    public override BattleActionData DecideAction(BattleContext context)
    {
        // player decides via UI, skip
        return null;
    }
}
