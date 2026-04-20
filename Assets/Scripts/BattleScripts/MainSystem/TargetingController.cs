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

        IsTargeting = false;

        BattleUIManager ui = BattleManager.instance.uiManager;
        mainCommandButtons.PostSkillSelection();  // reset command buttons to main commands after selecting target

        // disable targeting capabilities
        ui.EnableEnemyTargeting(false);
        ui.EnableAllyTargeting(false);

        var callback = onTargetSelected;
        callback?.Invoke(target);
        AudioManager.instance.PlaySelectSFX();

        // reset targeting state
        onTargetSelected = null;
        PendingAction = null;
        actingActor = null;
    }

    public void CancelTargeting()
    {
        if (!IsTargeting)
            return;

        IsTargeting = false;

        BattleUIManager ui = BattleManager.instance.uiManager;
        mainCommandButtons.PostSkillSelection();  // reset command buttons to main commands after canceling

        // disable targeting capabilities
        ui.EnableEnemyTargeting(false);
        ui.EnableAllyTargeting(false);

        // restore planning prompt
        if (actingActor != null)
            BattleDialogManager.instance.ShowPlanningPrompt(actingActor);

        // reset targeting state
        onTargetSelected = null;
        PendingAction = null;
        actingActor = null;
    }
}