using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("Refs")]
    public BattleUIManager uiManager;
    public MainCommandButtons commandButtons;
    [Space]
    [SerializeField] private float postDialogDelay = 1f;
    
    public static BattleManager instance;
    public BattleActor currentActor => context.currentActor;
    private List<(BattleActor actor, BattleActionData action)> plannedActions;
    private int planningIndex = 0;
    public bool HasActiveBattle { get; private set; }

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
        plannedActions = new List<(BattleActor, BattleActionData)>();
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
        
        context = new BattleContext(party, enemies);

        turnOrder = new TurnOrderManager();
        turnOrder.BuildQueue(GetAllActors());

        // reset planning for new battle
        planningIndex = 0;
        plannedActions.Clear();

        turnStateMachine = new TurnStateMachine(OnStateEntered);
        turnStateMachine.EnterState(BattleState.Start);

        Debug.Log("Battle details: " + $"{party.Count} party members vs {enemies.Count} enemies.");

        // bind UIs
        uiManager.BindActors(context.party, context.enemies);
        uiManager.UpdateAll();

        // cache refs
        screenShake = FindFirstObjectByType<ScreenShake>();
        digitSpawner = FindFirstObjectByType<UIDigitSpawner>();
    }

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
                Debug.Log("[BATTLE] Battle started");
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
    #endregion


    #region Battle Flow
    public void HandleCurrentActorTurn()
    {
        // all party members plan first
        if (planningIndex < context.party.Count)
        {
            context.currentActor = context.party[planningIndex];

            if (!context.currentActor.isAlive)
            {
                planningIndex++;
                HandleCurrentActorTurn();
                return;
            }

            if (context.currentActor is PlayerBattleActor player)
            {
                uiManager.SetActiveActor(player);
                uiManager.PopulateActionButtons(player);
                commandButtons.ShowMainCommands();
                BattleDialogManager.instance.ShowPlanningPrompt(player);
            }
            return;
        }

        // once all party members have planned, enemies plan
        foreach (var enemy in context.enemies)
        {
            if (!enemy.isAlive)
                continue;

            BattleActionData action = enemy.DecideAction(context);
            plannedActions.Add((enemy, action));
        }

        ResolvePlannedActions();
    }

    public bool IsPlayerTurn => context.currentActor is PlayerBattleActor;

    /// <summary>
    /// used for 'attack' button in UI
    /// </summary>
    public void SelectDefaultAttack()
    {
        if (context.currentActor is not PlayerBattleActor player)
        {
            Debug.LogError("Current actor = not player, ignoring");
            return;
        }

        BattleActionData action = player.DefaultAttack;
        OnPlayerSelectedAction(action);
    }

    public void OnPlayerSelectedAction(BattleActionData actionData)
    {
        plannedActions.Add((context.currentActor, actionData));
        planningIndex++;

        commandButtons.HideAllCommands();
        turnStateMachine.EnterState(BattleState.ActorTurn);
    }

    private void ResolvePlannedActions()
    {
        commandButtons.HideAllCommands();
        uiManager.ClearActiveActor();  // hide select frame

        plannedActions.Sort((a, b) => b.actor.speed.CompareTo(a.actor.speed));
        StartCoroutine(ResolveActionsRoutine());
    }

    private IEnumerator ResolveActionsRoutine()
    {
        foreach (var entry in plannedActions)
        {
            if (!entry.actor.isAlive)
                continue;

            List<BattleActor> targets = context.GetActionTargets(entry.actor, entry.action);

            // if no valid targets, skip action (e.g., attack when all enemies dead)
            if (targets.Count == 0)
            {
                Debug.Log($"[CANCEL] {entry.actor.name}'s action cancelled (no targets)");
                continue;
            }

            Debug.Log($"[EXECUTE] {entry.actor.name} uses {entry.action.actionName}");

            // 1. play action animation & sound + apply logic
            float animTime = BattleAnimationController.instance.PlayAction(entry.actor, entry.action, targets);
            AudioManager.instance.PlaySFX(entry.action.audioClip, entry.action.clipVolume);

            BattleActionResult result = UseAction(entry.actor, entry.action);

            // 1a. trigger shake + spawn dmg digits if dmg'd
            if (result.didDamage)
            {
                screenShake?.Shake(0.25f, result.didCrit ? 25f : 15f);
                foreach (var target in result.targets)
                {
                    if (target.ui != null)
                        digitSpawner.SpawnDamage(result, target.ui.GetComponent<RectTransform>());
                }
            }

            // 2. show battle dialog
            BattleDialogManager.instance.Show(
                entry.action.battleLogText,
                result
            );

            yield return new WaitForSeconds(animTime);

            // 3. wait for dialog to finish + small delay after
            while (BattleDialogManager.instance.typing)
                yield return null;

            yield return new WaitForSeconds(postDialogDelay);
        }

        // after all actions resolved, clear planned actions and reset for next turn
        plannedActions.Clear();
        planningIndex = 0;

        // update UIs before next turn starts
        uiManager.UpdateAll();
        turnStateMachine.EnterState(BattleState.ResolveTurn);
    }

    private BattleActionResult UseAction(BattleActor actor, BattleActionData actionData)
    {
        IBattleAction action = BattleActionAssignment.Create(actionData);
        return action.UseAction(context, actor);
    }

    private void ResolveEmotions()
    {
        // check for emotion 
        // if no, skip 
        // if yes, handle emotion accordingly
    }

    private bool CheckBattleEnd()
    {
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
        else
            yield return HandleDefeatSequence();

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

    public void AttemptFlee()
    {
        // simple flee logic: 50% chance to flee successfully
        bool fleeSuccessful = Random.value < 0.5f;

        if (fleeSuccessful)
        {
            EndBattle(false);
            BattleTransitionManager.instance.ReturnToOverworld();
            Debug.Log("Fled from battle successfully!");
        }
        else
        {
            Debug.Log("Flee attempt failed!");
            turnStateMachine.EnterState(BattleState.ResolveTurn);
        }
    }
    #endregion
}
