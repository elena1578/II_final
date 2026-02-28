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

        InitializeFromData(
            name: data.characterName,
            maxHP: data.baseHP,
            maxJuice: data.baseJuice,
            atk: data.baseAttack,
            def: data.baseDefense,
            speed: data.baseSpeed,
            startingEmotion: EmotionType.Neutral
        );

        Debug.Log($"[PlayerBattleActor Created] {characterData.characterName} with {maxHP} HP, {atk} ATK, {def} DEF, {speed} SPD.");
    }

    public override BattleActionData DecideAction(BattleContext context)
    {
        // player decides via UI, skip
        return null;
    }
}
