using UnityEngine;

public class Player : MonoBehaviour {
    private Rigidbody2D myRigidBody;
    private bool isFacingRight = true;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 10f;
    private bool isMovingRight = true; // input direction for Flip()

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
    [SerializeField] private float wallJumpLockTime = 0.2f;
    [SerializeField] private Vector2 wallJumpPower = new Vector2(10f, 30f);

    // Add wallJumpTime
    // Add wallJumpTimer

    private void Awake() {
        myRigidBody = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        HandleJumpInput();
        GroundCheck();
    }

    private void FixedUpdate() {
        Debug.Log($"wallJumpLock: {wallJumpLock}");
        ApplyJump();
        ApplyWallJump();
        ApplyFallGravity();
        ProcessWallSlide();

        // normal movement only if not wall-jump locked
        if (!wallJumpLock) {
            HandleMovement();
            Flip();
        }
    }

    private void HandleJumpInput() {
        // Normal jump
        if (GameInput.Instance.WasJumpActionPerformed() && jumpRemaining > 0) {
            isJumping = true;
            jumpRemaining--;
        }

        // Short-tap jump cut (light jump)
        if (GameInput.Instance.IsJumpActionReleased() && myRigidBody.linearVelocityY > 0f) {
            myRigidBody.linearVelocityY *= jumpCutMultiplier;
        }

        // Wall jump (only while sliding)
        if (GameInput.Instance.WasJumpActionPerformed() && isWallSliding) {
            isWallJumping = true;
            wallJumpLock = true;
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

    private void HandleMovement() {
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

    private void ApplyFallGravity() {
        // stronger gravity while falling
        if (myRigidBody.linearVelocityY < 0f && !wasGrounded) {
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
        bool isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

        if (isGrounded && !wasGrounded) jumpRemaining = maxJumpCount;

        wasGrounded = isGrounded;
    }

    private void CancelWallJumpLock() {
        wallJumpLock = false;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
    }
}
