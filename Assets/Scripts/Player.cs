using UnityEngine;

public class Player : MonoBehaviour {

    private Rigidbody2D myRigidBody;
    private bool isFacingRight = true;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 10f;
    private float moveInput;

    [Header("Jump")]
    [SerializeField] private float jumpPower = 30f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;
    [SerializeField] private int maxJumpCount = 2;
    [SerializeField] private int jumpRemaining;
    private bool isJumping;

    [Header("Gravity")]
    [SerializeField] private float baseGravity = 3f;
    [SerializeField] private float maxFallSpeed = 30f;
    [SerializeField] private float fallMultiplier = 3f;

    [Header("GroundCheck")]
    [SerializeField] private Transform groundCheckPosition;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.7f, 0.05f);
    [SerializeField] private LayerMask groundLayer;
    private bool wasGrounded;

    [Header("WallCheck")]
    [SerializeField] private Transform wallCheckPosition;
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.05f, 1.5f);
    [SerializeField] private LayerMask wallLayer;

    [Header("WallMovement")]
    [SerializeField] private float wallSlideSpeed = 3f;
    private bool isWallSliding;

    // Wall jumping
    private bool isWallJumping;
    private float wallJumpDirection;
    private float wallJumpTime = 0.5f;
    private float wallJumpTimer;
    [SerializeField] private Vector2 wallJumpPower = new Vector2(10f, 30f);
    private float wallJumpLockTime = 0.2f;
    private float wallJumpLockCounter = 0f;


    private void Awake() {
        myRigidBody = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        // Jump pressed (start jump)
        if (GameInput.Instance.WasJumpActionPerformed() && jumpRemaining > 0) {
            isJumping = true;
            jumpRemaining--;
        }

        // Jump released early → Light jump
        // Reduce the jump while the player is still going upward
        if (GameInput.Instance.IsJumpActionReleased() && myRigidBody.linearVelocityY > 0) {
            myRigidBody.linearVelocityY *= jumpCutMultiplier;
        }

        // Check for jump input while eligible for wall jump
        if (GameInput.Instance.WasJumpActionPerformed() && wallJumpTimer > 0f) {
            isWallJumping = true;
            wallJumpLockCounter = wallJumpLockTime;
            wallJumpTimer = 0f;
            Invoke(nameof(CancelWallJump), wallJumpTime);
        }

        GroundCheck();
    }


    private void FixedUpdate() {
        // Jump
        if (isJumping) {
            myRigidBody.linearVelocityY = jumpPower;
            isJumping = false;
        }

        // Wall jump
        if (isWallJumping) {
            myRigidBody.linearVelocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y);
            isWallJumping = false; // apply only once
        }

        ProcessFallGravity(); // Apply more gravity when falling
        ProcessWallSlide(); // Apply wall slide when on a wall
        ProcessWallJump(); // Apply wall jump

        if (!isWallJumping && wallJumpLockCounter <= 0f) {
            MovePlayer();
            Flip();
        }
    }

    private void MovePlayer() {
        // Horizontal movement
        if (GameInput.Instance.IsRightActionPressed()) {
            myRigidBody.linearVelocityX = walkSpeed;
            moveInput = 1f;
        }

        if (GameInput.Instance.IsLeftActionPressed()) {
            myRigidBody.linearVelocityX = -walkSpeed;
            moveInput = -1f;
        }
    }

    private void GroundCheck() {
        bool isGrounded = Physics2D.OverlapBox(groundCheckPosition.position, groundCheckSize, 0, groundLayer);

        if (isGrounded && !wasGrounded) {
            jumpRemaining = maxJumpCount;
        }

        wasGrounded = isGrounded;
    }

    private bool WallCheck() {
        return Physics2D.OverlapBox(wallCheckPosition.position, wallCheckSize, 0, wallLayer);
    }

    private void ProcessFallGravity() {
        // Apply stronger gravity when falling
        if (myRigidBody.linearVelocityY < 0 && !wasGrounded) {
            myRigidBody.gravityScale = baseGravity * fallMultiplier;
        }
        // Apply normal gravity when rising or grounded
        else {
            myRigidBody.gravityScale = baseGravity;
        }

        // Limit fall speed
        myRigidBody.linearVelocityY = Mathf.Max(myRigidBody.linearVelocityY, -maxFallSpeed);
    }

    private void ProcessWallSlide() {
        // Only slide when not grounded, touching wall, not wall jumping, and holding toward wall
        if (!wasGrounded && WallCheck() && !isWallJumping &&
            (GameInput.Instance.IsLeftActionPressed() || GameInput.Instance.IsRightActionPressed())) {

            isWallSliding = true;
            myRigidBody.linearVelocityY = Mathf.Max(myRigidBody.linearVelocityY, -wallSlideSpeed);
        }
        else {
            isWallSliding = false;
        }
    }

    private void ProcessWallJump() {
        if (isWallSliding) {
            wallJumpDirection = -transform.localScale.x;
            wallJumpTimer = wallJumpTime;
            CancelInvoke(nameof(CancelWallJump));
        }
        else if (wallJumpTimer > 0f) {
            wallJumpTimer -= Time.deltaTime;
        }

        // Countdown for movement lock
        if (wallJumpLockCounter > 0f)
            wallJumpLockCounter -= Time.deltaTime;
    }

    private void CancelWallJump() {
        isWallJumping = false;
    }

    private void Flip() {
        if (moveInput > 0 && !isFacingRight)
            FlipCharacter(true);
        else if (moveInput < 0 && isFacingRight)
            FlipCharacter(false);
    }

    // Flip character right if faceRight is true
    private void FlipCharacter(bool faceRight) {
        isFacingRight = faceRight;
        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * (faceRight ? 1 : -1);
        transform.localScale = localScale;
    }

    // Show gizmos in editor
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheckPosition.position, groundCheckSize);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(wallCheckPosition.position, wallCheckSize);
    }
}
