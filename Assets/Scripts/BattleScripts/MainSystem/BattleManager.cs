using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class BattleManager : MonoBehaviour
{
    [Header("Refs")]
    public BattleUIManager uiManager;
    public MainCommandButtons commandButtons;
    [Space]
    [Tooltip("How long to wait between dialog completing and next action starting")] public float postDialogDelay = 1f;
    
    public static BattleManager instance;
    public BattleActor currentActor => context.currentActor;
    private List<(BattleActor actor, BattleActionData action, BattleActor target)> plannedActions;
    private int planningIndex = 0;
    public bool HasActiveBattle { get; private set; }
    private const float fleeChance = 0.5f;
    private bool fleeSuccessful;

    // runtime systems
    private TurnStateMachine turnStateMachine;
    private TurnOrderManager turnOrder;
    private BattleContext context;
    private BattleResult battleResult;
    private ScreenShake screenShake;
    private UIDigitSpawner digitSpawner;


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        plannedActions = new List<(BattleActor, BattleActionData, BattleActor)>();
    }

    #region Setup
    /// <summary>
    /// sets conttext (party, enemies) & initializes turn order and state machine
    /// </summary>
    /// <param name="party"></param>
    /// <param name="enemies"></param>
    private void InitializeBattle(List<BattleActor> party, List<BattleActor> enemies)
    {
        InputSystemManager.instance.SwapToBattleMap();
        
        // establish context + turn order
        context = new BattleContext(party, enemies);
        turnOrder = new TurnOrderManager();
        turnOrder.BuildQueue(GetAllActors());

        // reset planning for new battle
        planningIndex = 0;
        plannedActions.Clear();

        // set up state machine + enter first state
        turnStateMachine = new TurnStateMachine(OnStateEntered);
        turnStateMachine.EnterState(BattleState.Start);

        Debug.Log("[BattleManager] Battle details: " + $"{party.Count} party members vs {enemies.Count} enemies. Enemy types: " + string.Join(", ", enemies.ConvertAll(e => e.name)));

        // bind UIs
        uiManager.BindActors(context.party, context.enemies);
        uiManager.UpdateAll();

        // cache refs
        screenShake = FindFirstObjectByType<ScreenShake>();
        digitSpawner = FindFirstObjectByType<UIDigitSpawner>();
    }

    /// <summary>
    /// called by BattleInitializer on Start to set party & enemy data
    /// </summary>
    /// <param name="partyData"></param>
    /// <param name="enemyData"></param>
    public void StartBattle(List<CharacterBattleData> partyData, List<EnemyBattleData> enemyData)
    {
        if (HasActiveBattle)
            return;

        HasActiveBattle = true;

        List<BattleActor> party = new();
        List<BattleActor> enemies = new();

        foreach (var data in partyData)
            party.Add(new PlayerBattleActor(data));

        foreach (var data in enemyData)
            enemies.Add(new EnemyBattleActor(data));

        InitializeBattle(party, enemies);
    }

    /// <summary>
    /// helper that sets list of all actors (characters + enemies) for turn order
    /// </summary>
    /// <returns></returns>
    public List<BattleActor> GetAllActors()
    {
        List<BattleActor> all = new List<BattleActor>();
        all.AddRange(context.party);
        all.AddRange(context.enemies);
        return all;
    }

    private void OnStateEntered(BattleState state)
    {
        switch (state)
        {
            case BattleState.Start:
                // Debug.Log("[BattleManager] Battle started");
                turnStateMachine.EnterState(BattleState.ActorTurn);
                break;

            case BattleState.ActorTurn:
                HandleCurrentActorTurn();
                break;

            case BattleState.ResolveTurn:
                ResolveEmotions();

                if (!CheckBattleEnd())
                    EndTurn();
                break;

            case BattleState.End:
                EndBattle(battleResult.win);
                break;
        }
    }
    public void RebuildTurnOrder() => turnOrder.BuildQueue(GetAllActors());
    #endregion


    #region Actor Planning
    public void HandleCurrentActorTurn()
    {
        // all party members plan first
        if (planningIndex < context.party.Count)
        {
            context.currentActor = context.party[planningIndex];

            // check if alive, if yes, proceed with planning
            // if no, skip to next actor's turn
            if (!context.currentActor.isAlive)
            {
                planningIndex++;
                HandleCurrentActorTurn();
                return;
            }

            // if character actor, show action buttons + prompt dialog
            if (context.currentActor is PlayerBattleActor player)
            {
                uiManager.SetActiveActor(player);
                uiManager.PopulateActionButtons(player);
                commandButtons.ShowMainCommands();
                BattleDialogManager.instance.ShowPlanningPrompt(player);
            }

            return; 
        }

        // once all character actors have planned, enemies plan/decide their actions
        foreach (var enemy in context.enemies)
        {
            if (!enemy.isAlive)
                continue;  // skip dead

            // also check if already has an action in plannedActions
            if (plannedActions.Exists(p => p.actor == enemy))
                continue;

            BattleActionData action = enemy.DecideAction(context);
            plannedActions.Add((enemy, action, null));
        }

        // execute all planned actions once, ordered by speed
        ResolvePlannedActions();  
    }

    /// <summary>
    /// helper to check if it's currently the player's turn
    /// </summary>
    public bool IsPlayerTurn => context.currentActor is PlayerBattleActor;

    /// <summary>
    /// used for 'attack' button in UI
    /// </summary>
    public void SelectDefaultAttack()
    {
        if (context.currentActor is not PlayerBattleActor player)
            return;

        TargetingController.instance.BeginTargeting(
            player,
            player.DefaultAttack,
            OnTargetSelected
        );
    }

    public void OnPlayerSelectedAction(BattleActionData actionData)
    {
        if (actionData.validTargets != TargetGroup.None)
        {
            TargetingController.instance.BeginTargeting(context.currentActor, actionData, OnTargetSelected);
            return;
        }

        CommitAction(actionData, null);
    }

    public void OnTargetSelected(BattleActor target) => CommitAction(TargetingController.instance.PendingAction, target);
    public void CommitAction(BattleActionData action, BattleActor target)
    {
        context.currentActor.CheckIfMovingFirst(action);  // set moveFirst flag here if applicable
        
        plannedActions.Add((context.currentActor, action, target));
        planningIndex++;

        commandButtons.HideAllCommands();
        turnStateMachine.EnterState(BattleState.ActorTurn);  // proceed to next actor's turn (player or enemy)
    }

    public void CheckIfMovingFirst(BattleActionData actionData)
    {
        if (actionData.alwaysMoveFirst)
            turnOrder.SetActorToMoveFirst(context.currentActor);
    }
    #endregion


    #region Round Resolution
    private void ResolvePlannedActions()
    {
        commandButtons.HideAllCommands();
        uiManager.ClearActiveActor();  // hide select frame

        // order planned actions by speed stat (highest speed goes first)
        plannedActions.Sort((a, b) =>
        {
            // moveFirst overrides speed
            if (a.actor.moveFirst && !b.actor.moveFirst) return -1;
            if (!a.actor.moveFirst && b.actor.moveFirst) return 1;

            // otherwise sort by speed like normal
            return b.actor.speed.CompareTo(a.actor.speed);
        });
        StartCoroutine(ResolveActionsRoutine());
    }

    private IEnumerator ResolveActionsRoutine()
    {
        // make copy to iterate over if plannedActions gets modified mid-round (e.g., from actor deaths)
        var actionsToResolve = new List<(BattleActor actor, BattleActionData action, BattleActor target)>(plannedActions);
        
        foreach (var entry in actionsToResolve)
        {
            if (entry.actor == null || entry.action == null || !entry.actor.isAlive)
                continue;

            // determine targets for action
            List<BattleActor> targets = entry.target != null 
                ? new List<BattleActor> { entry.target } 
                : context.GetActionTargets(entry.actor, entry.action);

            // remove null/dead targets
            targets.RemoveAll(t => t == null || !t.isAlive);

            if (targets.Count == 0)
            {
                // try retargeting if action was a single-target enemy action
                if (!entry.action.multiTarget && entry.action.validTargets.HasFlag(TargetGroup.Enemies))
                {
                    BattleActor newTarget = GetRandomValidEnemyTarget(entry.actor);

                    if (newTarget != null)
                    {
                        Debug.Log($"[BattleManager] {entry.actor.name} retargeted to {newTarget.name}");
                        targets = new List<BattleActor> { newTarget };
                    }
                    else
                    {
                        Debug.Log($"[BattleManager] {entry.actor.name}'s action cancelled (no targets)");
                        continue;
                    }
                }
                else
                {
                    Debug.Log($"[BattleManager] {entry.actor.name}'s action cancelled (no targets)");
                    continue;
                }
            }

            // double-check actor & action before use
            if (entry.actor == null || entry.action == null)
                continue;

            Debug.Log($"[BattleManager] {entry.actor.name} uses {entry.action.actionName}");

            // 1. play action animation & sound + apply logic
            float animTime = 0f;
            if (BattleAnimationController.instance != null)
                animTime = BattleAnimationController.instance.PlayAction(entry.actor, entry.action, targets);

            if (entry.action.audioClip != null)
                AudioManager.instance.PlaySFX(entry.action.audioClip, entry.action.clipVolume);

            BattleActionResult result = UseAction(entry.actor, entry.action, targets);
            if (result == null) continue;  // just in case

            // 1a. trigger shake + spawn dmg digits if dmg'd
            if (result.didDamage)
            {
                screenShake?.Shake(0.25f, result.didCrit ? 25f : 15f);

                // only spawn if alive & has UI 
                // (e.g., if target died earlier in round from another action, don't spawn dmg digits for them)
                foreach (var target in result.targets ?? new List<BattleActor>())  // null check just in case
                {
                    if (target != null && target.isAlive && target.ui != null && target.ui.gameObject.activeInHierarchy)
                        digitSpawner.SpawnDamage(result, target.ui.GetComponent<RectTransform>());
                }
            }

            // 1b. if any targets died from this action, play death animation + slide off screen
            foreach (var target in result.targets)
            {
                if (target != null && !target.isAlive && target.ui != null)
                {
                    target.ui.SetToastAnimation();
                    target.ui.SlideOffScreen();
                }
            }

            // 2. show battle dialog
            BattleDialogManager.instance.Show(entry.action, result);
            yield return new WaitForSeconds(animTime);  // wait for animation to complete (def'd in BattleActionData)

            // 3. wait for dialog to finish + small delay after
            while (BattleDialogManager.instance.typing)
                yield return null;

            yield return new WaitForSeconds(postDialogDelay);
        }

        // clear planned actions & reset planning index for next round
        plannedActions.Clear();
        planningIndex = 0;

        // check round events
        yield return HandleEndOfRoundEvents();

        uiManager.UpdateAll();
        if (!CheckBattleEnd())
            turnStateMachine.EnterState(BattleState.ActorTurn);
    }

    /// <summary>
    /// helper to use a BattleAction via BattleActionAssignment factory
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="actionData"></param>
    /// <param name="targets"></param>
    /// <returns></returns>
    public BattleActionResult UseAction(BattleActor actor, BattleActionData actionData, List<BattleActor> targets)
    {
        IBattleAction action = BattleActionAssignment.Create(actionData);
        return action.UseAction(context, actor, targets);
    }

    /// <summary>
    /// helper to remove planned actions for an actor (e.g., if they die mid-round).
    /// can also be used later to reselect an actor's action before all party members have committed 
    /// </summary>
    /// <param name="actor"></param>
    public void RemoveActorFromPlans(BattleActor actor) => plannedActions.RemoveAll(p => p.actor == actor);

    /// <summary>
    /// helper to get random valid target for an actor based on whether they're a player or enemy.
    /// used for if an enemy dies mid-round but there's still (a) target(s) that the action can be applied to
    /// </summary>
    /// <param name="actor"></param>
    /// <returns></returns>
    private BattleActor GetRandomValidEnemyTarget(BattleActor actor)
    {
        List<BattleActor> validTargets;

        // if actor is player, target enemies
        if (context.party.Contains(actor))
            validTargets = context.enemies.FindAll(e => e.isAlive);
        else
            validTargets = context.party.FindAll(p => p.isAlive);

        if (validTargets.Count == 0)
            return null;

        return validTargets[Random.Range(0, validTargets.Count)];
    }

    private bool CheckBattleEnd()
    {
        // check if all enemies or all party members are dead, if yes, end battle & set result
        bool enemiesAlive = context.enemies.Exists(e => e.isAlive);
        bool partyAlive = context.party.Exists(p => p.isAlive);

        if (!enemiesAlive)
        {
            battleResult = CreateBattleResult(true);
            turnStateMachine.EnterState(BattleState.End);
            return true;
        }

        if (!partyAlive)
        {
            battleResult = CreateBattleResult(false);
            turnStateMachine.EnterState(BattleState.End);
            return true;
        }

        return false;
    }

    private void ResolveEmotions()
    {
        // i don't think i need this function anymore but will keep here just in case
    }
   
    /// <summary>
    /// handles end of round events such as king crawler spawning sprout moles, decrementing stat mod durations, etc.
    /// </summary>
    /// <returns></returns>
    private IEnumerator HandleEndOfRoundEvents()
    {
        // king crawler case:
        // spawn a sprout mole at the end of each round if there are less than 2 sprout moles currently alive
        // if one is still alive at the end of the following round, use consume action to kill one sprout mole
        // and heal king crawler

        foreach (var enemy in new List<BattleActor>(context.enemies))
        {
            if (!enemy.isAlive)
                continue;

            if (enemy is EnemyBattleActor eba && eba.behavior != null)
                yield return eba.behavior.OnEndOfRound(this, context, eba);
        }

        // check stat mods and handle accordingly
        // i.e., decrement duration, remove expired mods, recalc stats if needed
        foreach (var actor in GetAllActors())
        {
            if (!actor.isAlive)
                continue;

            // only need to handle if has active stat mods
            if (actor.activeStatModifiers.Count == 0)
                continue;

            bool needsRecalc = false;

            // decrement duration of all active stat mods
            for (int i = actor.activeStatModifiers.Count - 1; i >= 0; i--)
            {
                var mod = actor.activeStatModifiers[i];
                mod.remainingTurns--;

                if (mod.remainingTurns <= 0)
                    needsRecalc = true;
            }

            if (needsRecalc)
                actor.RecalcStats();
        }

        // reset all moveFirst flags at end of round so they don't carry over to next round
        foreach (var actor in GetAllActors())
        {
            actor.moveFirst = false;

            // refresh buttons for alive player actors
            // used for if actors can no longer afford juice costs of certain actions
            if (actor is PlayerBattleActor player)
                uiManager.RefreshActionButtons(player); 
        }

        uiManager.UpdateAll();
    }

    private void EndTurn()
    {
        if (CheckBattleEnd())
            return;

        turnStateMachine.EnterState(BattleState.ActorTurn);
    }
    #endregion


    #region Battle End
    public void EndBattle(bool win) => StartCoroutine(BattleEndRoutine(win));
    private IEnumerator BattleEndRoutine(bool win)
    {
        commandButtons.HideAllCommands();

        if (win)
            yield return HandleVictorySequence();
        if (!win)
            if (fleeSuccessful)
                yield return HandleFleeSequence();
            else
                yield return HandleDefeatSequence();

        // record HP/juice changes so they carry over to the next battle
        foreach (var member in context.party)
        {
            if (member is PlayerBattleActor player)
            {
                BattlePartyDataManager.instance.SetHP(player.characterData.characterName, player.currentHP);
                BattlePartyDataManager.instance.SetJuice(player.characterData.characterName, player.currentJuice);
            }
        }

        // small pause before transition
        yield return new WaitForSeconds(4f);

        HasActiveBattle = false;
        BattleTransitionManager.instance.ReturnToOverworld();
    }

    private IEnumerator HandleVictorySequence()
    {
        // 1. enemy slides off screen
        foreach (var enemy in context.enemies)
        {
            enemy.ui?.SlideOffScreen();
        }
        yield return new WaitForSeconds(1f);
        
        // 2. party victory animations
        foreach (var actor in context.party)
        {
            if (!actor.isAlive) continue;
            actor.ui?.portraitAnimator.SetTrigger("victory");
        }
        yield return new WaitForSeconds(1f);

        // 3. calc rewards
        battleResult.expEarned = CalculateEXP();
        battleResult.clamsEarned = CalculateClams();

        // 4. show victory dialog
        BattleDialogManager.instance.Show(
            "OMORI's party was victorious!\n" +
            $"You gained {battleResult.expEarned} EXP!\n" +
            $"You gained {battleResult.clamsEarned} CLAMS!"
        );

        while (BattleDialogManager.instance.typing)
            yield return null;

        yield return new WaitForSeconds(postDialogDelay);
    }

    /// <summary>
    /// helper to calc total EXP via summing EXP rewards of all defeated enemies in battle result
    /// </summary>
    /// <returns></returns>
    private int CalculateEXP()
    {
        int total = 0;
        foreach (var enemy in battleResult.defeatedEnemies)
        {
            if (enemy is EnemyBattleActor e)
                total += e.enemyData.expReward;
        }
        return total;
    }

    /// <summary>
    /// helper to calc total clams via summing clam rewards of all defeated enemies in battle result
    /// </summary>
    /// <returns></returns>
    private int CalculateClams()
    {
        int total = 0;
        foreach (var enemy in battleResult.defeatedEnemies)
        {
            if (enemy is EnemyBattleActor e)
                total += e.enemyData.clamReward;
        }
        return total;
    }

    private IEnumerator HandleDefeatSequence()
    {
        // 1. show defeat dialog
        BattleDialogManager.instance.Show("OMORI's party was defeated...");
        while (BattleDialogManager.instance.typing)
            yield return null;

        yield return new WaitForSeconds(postDialogDelay);

        // 2. return to title screen
        BattleTransitionManager.instance.ReturnToTitleScreen();
    }

    public BattleResult CreateBattleResult(bool win)
    {
        BattleResult result = new BattleResult();
        result.win = win;
        result.defeatedEnemies = GetDefeatedEnemies();
        return result;
    }

    private List<BattleActor> GetDefeatedEnemies()
    {
        return context.enemies.FindAll(e => !e.isAlive);
    }
    #endregion


    #region Fleeing
    public void AttemptFlee()
    {
        // 50% chance to successfully flee
        // if successful, end battle with loss result & return to overworld (so player doesn't get rewards)

        fleeSuccessful = Random.value < fleeChance;  // reroll each attempt

        if (fleeSuccessful)
            EndBattle(false);  // end battle with loss result (no rewards)
        else
            StartCoroutine(HandleFleeFailure());
    }

    private IEnumerator HandleFleeFailure()
    {
        commandButtons.HideAllCommands();
        BattleDialogManager.instance.Show("Couldn't run away!");
        while (BattleDialogManager.instance.typing)
            yield return null;
        yield return new WaitForSeconds(2f);

        // after wait, proceed to next turn
        planningIndex++;  // consume this actor's turn
        turnStateMachine.EnterState(BattleState.ActorTurn);  // if flee fails, still counts as turn and proceed to next actor's turn
    }

    private IEnumerator HandleFleeSequence()
    {
        BattleDialogManager.instance.Show("OMORI's party fled from battle!");
        while (BattleDialogManager.instance.typing)
            yield return null;
        yield return new WaitForSeconds(1f);

        if (uiManager != null)
            uiManager.ShowFleeScreen();
        else 
            FindFirstObjectByType<BattleUIManager>()?.ShowFleeScreen();
    }
    #endregion
}
