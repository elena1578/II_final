using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[RequireComponent(typeof(Rigidbody2D))]
public abstract class GridMovementController : MonoBehaviour
{
    [Header("Settings via Inheritance")]
    [SerializeField] protected float walkSpeed = 1f;
    [SerializeField] protected float sprintSpeed = 2f;
    protected float moveSpeed = 1f;
    protected float gridSize = 0.32f;
    protected Vector3 gridOffset = new Vector3(0.23f, -2.75f, 0f); 
    protected float collisionRadius = 0.12f;

    protected Vector2 movement;
    protected Vector3 targetPosition;
    protected Vector2 lastDirection = Vector2.down;
    protected bool isMoving = false;
    protected Rigidbody2D rb;
    protected LayerMask collisionLayer;


    /// <summary>
    /// ensures object starts aligned to the grid
    /// </summary>
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        collisionLayer = LayerMask.GetMask("Collision");

        targetPosition = SnapToGrid(transform.position);
        transform.position = targetPosition;
    }

    protected virtual void FixedUpdate()
    {
        if (isMoving)
            MoveToTile();
        else if (movement != Vector2.zero)
            TryMove();
    }

    /// <summary>
    /// try to move in direction of input. if collision, stop movement 
    /// </summary>
    private void TryMove()
    {
        Vector2 dir = SnapToDirection(movement);
        Vector3 nextTile = targetPosition + (Vector3)(dir * gridSize);

        if (!CanMoveTo(nextTile))
        {
            movement = Vector2.zero;
            return;
        }

        targetPosition = nextTile;
        isMoving = true;
        lastDirection = dir;
    }

    /// <summary>
    /// checks if the target position is free of collision before moving
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    private bool CanMoveTo(Vector3 target)
    {
        Vector2 origin = rb.position;
        Vector2 direction = (target - (Vector3)origin).normalized;
        float distance = Vector2.Distance(origin, target);

        RaycastHit2D hit = Physics2D.CircleCast(
            origin,
            collisionRadius,
            direction,
            distance,
            collisionLayer
        );

        return hit.collider == null;
    }

    /// <summary>
    /// moves the object towards the target tile
    /// </summary>
    private void MoveToTile()
    {
        Vector2 newPos = Vector2.MoveTowards(
            rb.position,
            targetPosition,
            moveSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(newPos);

        // snap to target position if close enough
        if (Vector2.Distance(rb.position, targetPosition) <= moveSpeed * Time.fixedDeltaTime)
        {
            rb.MovePosition(targetPosition);
            isMoving = false;
        }
    }

    protected Vector2 SnapToDirection(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            return new Vector2(Mathf.Sign(dir.x), 0);
        else
            return new Vector2(0, Mathf.Sign(dir.y));
    }

    /// <summary>
    /// returns nearest grid-aligned position based on current position and grid 
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    protected Vector3 SnapToGrid(Vector3 pos)
    {
        float x = Mathf.Round((pos.x - gridOffset.x) / gridSize) * gridSize + gridOffset.x;
        float y = Mathf.Round((pos.y - gridOffset.y) / gridSize) * gridSize + gridOffset.y;
        return new Vector3(x, y, pos.z);
    }

    public void ForceSnapToGrid(Vector3 worldPosition)
    {
        isMoving = false;
        movement = Vector2.zero;

        targetPosition = SnapToGrid(worldPosition);
        rb.position = targetPosition;
        rb.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// used in RoomChangeManager to give extra time for player to spawn in &
    /// have physics etc. initialize prior to snapping
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    public IEnumerator ForceSnapToGridNextFrame(PlayerOverworldController controller, Vector3 pos)
    {
        yield return null;
        controller.ForceSnapToGrid(pos);
    }

    /// <summary>
    /// helper method to get random direction Vector2.
    /// mainly for enemy movement
    /// </summary>
    /// <returns></returns>
    protected Vector2 GetRandomDirection()
    {
        int choice = Random.Range(0, 4);
        switch (choice)
        {
            case 0: return Vector2.up;
            case 1: return Vector2.down;
            case 2: return Vector2.left;
            case 3: return Vector2.right;
            default: return Vector2.zero;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        float size = gridSize;
        const int radius = 40;  // how many cells to draw in each direction from current position

        // snap origin to nearest grid point 
        Vector3 origin = transform.position;
        origin.x = Mathf.Round((origin.x - gridOffset.x) / size) * size + gridOffset.x;
        origin.y = Mathf.Round((origin.y - gridOffset.y) / size) * size + gridOffset.y;

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                // cal center of each cell based on grid size & offset
                Vector3 cellCenter = new Vector3(
                    origin.x + x * size,
                    origin.y + y * size,
                    origin.z
                );

                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(cellCenter, new Vector3(size, size, 0.01f));
            }
        }
    }
#endif
}
