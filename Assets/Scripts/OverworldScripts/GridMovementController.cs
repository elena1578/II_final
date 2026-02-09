using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public abstract class GridMovementController : MonoBehaviour
{
    [Header("Settings via Inheritance")]
    [SerializeField] protected float moveSpeed = 1f;
    protected float gridSize = 0.25f;
    protected float collisionRadius = 0.12f;

    protected Vector2 movement;
    protected Vector3 targetPosition;
    protected Vector2 lastDirection = Vector2.down;
    protected bool isMoving = false;
    protected Rigidbody2D rb;
    protected LayerMask collisionLayer;


    /// <summary>
    /// Ensures object starts aligned to the grid
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
    /// Checks if the target position is free of collision before moving
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    private bool CanMoveTo(Vector3 target)
    {
        return !Physics2D.OverlapCircle(
            target,
            collisionRadius,
            collisionLayer
        );
    }

    /// <summary>
    /// Moves the object towards the target tile
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
        if (Vector2.Distance(rb.position, targetPosition) < 0.001f)
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

    protected Vector3 SnapToGrid(Vector3 pos)
    {
        float x = Mathf.Round(pos.x / gridSize) * gridSize;
        float y = Mathf.Round(pos.y / gridSize) * gridSize;
        return new Vector3(x, y, pos.z);
    }

    /// <summary>
    /// Helper method to get random direction Vector2
    /// Mainly for enemy movement
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
}
