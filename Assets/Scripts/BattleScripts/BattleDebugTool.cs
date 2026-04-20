using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class BattleDebugTool : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Debug Party")]
    public List<CharacterBattleData> party;

    [Header("Debug Enemies")]
    public List<EnemyBattleData> enemies;

    [Header("Settings")]
    public bool autoStartBattle = true;

    private void Start()
    {       
        if (!autoStartBattle)
            return;

        // if battle was already started by overworld, do nothing
        if (BattleManager.instance.HasActiveBattle)
            return;

        Debug.Log("[BattleDebugTool] Starting debug battle");
        BattleManager.instance.StartBattle(party, enemies);
    }
#endif
}

