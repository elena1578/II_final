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

    void Start()
    {
        walkAction = InputSystemManager.instance.actions["Walk"];
    }

    protected override void FixedUpdate()
    {
        ReadInput();
        UpdateWalkingAnimation();
        base.FixedUpdate();
    }

    private void ReadInput()
    {
        if (isMoving)
            return;

        if (!canMove)
            return;
        
        Vector2 input = walkAction.ReadValue<Vector2>();

        movement = input == Vector2.zero
            ? Vector2.zero
            : SnapToDirection(input);
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
        isMoving = false;
        UpdateWalkingAnimation();
    }

    public void EnablePlayerMovement() => canMove = true;
}
