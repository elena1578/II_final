using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerOverworldController : GridMovementController
{
    // [Space] can use to break up sections  
    // private int facingDirection = 0;  // 0 = down, 1 = up, 2 = left, 3 = right
    
    // private refs
    private InputAction walkAction;
    // private InputAction interactAction, runAction;
    private bool canMove = true;

    private void Start()
    {      
        // ensure player starts aligned to the grid
        ForceSnapToGrid(transform.position);

        // no forces applied to player
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.None;
    }

    protected override void FixedUpdate()
    {
        ReadInput();
        UpdateWalkingAnimation();
        base.FixedUpdate();
    }

    private void ReadInput()
    {
        // ensure dynamic rb before trying to read
        rb.bodyType = RigidbodyType2D.Dynamic;
        
        if (isMoving || !canMove)
            return;

        var action = InputSystemManager.instance.actions["Walk"];
        Vector2 input = action.ReadValue<Vector2>();

        movement = input == Vector2.zero ? Vector2.zero : SnapToDirection(input);
    }

    private void UpdateWalkingAnimation()
    {
        Animator animator = GetComponentInChildren<Animator>();
        if (animator == null) return;

        bool isMoving = movement != Vector2.zero;
        animator.SetBool("isMoving", isMoving);

        if (isMoving)
        {
            animator.SetFloat("moveX", movement.x);
            animator.SetFloat("moveY", movement.y);
        }
        else
        {
            string idleState = "Down Idle";  // default

            if (lastDirection == Vector2.up) idleState = "Up Idle";
            else if (lastDirection == Vector2.down) idleState = "Down Idle";
            else if (lastDirection == Vector2.left) idleState = "Left Idle";
            else if (lastDirection == Vector2.right) idleState = "Right Idle";

            animator.Play(idleState, 0, 0f);
        }
    }

    public void DisablePlayerMovement()
    {
        canMove = false;
        movement = Vector2.zero;
        // isMoving = false;
        UpdateWalkingAnimation();
    }

    public void EnablePlayerMovement() => canMove = true;
}
