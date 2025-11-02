using System.Collections;
using UnityEngine;

public class EnemyMovement : MonoBehaviour {
    private Rigidbody2D myRigidBody;
    private BoxCollider2D myBoxCollider;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpPower = 30f;
    [SerializeField] private float chaseRange = 15f; // how far the enemy can detect/chase the player
    private Coroutine stopMovingCoroutine;

    float distanceToPlayer;
    private float direction;
    private bool isFacingRight = true;

    [Header("Checks")]
    [SerializeField] private float playerAboveCheckDistance = 15f;
    [SerializeField] private float playerBelowCheckDistance = 15f;
    [SerializeField] private float landAboveCheckDistance = 7f;
    [SerializeField] private float frontCheckDistance = 2f;
    [SerializeField] private float gapCheckDistance = 2f;

    [Header("References & Layers")]
    private Transform playerTransform;
    [SerializeField] private LayerMask playerLayer;
    // Land
    [SerializeField] private LayerMask landLayer;
    [SerializeField] private Transform landCheck;
    [SerializeField] private Vector2 landCheckSize = new Vector2(2f, 0.025f);
    // Platform
    [SerializeField] private LayerMask platformLayer;
    private CompositeCollider2D platformCollider;
    private Coroutine disableCollisionCoroutine;

    private bool shouldJumpUp;
    private bool shouldJumpSide;
    private bool isJumping;
    private bool stopMoving;
    private bool stopChasing;
    private bool isOnLand;
    private bool isOnPlatform;
    private bool isPlayerDirectlyAbove;
    private bool isPlayerDirectlyBelow;
    private bool isPlayerAnyBelow;

    void Awake() {
        myRigidBody = GetComponent<Rigidbody2D>();
        myBoxCollider = GetComponent<BoxCollider2D>();
    }

    private void Start() {
        playerTransform = PlayerMovement.Instance.transform;
        platformCollider = Platform.instance.GetComponent<CompositeCollider2D>();
    }

    private void FixedUpdate() {
        UpdateTargetInfo();

        // Stop moving if player is too far
        if (distanceToPlayer > chaseRange) {
            myRigidBody.linearVelocityX = 0f;
            stopChasing = true;
        }
        else {
            stopChasing = false;
        }

        if (!stopMoving && !isJumping && !stopChasing) {
            HandleGroundMovement();
            DecideJump();
            ApplyJump();
            DecideDroppingDown();
        }

        Flip();
        OnLandCheck();
        OnPlatformCheck();
        PlayerDirectlyAboveCheck();
        PlayerDirectlyBelowCheck();
        PlayerAnyBelowCheck();
    }

    private void UpdateTargetInfo() {
        // Use Mathf.Sign for simple horizontal chasing (most cases)
        distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        direction = Mathf.Sign(playerTransform.position.x - transform.position.x); // return -1 or 1 depending on direction
    }

    private void HandleGroundMovement() {
        if (!isOnLand || shouldJumpUp || isPlayerDirectlyAbove || isPlayerDirectlyBelow) return;

        // Move toward the player
        myRigidBody.linearVelocityX = moveSpeed * direction; // chase player
    }

    private void DecideJump() {
        if (!isOnLand || isPlayerAnyBelow) return;

        // Check for land in front, gap ahead, and platform above
        RaycastHit2D landInFront = Physics2D.Raycast(transform.position, Vector2.right * direction, frontCheckDistance, landLayer);
        RaycastHit2D gapAhead = Physics2D.Raycast(transform.position + new Vector3(direction, 0, 0), Vector2.down, gapCheckDistance, landLayer);
        RaycastHit2D platformAbove = Physics2D.Raycast(transform.position, Vector2.up, landAboveCheckDistance, platformLayer);

        bool hasLandInFront = landInFront.collider;
        bool hasGapAhead = !gapAhead.collider;
        bool hasPlatformAbove = platformAbove.collider != null;
        bool isPlayerOnPlatform = PlayerMovement.Instance.GetIsOnPlatform();

        // Jump conditions
        if (!hasLandInFront && hasGapAhead) {
            shouldJumpSide = true; // jump for gap
        }
        else if (isPlayerDirectlyAbove && hasPlatformAbove && isPlayerOnPlatform) {
            shouldJumpUp = true; // jump for platform
        }

        else if (isPlayerDirectlyAbove && PlayerMovement.Instance.GetIsOnWall()) {
            shouldJumpUp = true; // jump for player on wall
        }
    }

    private void ApplyJump() {
        if (!shouldJumpSide && !shouldJumpUp) return;

        if (shouldJumpUp) {
            float sideJumpPower = 0f;
            myRigidBody.linearVelocity = new Vector2(sideJumpPower, jumpPower);
            shouldJumpUp = false;
        }
        if (shouldJumpSide) {
            float sideJumpPower = 20f;
            myRigidBody.linearVelocity = new Vector2(direction * sideJumpPower, jumpPower);
            shouldJumpSide = false;
        }
        StartCoroutine(JumpCooldown());
    }

    private IEnumerator JumpCooldown() {
        float jumpCooldown = 1f;
        isJumping = true;
        yield return new WaitForSeconds(jumpCooldown);
        isJumping = false;
    }

    private void DecideDroppingDown() {
        if (!isOnLand || isPlayerDirectlyAbove || !isPlayerAnyBelow) return;

        if (isPlayerDirectlyBelow && isOnPlatform) {
            TriggerDisablePlatformCollision();
        }
    }

    private void TriggerDisablePlatformCollision() {
        if (disableCollisionCoroutine != null)
            StopCoroutine(disableCollisionCoroutine);

        disableCollisionCoroutine = StartCoroutine(DisablePlatformCollision());
    }

    private IEnumerator DisablePlatformCollision() {
        float dropDuration = 0.4f;
        Physics2D.IgnoreCollision(myBoxCollider, platformCollider, true);
        yield return new WaitForSeconds(dropDuration);
        Physics2D.IgnoreCollision(myBoxCollider, platformCollider, false);
        disableCollisionCoroutine = null;
    }

    private void OnLandCheck() {
        isOnLand = Physics2D.OverlapBox(landCheck.position, landCheckSize, 0f, landLayer);
    }

    private void OnPlatformCheck() {
        isOnPlatform = Physics2D.OverlapBox(landCheck.position, landCheckSize, 0f, platformLayer);
    }

    private void PlayerDirectlyAboveCheck() {
        // Player directly above detection
        isPlayerDirectlyAbove = Physics2D.Raycast(transform.position, Vector2.up, playerAboveCheckDistance, playerLayer);
    }

    private void PlayerDirectlyBelowCheck() {
        // Player directly below detection
        isPlayerDirectlyBelow = Physics2D.Raycast(transform.position, Vector2.down, playerBelowCheckDistance, playerLayer);
    }

    private void PlayerAnyBelowCheck() {
        float playerBelowYOffset = 1f;
        isPlayerAnyBelow = playerTransform.position.y < transform.position.y - playerBelowYOffset; // small offset to avoid noise
    }

    private void Flip() {
        // Flip character
        if (direction > 0f && !isFacingRight) FlipCharacter(true);
        else if (direction < 0f && isFacingRight) FlipCharacter(false);
    }

    private void FlipCharacter(bool faceRight) {
        isFacingRight = faceRight;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (faceRight ? 1f : -1f);
        transform.localScale = scale;
    }

    public void TriggerStopMoving() {
        if (stopMovingCoroutine != null) {
            StopCoroutine(stopMovingCoroutine);
        }
        stopMovingCoroutine = StartCoroutine(StopMoving());
    }

    private IEnumerator StopMoving() {
        float stopDuration = 2f;
        stopMoving = true;
        yield return new WaitForSeconds(stopDuration);
        stopMoving = false;
        stopMovingCoroutine = null; // clear reference
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(landCheck.position, landCheckSize);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Vector2.up * playerAboveCheckDistance);
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, Vector2.down * playerBelowCheckDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector2.up * landAboveCheckDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector2.right * frontCheckDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position + Vector3.right, Vector2.down * gapCheckDistance);
    }

    public float GetHorizontalSpeed() {
        return Mathf.Abs(myRigidBody.linearVelocityX);
    }
}