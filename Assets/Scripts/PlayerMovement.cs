using UnityEngine;
using System;

public class PlayerMovement : MonoBehaviour {
    public static PlayerMovement Instance { get; private set; }

    public event EventHandler OnJump;
    public event EventHandler OnAirJump;
    public event EventHandler OnWallJump;
    public event EventHandler OnLanded;
    public event EventHandler OnFlip;
    public event EventHandler OnStartMovingSameDirection;
    public event EventHandler OnPowerUp;

    private Rigidbody2D myRigidBody;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 10f;
    private bool isMovingRight = true; // input direction for Flip()
    private bool isFacingRight = true;
    private float stopMovingTimer = 0f; // timer count up
    [SerializeField] private float stopMovingThreshold = 1f; // seconds of standing still to consider "stopped moving"

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
    private bool isFalling;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.7f, 0.05f);
    [SerializeField] private LayerMask landLayer;
    private bool isGrounded;
    private bool wasGrounded;

    [Header("Wall Check")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.05f, 1.5f);
    [SerializeField] private LayerMask wallLayer;

    [Header("Wall Movement")]
    // Wall Slide
    [SerializeField] private float wallSlideSpeed = 3f;
    private bool isWallSliding;

    // Wall Jump
    private bool isWallJumping;       // request flag
    private bool wallJumpLock;        // persistent movement lock
    private float wallJumpDirection;  // +1 right, -1 left
    private float wallJumpCoyoteCounter; // counter count down
    [SerializeField] private float wallJumpCoyoteTime = 0.1f; // duration of be able to wall jump after leaving wall
    [SerializeField] private float wallJumpLockTime = 0.2f; // movement lock duration after wall jump
    [SerializeField] private Vector2 wallJumpPower = new Vector2(10f, 30f);

    [Header("PowerUp")] // Add timer
    [SerializeField] private float powerUpSpeedIncrease = 5f;
    [SerializeField] private float powerUpJumpIncrease = 10f;
    [SerializeField] private float powerUpTime = 15f;
    private float powerUpTimer = 0f;

    private void Awake() {
        Instance = this;
        myRigidBody = GetComponent<Rigidbody2D>();
    }

    private void Start() {
        HoldToPowerUp.OnHoldComplete += HoldToPowerUp_OnHoldComplete;
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
        if (GameInput.Instance.WasJumpActionPerformed() && jumpRemaining > 0 && wallJumpCoyoteCounter <= 0f) {
            isJumping = true;
            jumpRemaining--;

            // If not grounded then this is a air-jump
            if (!isGrounded) {
                OnAirJump?.Invoke(this, EventArgs.Empty);
            }
            else {
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
            isWallSliding = false; // stop wall-slide state immediately and trigger jump animation

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

        isWallJumping = false;
    }

    private void HandleMovementInput() {
        // Moving right
        if (GameInput.Instance.IsRightActionPressed()) {
            myRigidBody.linearVelocityX = walkSpeed;
            isMovingRight = true;

            if (stopMovingTimer >= stopMovingThreshold) {
                OnStartMovingSameDirection?.Invoke(this, EventArgs.Empty);
            }
            stopMovingTimer = 0f; // reset timer whenever moving
        }
        // Moving left
        else if (GameInput.Instance.IsLeftActionPressed()) {
            myRigidBody.linearVelocityX = -walkSpeed;
            isMovingRight = false;

            if (stopMovingTimer >= stopMovingThreshold) {
                OnStartMovingSameDirection?.Invoke(this, EventArgs.Empty);
            }
            stopMovingTimer = 0f; // reset timer whenever moving
        }
        // Not moving
        else {
            myRigidBody.linearVelocityX = 0f;
            stopMovingTimer += Time.deltaTime;
        }
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
        if (myRigidBody.linearVelocityY < 0f && !isGrounded && !isWallSliding) {
            isFalling = true;
            // apply fall multiplier
            myRigidBody.gravityScale = baseGravity * fallMultiplier;
        }
        else {
            isFalling = false;
            // reset gravity
            myRigidBody.gravityScale = baseGravity;
        }
        // limit max fall speed
        myRigidBody.linearVelocityY = Mathf.Max(myRigidBody.linearVelocityY, -maxFallSpeed);
    }

    private bool WallCheck() {
        return Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, wallLayer);
    }

    private void GroundCheck() {
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, landLayer);

        if (isGrounded && !wasGrounded) {
            jumpRemaining = maxJumpCount;
            OnLanded?.Invoke(this, EventArgs.Empty);
        }
        wasGrounded = isGrounded;
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

        OnFlip?.Invoke(this, EventArgs.Empty);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
    }

    private void HoldToPowerUp_OnHoldComplete() {
        OnPowerUp?.Invoke(this, EventArgs.Empty);
        // Add timer
        walkSpeed += powerUpSpeedIncrease;
        jumpPower += powerUpJumpIncrease;
    }

    public float GetHorizontalSpeed() {
        return Mathf.Abs(myRigidBody.linearVelocityX);
    }

    public bool GetIsGrounded() {
        return isGrounded;
    }

    public bool GetIsFalling() {
        return isFalling;
    }

    public bool GetIsWallSliding() {
        return isWallSliding;
    }
}
