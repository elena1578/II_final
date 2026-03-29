using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class PlayerOverworldController : GridMovementController
{
    // [Space] can use to break up sections  
    
    // private refs
    private InputAction walkAction, sprintAction;
    public InputAction interactAction;
    private bool canMove = true;
    private bool sprinting = false; 

    protected override void Awake()
    {
        walkAction = InputSystemManager.instance.actions["Walk"];
        sprintAction = InputSystemManager.instance.actions["Sprint"];
        interactAction = InputSystemManager.instance.actions["Interact"];
        base.Awake();
    }

    private void Update()
    {
        ReadInput();
        UpdateWalkingAnimation();
    }

    private void ReadInput()
    {
        // always check for sprint toggle first
        if (sprintAction.WasPressedThisFrame())
            sprinting = !sprinting;

        if (!canMove)
            return;

        Vector2 input = walkAction.ReadValue<Vector2>();
        moveSpeed = sprinting ? sprintSpeed : walkSpeed;  // set move speed based on sprinting state

        // don't interrupt tile movement
        if (isMoving)
            return;

        // snap input to cardinal directions
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
        sprinting = false;
        movement = Vector2.zero;
        // isMoving = false;
        UpdateWalkingAnimation();
    }

    public void FreezeForBattle()
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        movement = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        isMoving = false;
        gameObject.layer = LayerMask.NameToLayer("IgnorePhysics"); 

        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
            animator.speed = 0f;
    }

    public void EnablePlayerMovement() => canMove = true;

    public bool InteractPressed()
    {
        return interactAction != null && interactAction.WasPressedThisFrame();
    }
}
