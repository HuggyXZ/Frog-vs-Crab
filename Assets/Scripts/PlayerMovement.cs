using UnityEngine;
using System;
using System.Collections;
using UnityEngine.XR;

public class PlayerMovement : MonoBehaviour {
    public static PlayerMovement Instance { get; private set; }

    public event EventHandler OnJump;
    public event EventHandler OnAirJump;
    public event EventHandler OnWallJump;
    public event EventHandler OnDash;
    public event EventHandler OnDashEnd;
    public event EventHandler OnLanded;
    public event EventHandler OnFlip;
    public event EventHandler OnStartMovingSameDirection;

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
    private bool jumpFlag; // request flag, applied in FixedUpdate

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 50f;
    [SerializeField] private float dashDuration = 0.25f;
    [SerializeField] private float dashCooldown = 3f;
    private bool dashFlag; // request flag, applied in FixedUpdate
    private bool canDash = true;
    private bool isDasing;

    [Header("Land Check")]
    // Land
    [SerializeField] private Transform landCheck;
    [SerializeField] private Vector2 landCheckSize = new Vector2(1f, 0.025f);
    [SerializeField] private LayerMask landLayer;
    private bool isOnLand;
    private bool wasOnLand;
    // Platform
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private CompositeCollider2D platformCollider;
    [SerializeField] private BoxCollider2D movingPlatformCollider;
    private bool isOnPlatform;
    private Coroutine disableCollisionCoroutine;


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
    private bool wallJumpFlag;       // request flag, applied in FixedUpdate
    private bool isWallJumping;        // persistent movement lock
    private float wallJumpDirection;  // +1 right, -1 left
    private float wallJumpCoyoteCounter; // counter count down
    [SerializeField] private float wallJumpCoyoteTime = 0.1f; // duration of be able to wall jump after leaving wall
    [SerializeField] private float wallJumpLockTime = 0.2f; // movement lock duration after wall jump
    [SerializeField] private Vector2 wallJumpPower;

    [Header("Gravity")]
    [SerializeField] private float baseGravity = 3f;
    [SerializeField] private float maxFallSpeed = 30f;
    [SerializeField] private float fallMultiplier = 3f;
    private bool isFalling;


    [Header("PowerUp")] // Add timer
    [SerializeField] private float powerUpMoveSpeedIncrease = 5f;
    [SerializeField] private float powerUpJumpIncrease = 10f;
    [SerializeField] private int powerUpMaxJumpIncrease = 1;
    [SerializeField] private float powerUpDashIncrease = 25f;

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
        HoldToPowerUp.Instance.OnPowerUp += HoldToPowerUp_OnPowerUp;
        HoldToPowerUp.Instance.OnPowerUpEnd += HoldToPowerUp_OnPowerUpEnd;
        PlayerHealth.Instance.OnPlayerDied += PlayerHealth_OnPlayerDied;
        jumpRemaining = maxJumpCount;
    }

    private void Update() {
        if (!canMove || isDasing) return; // movement locked during knockback

        HandleJumpInput();
        HandleDashInput();
    }

    private void FixedUpdate() {
        // movement locked during knockback
        if (canMove) {
            ApplyDash();
            if (!isDasing) {
                ApplyJump();
                ApplyWallJump();
                ProcessWallSlide();
                ProcessWallJumpCoyote();
                ApplyFallGravity();

                // normal movement only if not wall-jump locked
                if (!isWallJumping) {
                    HandleMovementInput();
                    Flip();
                }
            }
        }

        // Check physics-based states *after* movement
        OnLandCheck();
        WallCheck();
        PlatformCheck();
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

        if (GameInput.Instance.IsDownActionPressed() && isOnPlatform) {
            TriggerDisablePlatformCollision();
        }
    }

    private void HandleJumpInput() {
        // Normal jump (ground or mid-air double jump)
        if (GameInput.Instance.WasJumpActionPerformed() && jumpRemaining > 0 && wallJumpCoyoteCounter <= 0f) {
            jumpFlag = true;
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
            wallJumpFlag = true;
            isWallJumping = true;
            wallJumpCoyoteCounter = 0f;
            isWallSliding = false; // stop wall-slide state immediately and trigger jump animation

            OnWallJump?.Invoke(this, EventArgs.Empty);

            Invoke(nameof(CancelWallJumpLock), wallJumpLockTime);
        }
    }

    private void ApplyJump() {
        if (!jumpFlag) return;

        myRigidBody.linearVelocityY = jumpPower;
        jumpFlag = false;
    }

    private void ApplyWallJump() {
        if (!wallJumpFlag) return;

        // Flip toward jump direction
        FlipCharacter(wallJumpDirection > 0f);
        isMovingRight = wallJumpDirection > 0f;

        // Apply jump velocity
        myRigidBody.linearVelocity = new Vector2(wallJumpDirection * moveSpeed, jumpPower);

        wallJumpFlag = false;
    }

    private void HandleDashInput() {
        if (GameInput.Instance.WasDashActionPerformed() && canDash) {
            dashFlag = true;
        }
    }

    private void ApplyDash() {
        if (!dashFlag) return;
        StartCoroutine(DashCoroutine());
        dashFlag = false;
    }

    private IEnumerator DashCoroutine() {
        Physics2D.IgnoreLayerCollision(8, 9, true);
        canDash = false;
        isDasing = true;
        OnDash?.Invoke(this, EventArgs.Empty);

        myRigidBody.gravityScale = 0f;
        myRigidBody.linearVelocityY = 0f;
        float dashDirection = isMovingRight ? 1f : -1f;
        myRigidBody.linearVelocityX = dashDirection * dashSpeed; // Dash movement

        yield return new WaitForSeconds(dashDuration);

        myRigidBody.linearVelocityX = 0f; // Stop movement
        myRigidBody.gravityScale = baseGravity;
        isDasing = false;
        OnDashEnd?.Invoke(this, EventArgs.Empty);
        Physics2D.IgnoreLayerCollision(8, 9, false);

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
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
        isWallJumping = false;
    }

    private void TriggerDisablePlatformCollision() {
        if (disableCollisionCoroutine != null)
            StopCoroutine(disableCollisionCoroutine);

        disableCollisionCoroutine = StartCoroutine(DisablePlatformCollision());
    }

    private IEnumerator DisablePlatformCollision() {
        float dropDuration = 0.3f;
        Physics2D.IgnoreCollision(myBoxCollider, platformCollider, true);
        Physics2D.IgnoreCollision(myBoxCollider, movingPlatformCollider, true);
        yield return new WaitForSeconds(dropDuration);
        Physics2D.IgnoreCollision(myBoxCollider, platformCollider, false);
        Physics2D.IgnoreCollision(myBoxCollider, movingPlatformCollider, false);
        disableCollisionCoroutine = null;
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
        isOnPlatform = Physics2D.OverlapBox(landCheck.position, landCheckSize, 0f, platformLayer);
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

    private void HoldToPowerUp_OnPowerUp(object sender, EventArgs e) {
        moveSpeed += powerUpMoveSpeedIncrease;
        jumpPower += powerUpJumpIncrease;
        maxJumpCount += powerUpMaxJumpIncrease;
        dashSpeed += powerUpDashIncrease;
    }

    private void HoldToPowerUp_OnPowerUpEnd(object sender, EventArgs e) {
        moveSpeed -= powerUpMoveSpeedIncrease;
        jumpPower -= powerUpJumpIncrease;
        maxJumpCount -= powerUpMaxJumpIncrease;
        dashSpeed -= powerUpDashIncrease;
    }

    public void OnHitByEnemy(Vector3 enemyPosition) {
        // Calculate direction away from enemy
        Vector2 knockDirection = (transform.position - enemyPosition).normalized;

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

    private void PlayerHealth_OnPlayerDied(object sender, EventArgs e) {
        this.enabled = false;
    }

    public float GetHorizontalSpeed() {
        return Mathf.Abs(myRigidBody.linearVelocityX);
    }

    public bool GetIsOnLand() { return isOnLand; }
    public bool GetIsOnPlatform() { return isOnPlatform; }
    public bool GetIsOnWall() { return isOnWall; }
    public bool GetIsFalling() { return isFalling; }
    public bool GetIsWallSliding() { return isWallSliding; }

    private void OnDisable() {
        HoldToPowerUp.Instance.OnPowerUp -= HoldToPowerUp_OnPowerUp;
        HoldToPowerUp.Instance.OnPowerUpEnd -= HoldToPowerUp_OnPowerUpEnd;
        PlayerHealth.Instance.OnPlayerDied -= PlayerHealth_OnPlayerDied;
    }
}
