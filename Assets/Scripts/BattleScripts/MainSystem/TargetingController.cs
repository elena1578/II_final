using UnityEngine;


public class TargetingController : MonoBehaviour
{
    public static TargetingController instance;
    public MainCommandButtons mainCommandButtons;
    public bool IsTargeting { get; private set; }
    public BattleActionData PendingAction { get; private set; }
    private BattleActor actingActor;

    private System.Action<BattleActor> onTargetSelected;

    private void Awake()
    {
        instance = this;
    }

    public void BeginTargeting(BattleActor actor, BattleActionData action, System.Action<BattleActor> callback)
    {
        BattleUIManager ui = BattleManager.instance.uiManager;
        
        actingActor = actor;
        PendingAction = action;
        onTargetSelected = callback;

        IsTargeting = true;
        mainCommandButtons.HideAllCommands();  // hide all command buttons while targeting
        BattleDialogManager.instance.Show("Select a target!");

        // enable valid target UIs based on action's valid target groups
        ui.EnableEnemyTargeting(false);
        ui.EnableAllyTargeting(false);

        if (action.validTargets.HasFlag(TargetGroup.Enemies))
            ui.EnableEnemyTargeting(true);

        if (action.validTargets.HasFlag(TargetGroup.Allies))
            ui.EnableAllyTargeting(true);

        if (action.validTargets == TargetGroup.Self)
            SelectTarget(actor);
    }
    public void SelectTarget(BattleActor target)
    {
        if (!IsTargeting)
            return;

        BattleUIManager ui = BattleManager.instance.uiManager;

        IsTargeting = false;

        ui.EnableEnemyTargeting(false);
        ui.EnableAllyTargeting(false);

        var callback = onTargetSelected;
        callback?.Invoke(target);

        // reset targeting state
        onTargetSelected = null;
        PendingAction = null;
        actingActor = null;
    }
}