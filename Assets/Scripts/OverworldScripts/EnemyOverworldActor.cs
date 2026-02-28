using UnityEngine;


public class EnemyOverworldActor : GridMovementController
{
    public EnemyOverworldData data;
    public GameObject alertBalloonPrefab;

    // values set from data
    private float alertRadius = 5.0f;
    private float chanceToMove = 1f;

    [Header("Movement Settings")]
    [SerializeField] private float followMoveSpeed = 0.5f;
    [SerializeField] private float minMoveTime = 1.0f;
    [SerializeField] private float maxMoveTime = 3.0f;
    [SerializeField] private float minIdleTime = 0.5f;
    [SerializeField] private float maxIdleTime = 1.5f;
    private float moveTimer;
    private float idleTimer;
    private bool frozen = false;  // used to freeze enemies for a few seconds after returning/running from battle
    private float frozenTimer = 0f;
    private float frozenDuration = 3f;

    // internal refs
    private Animator animator;
    private SpriteRenderer sr;
    private bool alerted = false;
    private Transform playerTransform;
    private bool enteringBattle = false;


    protected override void Awake()
    {
        base.Awake();
        animator = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    public void InitializeData(EnemyOverworldData enemyData)
    {
        data = enemyData;
        
        if (data != null)
        {
            // sprite
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null && data.sprite != null)
                sr.sprite = data.sprite;

            // animator controller
            if (animator != null && data.animatorController != null)
                animator.runtimeAnimatorController = data.animatorController;

            // behavior 
            moveSpeed = data.moveSpeed;
            alertRadius = data.alertRadius;
            chanceToMove = data.chanceToMove;
        }

        SnapToGrid(transform.position);  // once spawned + initialized, ensure grid alignment
    }

    protected override void FixedUpdate()
    {
        if (frozen)
        {
            frozenTimer += Time.fixedDeltaTime;
            movement = Vector2.zero;

            if (frozenTimer >= frozenDuration)
            {
                Unfreeze();
                return;
            }

            UpdateWalkingAnimation();
            base.FixedUpdate();
            return;
        }

        if (alerted && playerTransform != null)
            ChasePlayer(playerTransform);
        else
            IdleAndMove();

        UpdateWalkingAnimation();
        base.FixedUpdate();
    }

    private void IdleAndMove()
    {
        // currently moving
        if (moveTimer > 0)
        {
            moveTimer -= Time.fixedDeltaTime;
            return;
        }

        // currently idling
        if (idleTimer > 0)
        {
            idleTimer -= Time.fixedDeltaTime;
            movement = Vector2.zero;
            return;
        }

        // choose next action
        ChooseNextMovementState();
    }

    /// <summary>
    /// randomly chooses the next movement state: moving in a random direction
    /// for a random duration, or idling for a random duration.
    /// this will later be expanded to more complex behavior & pathfinding
    /// </summary>
    private void ChooseNextMovementState()
    {
        // 70% chance to move, 30% chance to idle
        if (Random.value < chanceToMove)
        {
            movement = GetRandomDirection();
            moveTimer = Random.Range(minMoveTime, maxMoveTime);
        }
        else
        {
            movement = Vector2.zero;
            idleTimer = Random.Range(minIdleTime, maxIdleTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (frozen) return;  // ignore if player just returned from battle
            
            TriggerAlertBalloon();
            playerTransform = other.transform;
            alerted = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            alerted = true;  // once alerted, stays alerted
    }

    private void ChasePlayer(Transform player)
    {
        // Debug.Log(data.name + " has spotted the player!");
        
        // slow movement speed when chasing
        movement = SnapToDirection((player.position - transform.position).normalized) * followMoveSpeed;

        // override timers
        idleTimer = 0;
        moveTimer = 0;
    }

    private void TriggerAlertBalloon()
    {
        if (alertBalloonPrefab != null && !alerted)
        {
            Instantiate(alertBalloonPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity, transform);
            alerted = true;
        }
        else
            Debug.LogWarning("Alert balloon prefab not set on " + data.name);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Player") && !enteringBattle && !frozen)
        {           
            // destroy other enemies to halt movement & prevent multiple battle triggers
            // if a save/load system is implemented, this would instead be saving what enemies there are and
            // respawning them after battle rather vs. destroying them

            EnemyOverworldActor[] otherEnemies = FindObjectsByType<EnemyOverworldActor>(FindObjectsSortMode.None);
            foreach (EnemyOverworldActor enemy in otherEnemies)            
            {
                if (enemy != this)
                    Destroy(enemy.gameObject);
            }

            enteringBattle = true;

            // disable movement of this enemy and player
            DisableMovement(); 
            PlayerOverworldController player = other.collider.GetComponent<PlayerOverworldController>();
            if (player != null)
                player.DisablePlayerMovement();

            Debug.Log("Beginning battle with " + data.name);
            BattleTransitionManager.instance.StartBattle(data);
        }
    }

    // this is slightly outdated/broken (i.e. fix animation flags to be more sim to playercontroller)
    private void UpdateWalkingAnimation()
    {
        bool isMoving = movement.sqrMagnitude > 0.01f;
        animator.SetBool("isMoving", isMoving);

        if (isMoving)
        {
            animator.SetFloat("moveX", movement.x);
            animator.SetFloat("moveY", movement.y);
        }
        else
        {
            animator.SetFloat("moveX", lastDirection.x);
            animator.SetFloat("moveY", lastDirection.y);
        }
    }


    #region Helpers
    /// <summary>
    /// used for battle transitioning, stops movement & resets internal timers
    /// </summary>
    private void DisableMovement()
    {
        movement = Vector2.zero;
        moveTimer = 0f;
        idleTimer = 0f;
        isMoving = false;

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public void FreezeForDuration(float duration)
    {
        frozen = true;
        frozenDuration = duration;
        frozenTimer = 0f;
        
        movement = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic; 

        PauseAnimations();

        // set false while frozen to further prevent immediate battle re-triggering
        alerted = false;
        enteringBattle = false;

        // give sprite gray color to indicate frozen state
        if (sr != null)
            sr.color = Color.gray;
    }

    public void Unfreeze()
    {
        frozen = false;
        frozenTimer = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;

        // reset sprite color
        if (sr != null)
            sr.color = Color.white;

        ResumeAnimations();
    }

    private void PauseAnimations()
    {
        if (animator != null)
            animator.speed = 0f;
    }

    private void ResumeAnimations()
    {
        if (animator != null)
            animator.speed = 1f;
    }
    #endregion
}
