using System.Collections.Generic;
using UnityEngine;


public enum EnemySpawnPosition
{
    Center,
    Left,
    Right
}


public class BattleUIManager : MonoBehaviour
{
    [Header("Character Party UI")]
    [SerializeField] private List<BattleActorUI> partySlots;

    [Header("Enemy UI")]
    [SerializeField] private Transform enemyContainer;
    [SerializeField] private BattleActorUI enemySlotPrefab;
    [SerializeField] private Transform centerPosition;
    [SerializeField] private Transform leftPosition;
    [SerializeField] private Transform rightPosition;

    [Header("Action Buttons")]
    [SerializeField] private List<BattleActionButton> actionButtons;

    private List<BattleActorUI> enemySlots = new();


    #region Portraits
    public void BindActors(List<BattleActor> party, List<BattleActor> enemies)
    {
        // iterate through party & bind to UI 
        for (int i = 0; i < party.Count; i++)
        {
            var actor = party[i];
            var ui = partySlots[i];

            actor.ui = ui;

            var player = actor as PlayerBattleActor;
            ui.Bind(actor, player.characterData.battleSprite);
        }

        // instantiate enemy UI elements & bind
        foreach (var enemy in enemies)
        {
            var enemyActor = enemy as EnemyBattleActor;

            // default first enemy = center
            EnemySpawnPosition spawnPos = EnemySpawnPosition.Center;

            if (IsPositionOccupied(EnemySpawnPosition.Center))
                spawnPos = EnemySpawnPosition.Left;

            if (IsPositionOccupied(EnemySpawnPosition.Left))
                spawnPos = EnemySpawnPosition.Right;

            AddEnemyUI(enemyActor, spawnPos);
        }
    }

    public void UpdateAll()
    {
        foreach (var actor in BattleManager.instance.GetAllActors())
            actor.ui?.UpdateAll();
    }

    public void SetActiveActor(PlayerBattleActor actor)
    {
        foreach (var partyActor in BattleManager.instance.GetAllActors())
            partyActor.ui?.SetTurnActive(false);

        actor.ui?.SetTurnActive(true);
    }

    public void ClearActiveActor()
    {
        foreach (var actor in BattleManager.instance.GetAllActors())
        {
            actor.ui?.SetTurnActive(false);
        }
    }

    /// <summary>
    /// adds a new enemy UI element for a newly spawned enemy (e.g., king crawler mole) and binds it to the enemy actor
    /// </summary>
    /// <param name="enemy"></param>
    public void AddEnemyUI(EnemyBattleActor enemy, EnemySpawnPosition spawnPosition = EnemySpawnPosition.Center)
    {
        // if center pos is taken, try left, then right 
        // if both taken, default to center (will overlap but at least ensures new enemy gets a UI)
        if (spawnPosition == EnemySpawnPosition.Center && IsPositionOccupied(EnemySpawnPosition.Center))
        {
            spawnPosition = !IsPositionOccupied(EnemySpawnPosition.Left)
                ? EnemySpawnPosition.Left
                : EnemySpawnPosition.Right;
        }

        Transform parent = GetAnchor(spawnPosition);

        var ui = Instantiate(enemySlotPrefab, parent);
        enemy.ui = ui;
        enemySlots.Add(ui);  // track enemy UIs for cleanup later

        ui.Bind(enemy, enemy.enemyData.battleSprite);

        var dataSetter = ui.GetComponent<EnemyBattleActorDataSetter>();
        if (dataSetter != null)
            dataSetter.SetBattleData(enemy.enemyData);

        ui.transform.SetAsFirstSibling();
    }

    public void RemoveEnemyUI(EnemyBattleActor enemy)
    {
        if (enemy.ui != null)
        {
            enemySlots.Remove(enemy.ui);
            Destroy(enemy.ui.gameObject);
            enemy.ui = null;
        }
    }

    public bool IsPositionOccupied(EnemySpawnPosition spawnPosition)
    {
        Transform anchor = GetAnchor(spawnPosition);
        return anchor.childCount > 0;
    }

    private Transform GetAnchor(EnemySpawnPosition pos)
    {
        return pos switch
        {
            EnemySpawnPosition.Center => centerPosition,
            EnemySpawnPosition.Left => leftPosition,
            EnemySpawnPosition.Right => rightPosition,
            _ => centerPosition
        };
    }
    #endregion


    #region Targeting
    public void EnableEnemyTargeting(bool enable)
    {
        foreach (var enemyUI in enemySlots)
        {
            enemyUI.SetTargetable(enable);
        }
    }

    public void EnableAllyTargeting(bool enable)
    {
        foreach (var partyUI in partySlots)
        {
            partyUI.SetTargetable(enable);
        }
    }

    public void EnableSelfTargeting(BattleActor selfActor)
    {
        foreach (var partyUI in partySlots)
        {
            if (partyUI == selfActor.ui)
                partyUI.SetTargetable(true);
        }
    }
    #endregion


    #region Action Buttons
    public void PopulateActionButtons(PlayerBattleActor actor)
    {
        // check actors first
        if (actor == null)
        {
            Debug.LogError("[BattleUIManager] PopulateActionButtons called w/ null actor");
            return;
        }

        foreach (var button in actionButtons)
            button.Clear();

        var actions = actor.AllActions;

        // should be 16 [actions] and 16 [buttons]
        Debug.Log($"[BattleUIManager] Populating {actions.Count} actions into {actionButtons.Count} buttons");

        // iterate through actions & bind to buttons
        for (int i = 0; i < actions.Count && i < actionButtons.Count; i++)
        {
            actionButtons[i].Initialize(actions[i]);
        }
    }
    #endregion
}
