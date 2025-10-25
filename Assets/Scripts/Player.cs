using UnityEngine;
using System;

public class Player : MonoBehaviour {
    public static Player Instance { get; private set; }

    public event EventHandler OnJump;
    public event EventHandler OnAirJump;
    public event EventHandler OnWallJump;
    public event EventHandler OnLanded;

    private Rigidbody2D myRigidBody;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 10f;
    private bool isMovingRight = true; // input direction for Flip()
    private bool isFacingRight = true;

    [Header("Jump")]
    [SerializeField] private float jumpPower = 30f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;
    [SerializeField] private int maxJumpCount = 2;
    [SerializeField] private int jumpRemaining;
    private bool isJumping; // request flag, applied in FixedUpdate

    [Header("Gravity")]
    [SerializeField] private float baseGravity = 3f;
    [SerializeField] private float maxFallSpeed = 30f;
    [SerializeField] private float fallMultiplier = 3f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.7f, 0.05f);
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;
    private bool wasGrounded;

    [Header("Wall Check")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.05f, 1.5f);
    [SerializeField] private LayerMask wallLayer;

    [Header("Wall Movement")]
    [SerializeField] private float wallSlideSpeed = 3f;
    private bool isWallSliding;

    [Header("Wall Jump")]
    private bool isWallJumping;       // request flag
    private bool wallJumpLock;        // persistent movement lock
    private float wallJumpDirection;  // +1 right, -1 left
    [SerializeField] private float wallJumpCoyoteCounter;
    [SerializeField] private float wallJumpCoyoteTime = 0.1f; // duration of be able to wall jump after leaving wall
    [SerializeField] private float wallJumpLockTime = 0.2f; // movement lock duration after wall jump
    [SerializeField] private Vector2 wallJumpPower = new Vector2(10f, 30f);

    private void Awake() {
        Instance = this;
        myRigidBody = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        HandleJumpInput();
        GroundCheck();
    }

    private void FixedUpdate() {
        ApplyJump();
        ApplyWallJump();
        ApplyFallGravity();
        ProcessWallSlide();
        ProcessWallJumpCoyote();

        // normal movement only if not wall-jump locked
        if (!wallJumpLock) {
            HandleMovementInput();
            Flip();
        }
    }

    private void HandleJumpInput() {
        // Normal jump (ground or mid-air double jump)
        if (GameInput.Instance.WasJumpActionPerformed() && jumpRemaining > 0) {
            isJumping = true;
            jumpRemaining--;

            // If not grounded then this is a air-jump
            if (!isGrounded && !isWallSliding) {
                OnAirJump?.Invoke(this, EventArgs.Empty);
            }
            else if (!isWallSliding) {
                OnJump?.Invoke(this, EventArgs.Empty);
            }
        }

        // Short-tap jump cut (light jump)
        if (GameInput.Instance.IsJumpActionReleased() && myRigidBody.linearVelocityY > 0f) {
            myRigidBody.linearVelocityY *= jumpCutMultiplier;
        }

        // Wall jump (only while sliding or within coyote window)
        if (GameInput.Instance.WasJumpActionPerformed() && wallJumpCoyoteCounter > 0f) {
            isWallJumping = true;
            wallJumpLock = true;
            wallJumpCoyoteCounter = 0f;

            // stop wall-slide state immediately and trigger jump animation
            isWallSliding = false;

            OnWallJump?.Invoke(this, EventArgs.Empty);

            Invoke(nameof(CancelWallJumpLock), wallJumpLockTime);
        }
    }

    private void ApplyJump() {
        if (!isJumping) return;

        myRigidBody.linearVelocityY = jumpPower;
        isJumping = false;
    }

    private void ApplyWallJump() {
        if (!isWallJumping) return;

        // Flip toward jump direction
        FlipCharacter(wallJumpDirection > 0f);
        isMovingRight = wallJumpDirection > 0f;

        // Apply jump velocity
        myRigidBody.linearVelocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y);
        jumpRemaining = 1;

        isWallJumping = false;
    }

    private void HandleMovementInput() {
        if (GameInput.Instance.IsRightActionPressed()) {
            myRigidBody.linearVelocityX = walkSpeed;
            isMovingRight = true;
        }
        else if (GameInput.Instance.IsLeftActionPressed()) {
            myRigidBody.linearVelocityX = -walkSpeed;
            isMovingRight = false;
        }
        else {
            myRigidBody.linearVelocityX = 0f;
        }
    }

    private void Flip() {
        if (isMovingRight && !isFacingRight) FlipCharacter(true);
        else if (!isMovingRight && isFacingRight) FlipCharacter(false);
    }

    private void FlipCharacter(bool faceRight) {
        isFacingRight = faceRight;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (faceRight ? 1f : -1f);
        transform.localScale = scale;
    }

    private void ProcessWallSlide() {
        // Slide only when airborne and touching a wall
        if (!wasGrounded && WallCheck() &&
            (GameInput.Instance.IsLeftActionPressed() || GameInput.Instance.IsRightActionPressed())) {
            isWallSliding = true;

            // Cap fall speed
            myRigidBody.linearVelocityY = Mathf.Max(myRigidBody.linearVelocityY, -wallSlideSpeed);

            // Set direction for next wall jump
            wallJumpDirection = -transform.localScale.x;
        }
        else {
            isWallSliding = false;
        }
    }

    private void ProcessWallJumpCoyote() {
        if (isWallSliding) {
            wallJumpCoyoteCounter = wallJumpCoyoteTime;
        }
        else if (wallJumpCoyoteCounter > 0f) {
            wallJumpCoyoteCounter -= Time.deltaTime; // decrease timer over time
        }
    }

    private void CancelWallJumpLock() {
        wallJumpLock = false;
    }

    private void ApplyFallGravity() {
        // stronger gravity while falling
        if (myRigidBody.linearVelocityY < 0f && !isGrounded) {
            myRigidBody.gravityScale = baseGravity * fallMultiplier;
        }
        else {
            myRigidBody.gravityScale = baseGravity;
        }
        // limit max fall speed
        myRigidBody.linearVelocityY = Mathf.Max(myRigidBody.linearVelocityY, -maxFallSpeed);
    }

    private bool WallCheck() {
        return Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, wallLayer);
    }

    private void GroundCheck() {
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

        if (isGrounded && !wasGrounded) {
            jumpRemaining = maxJumpCount;
            OnLanded?.Invoke(this, EventArgs.Empty);
        }
        wasGrounded = isGrounded;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
    }

    public float GetHorizontalSpeed() {
        return Mathf.Abs(myRigidBody.linearVelocityX);
    }

    public bool GetIsGrounded() {
        return isGrounded;
    }

    public bool GetIsFalling() {
        // Falling is true when going down and not grounded (exclude wall sliding if you prefer)
        return myRigidBody.linearVelocityY < -0.1f && !isGrounded && !isWallSliding;
    }

    public bool GetIsWallSliding() {
        return isWallSliding;
    }
}
