using System.Collections.Generic;
using UnityEngine;


public class BattleUIManager : MonoBehaviour
{
    [Header("Character Party UI")]
    [SerializeField] private List<BattleActorUI> partySlots;

    [Header("Enemy UI")]
    [SerializeField] private Transform enemyContainer;
    [SerializeField] private BattleActorUI enemySlotPrefab;

    [Header("Action Buttons")]
    [SerializeField] private List<BattleActionButton> actionButtons;


    #region Portraits
    public void BindActors(List<BattleActor> party, List<BattleActor> enemies)
    {
        // party members
        for (int i = 0; i < party.Count; i++)
        {
            var actor = party[i];
            var ui = partySlots[i];

            actor.ui = ui;

            var player = actor as PlayerBattleActor;
            ui.Bind(actor, player.characterData.battleSprite);
        }

        // enemies
        foreach (var enemy in enemies)
        {
            var ui = Instantiate(enemySlotPrefab, enemyContainer);

            enemy.ui = ui;

            var enemyActor = enemy as EnemyBattleActor;
            ui.Bind(enemy, enemyActor.enemyData.battleSprite);
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
    #endregion


    #region Action Buttons
    public void PopulateActionButtons(PlayerBattleActor actor)
    {
        // check actors first
        if (actor == null)
        {
            Debug.LogError("PopulateActionButtons called with null actor");
            return;
        }

        foreach (var button in actionButtons)
            button.Clear();

        var actions = actor.AllActions;

        // should be 16 [actions] and 16 [buttons]
        Debug.Log($"Populating {actions.Count} actions into {actionButtons.Count} buttons");

        for (int i = 0; i < actions.Count && i < actionButtons.Count; i++)
        {
            actionButtons[i].Initialize(actions[i]);
        }
    }
    #endregion
}
