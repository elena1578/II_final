using UnityEngine;
using UnityEngine.EventSystems;


public class ClickBattleActorUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private BattleActor actor;
    private bool targetable;
    private BattleActorUI actorUI;

    public void Bind(BattleActor actor)
    {
        this.actor = actor;
        actorUI = GetComponentInParent<BattleActorUI>();

        Debug.Log($"[ClickBattleActorUI] Bound {actor?.name} | actorUI found: {actorUI != null}");
    }

    public void SetTargetable(bool value)
    {
        targetable = value;

        // ensure highlight resets when targeting is disabled
        if (!value && actorUI != null)
            actorUI.ResetHighlight();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!targetable)
            return;

        if (!TargetingController.instance.IsTargeting)
            return;

        if (actor == null || !actor.isAlive)  // can't select if null/dead
            return;

        TargetingController.instance.SelectTarget(actor);
        Debug.Log($"[ClickBattleActorUI] Clicked {actor.name} | waiting: {TargetingController.instance.IsTargeting} | targetable: {targetable}");
    }


    #region Tooltip + Highlight
    // used for enemy UI to show name + health bar on hover
    // for fun could also be added to portrait UIs to show detailed info like buffs, suggested actions, etc.
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (actor is EnemyBattleActor enemy)
            EnemyTooltipUI.instance.Show(enemy);

        if (targetable && actorUI != null)
            actorUI.SetHoverHighlight(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (actor is EnemyBattleActor)
            EnemyTooltipUI.instance.Hide();

        if (targetable && actorUI != null)
            actorUI.SetHoverHighlight(false);
    }
    #endregion
}