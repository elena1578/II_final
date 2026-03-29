using UnityEngine;
using System.Collections.Generic;


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

    // A* pathfinding
    private List<Vector3> currentPath;
    private int pathIndex;
    private float pathTimer;
    private float pathUpdateInterval = 0.5f;
    private Vector3 spawnedPos;
    private bool returningToSpawn = false;

    // internal refs
    private Animator animator;
    private SpriteRenderer sr;
    private EnemyOverworldSpawnArea spawnArea;
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
            walkSpeed = data.moveSpeed;
            alertRadius = data.alertRadius;
            chanceToMove = data.chanceToMove;
        }

        SnapToGrid(transform.position);  // once spawned + initialized, ensure grid alignment
        spawnedPos = transform.position;  // store spawn position for pathfinding to return to if player leaves spawn area
    }
    public void SetSpawnArea(EnemyOverworldSpawnArea area) => spawnArea = area;

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
        else if (returningToSpawn)
            ReturnToSpawn();
        else
            IdleAndMove();

        UpdateWalkingAnimation();
        base.FixedUpdate();
    }


    #region Movement
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

    private void ChasePlayer(Transform player)
    {
        // if player leaves spawn area, stop chasing and return to initial spawn position
        if (!IsInsideSpawnArea(player.position))
        {
            alerted = false;
            returningToSpawn = true;
            currentPath = null;
            return;
        }
        
        // update path periodically vs. every frame for performance
        pathTimer -= Time.deltaTime;

        if (pathTimer <= 0)
        {
            currentPath = OverworldPathfinder.instance.FindPath(transform.position, player.position);
            pathIndex = 0;
            pathTimer = pathUpdateInterval;
        }

        FollowPath();

        // override timers
        idleTimer = 0;
        moveTimer = 0;
    }

    private void FollowPath()
    {
        if (currentPath == null || pathIndex >= currentPath.Count)
            return;

        Vector3 target = currentPath[pathIndex];
        Vector2 dir = target - transform.position;

        // next tile enemy wants to move to
        Vector3 nextTile = currentPath[pathIndex];

        // prevent leaving spawn area
        if (!IsInsideSpawnArea(nextTile))
        {
            currentPath = null;   // cancel path so enemy recalculates
            movement = Vector2.zero;
            return;
        }

        // safety check if tile becomes blocked
        if (!OverworldPathfinder.instance.IsWalkable(nextTile))
        {
            currentPath = null;
            return;
        }

        movement = SnapToDirection(dir.normalized) * followMoveSpeed;  // slow down when chasing player

        if (Vector2.Distance(transform.position, target) < 0.1f)
            pathIndex++;
    }

    private void ReturnToSpawn()
    {
        pathTimer -= Time.deltaTime;

        if (pathTimer <= 0)
        {
            currentPath = OverworldPathfinder.instance.FindPath(transform.position, spawnedPos);
            pathIndex = 0;
            pathTimer = pathUpdateInterval;
        }

        FollowPath();

        // if close to spawn point, stop moving & reset state
        if (Vector2.Distance(transform.position, spawnedPos) < 0.1f)
        {
            returningToSpawn = false;
            alerted = false;  // reset alerted state so enemy can be alerted again if player re-enters spawn area
            currentPath = null;
            movement = Vector2.zero;
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
    #endregion


    #region Alert & Battle
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !alerted && !enteringBattle && !frozen)
        {     
            TriggerAlertBalloon();
            AudioManager.instance.PlaySFX(AudioManager.instance.alert, 0.7f);
            playerTransform = other.transform;
            alerted = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            alerted = true;  // once alerted, stays alerted
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
            FreezeForBattle(); 
            PlayerOverworldController player = other.collider.GetComponent<PlayerOverworldController>();
            if (player != null)
                player.FreezeForBattle();

            Debug.Log("[EnemyOverworldActor] Beginning battle with " + data.name);
            BattleTransitionManager.instance.StartBattle(data);
        }
    }
    #endregion


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
    
    public void FreezeForBattle()
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        movement = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        frozen = true;
        isMoving = false;
        gameObject.layer = LayerMask.NameToLayer("IgnorePhysics"); 

        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
            animator.speed = 0f;
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

    private bool IsInsideSpawnArea(Vector3 pos)
    {
        if (spawnArea == null)
            return true;

        Vector3 center = spawnArea.transform.position + (Vector3)spawnArea.offset;

        return
            pos.x >= center.x - spawnArea.size.x / 2f &&
            pos.x <= center.x + spawnArea.size.x / 2f &&
            pos.y >= center.y - spawnArea.size.y / 2f &&
            pos.y <= center.y + spawnArea.size.y / 2f;
    }
    #endregion
}
