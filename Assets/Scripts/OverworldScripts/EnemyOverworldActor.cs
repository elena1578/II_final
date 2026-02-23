using UnityEngine;

public class EnemyOverworldActor : GridMovementController
{
    public EnemyOverworldData data;
    public GameObject alertBalloonPrefab;

    // values set from data
    private float alertRadius = 5.0f;
    private float chanceToMove = 1f;

    // walking behavior
    private float moveTimer;
    private float idleTimer;
    [SerializeField] private float minMoveTime = 1.0f;
    [SerializeField] private float maxMoveTime = 3.0f;
    [SerializeField] private float minIdleTime = 0.5f;
    [SerializeField] private float maxIdleTime = 1.5f;

    // internal refs
    private Animator animator;
    private bool alerted = false;
    private Transform playerTransform;


    protected override void Awake()
    {
        base.Awake();
        animator = GetComponentInChildren<Animator>();
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
    /// Randomly chooses the next movement state: moving in a random direction
    /// for a random duration, or idling for a random duration
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
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
        
        movement = SnapToDirection((player.position - transform.position).normalized);

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
        if (other.collider.CompareTag("Player"))
        {
            Debug.Log("Beginning battle with " + data.name);
            BattleTransitionManager.instance.StartBattle(data);
        }
    }
}
