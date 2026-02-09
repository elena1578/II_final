using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


#if UNITY_EDITOR
public class BattleDebugTool : MonoBehaviour
{
    [Header("Debug Party")]
    public List<CharacterBattleData> party;

    [Header("Debug Enemies")]
    public List<EnemyBattleData> enemies;

    [Header("Settings")]
    public bool autoStartBattle = true;

    private bool speedUp = false;
    private InputAction toggleSpeedUpAction;

    private void Start()
    {
        toggleSpeedUpAction = InputSystemManager.instance.actions["ToggleSpeedUp"];
        
        if (!autoStartBattle)
            return;

        // if battle was already started by overworld, do nothing
        if (BattleManager.instance.HasActiveBattle)
            return;

        Debug.Log("[BattleDebugTool] Starting debug battle");

        BattleManager.instance.StartBattle(party, enemies);
    }

    private void Update()
    {
        if (toggleSpeedUpAction.triggered && !speedUp)
            SpeedUp();
        else if (toggleSpeedUpAction.triggered && speedUp)
            ResetSpeedUp();
    }

    private void SpeedUp()
    {
        Time.timeScale = 5f;
        speedUp = true;
        Debug.Log("[BattleDebugTool] Speed up enabled");
    }

    private void ResetSpeedUp()
    {
        Time.timeScale = 1f;
        speedUp = false;
        Debug.Log("[BattleDebugTool] Speed up disabled");
    }
}
#endif
