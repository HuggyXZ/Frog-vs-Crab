using UnityEngine;
using System;
using System.Collections;

public class PlayerMovement : MonoBehaviour {
    public static PlayerMovement Instance { get; private set; }

    public event EventHandler OnJump;
    public event EventHandler OnAirJump;
    public event EventHandler OnWallJump;
    public event EventHandler OnLanded;
    public event EventHandler OnFlip;
    public event EventHandler OnStartMovingSameDirection;
    public event EventHandler OnPowerUp;
    public event EventHandler OnPowerUpCounterUpdate;

    private Rigidbody2D myRigidBody;
    private BoxCollider2D myBoxCollider;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;
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

    [Header("Land Check")]
    [SerializeField] private Transform landCheck;
    [SerializeField] private Vector2 landCheckSize = new Vector2(1f, 0.025f);
    [SerializeField] private LayerMask landLayer;
    private bool isOnLand;
    private bool wasOnLand;

    // Platform
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private CompositeCollider2D platformCollider;
    private bool isOnPlatform;


    [Header("Wall Check")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.025f, 1.55f);
    [SerializeField] private LayerMask wallLayer;
    private bool isOnWall;

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
    [SerializeField] private Vector2 wallJumpPower;

    [Header("PowerUp")] // Add timer
    [SerializeField] private float powerUpMoveSpeedIncrease = 5f;
    [SerializeField] private float powerUpJumpIncrease = 10f;
    [SerializeField] private int powerUpMaxJumpIncrease = 1;
    [SerializeField] private float powerUpTime = 15f;
    private float powerUpCounter;
    private bool isPoweredUp;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 15f;
    [SerializeField] private float knockbackUpward = 10f;
    [SerializeField] private float knockbackDuration = 0.5f;
    private bool canMove = true;

    private void Awake() {
        Instance = this;
        myRigidBody = GetComponent<Rigidbody2D>();
        myBoxCollider = GetComponent<BoxCollider2D>();
    }

    private void Start() {
        jumpRemaining = maxJumpCount;
    }

    private void Update() {
        if (!canMove) return; // movement locked during knockback

        HandleJumpInput();
    }

    private void FixedUpdate() {
        // movement locked during knockback
        if (canMove) {
            ApplyJump();
            ApplyWallJump();
            ApplyFallGravity();
            ProcessWallSlide();
            ProcessWallJumpCoyote();
            HandleDownInput();

            // normal movement only if not wall-jump locked
            if (!wallJumpLock) {
                HandleMovementInput();
                Flip();
            }
        }

        // Check physics-based states *after* movement
        OnLandCheck();
        WallCheck();
        PlatformCheck();
    }

    private void HandleJumpInput() {
        // Normal jump (ground or mid-air double jump)
        if (GameInput.Instance.WasJumpActionPerformed() && jumpRemaining > 0 && wallJumpCoyoteCounter <= 0f) {
            isJumping = true;
            jumpRemaining--;

            // If not grounded then this is a air-jump
            if (!isOnLand) {
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
        myRigidBody.linearVelocity = new Vector2(wallJumpDirection * moveSpeed, jumpPower);

        isWallJumping = false;
    }

    private void HandleMovementInput() {
        // Moving right
        if (GameInput.Instance.IsRightActionPressed()) {
            myRigidBody.linearVelocityX = moveSpeed;
            isMovingRight = true;

            if (stopMovingTimer >= stopMovingThreshold) {
                OnStartMovingSameDirection?.Invoke(this, EventArgs.Empty);
            }
            stopMovingTimer = 0f; // reset timer whenever moving
        }
        // Moving left
        else if (GameInput.Instance.IsLeftActionPressed()) {
            myRigidBody.linearVelocityX = -moveSpeed;
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
        if (!wasOnLand && isOnWall &&
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

    private void HandleDownInput() {
        if (GameInput.Instance.IsDownActionPressed() && isOnPlatform) {
            StartCoroutine(DisablePlatformCollision());
        }
    }

    private IEnumerator DisablePlatformCollision() {
        float dropDuration = 0.4f;
        Physics2D.IgnoreCollision(myBoxCollider, platformCollider, true);
        yield return new WaitForSeconds(dropDuration);
        Physics2D.IgnoreCollision(myBoxCollider, platformCollider, false);
    }

    private void OnLandCheck() {
        isOnLand = Physics2D.OverlapBox(landCheck.position, landCheckSize, 0f, landLayer);

        if (isOnLand && !wasOnLand) {
            jumpRemaining = maxJumpCount;
            OnLanded?.Invoke(this, EventArgs.Empty);
        }
        wasOnLand = isOnLand;
    }
    private void WallCheck() {
        isOnWall = Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, wallLayer);
    }

    public void PlatformCheck() {
        isOnPlatform = Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, platformLayer);
    }

    private void ApplyFallGravity() {
        // stronger gravity while falling
        if (myRigidBody.linearVelocityY < 0f && !isOnLand && !isWallSliding) {
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

    public void ActivatePowerUp() {
        if (isPoweredUp) return; // prevent overlap

        isPoweredUp = true;
        moveSpeed += powerUpMoveSpeedIncrease;
        jumpPower += powerUpJumpIncrease;
        maxJumpCount += powerUpMaxJumpIncrease;

        OnPowerUp?.Invoke(this, EventArgs.Empty);

        StartCoroutine(PowerUpDuration());
    }

    private IEnumerator PowerUpDuration() {
        powerUpCounter = powerUpTime;
        while (powerUpCounter > 0) {
            powerUpCounter -= Time.deltaTime;
            OnPowerUpCounterUpdate?.Invoke(this, EventArgs.Empty);
            yield return null;
        }

        // Revert stats
        powerUpCounter = 0;
        moveSpeed -= powerUpMoveSpeedIncrease;
        jumpPower -= powerUpJumpIncrease;
        maxJumpCount -= powerUpMaxJumpIncrease;
        isPoweredUp = false;
    }

    public void OnHitByEnemy(Vector3 enemyPosition) {
        // Calculate direction away from enemy
        Vector2 knockDirection = (transform.position - enemyPosition).normalized; // return -1 if left, 1 if right

        // Apply instant knockback
        myRigidBody.linearVelocity = new Vector2(knockDirection.x * knockbackForce, knockbackUpward);

        // Disable movement temporarily
        StartCoroutine(HandleKnockback());
    }

    private IEnumerator HandleKnockback() {
        canMove = false;
        yield return new WaitForSeconds(knockbackDuration);
        canMove = true;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(landCheck.position, landCheckSize);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
    }

    public float GetPowerUpTime() { return powerUpTime; }
    public float GetPowerUpCounter() { return powerUpCounter; }

    public float GetHorizontalSpeed() {
        return Mathf.Abs(myRigidBody.linearVelocityX);
    }

    public bool GetIsOnLand() { return isOnLand; }
    public bool GetIsOnPlatform() { return isOnPlatform; }
    public bool GetIsOnWall() { return isOnWall; }
    public bool GetIsFalling() { return isFalling; }
    public bool GetIsWallSliding() { return isWallSliding; }
}
