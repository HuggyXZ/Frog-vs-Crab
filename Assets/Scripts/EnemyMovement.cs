using System.Collections;
using UnityEngine;

public class EnemyMovement : MonoBehaviour {
    private Rigidbody2D myRigidBody; ////////////////// Enemy move down when player is down, move down on platform

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpPower = 25f;
    [SerializeField] private float chaseRange = 15f; // how far the enemy can detect/chase the player
    float distanceToPlayer;

    private float direction;
    private bool isMovingRight = true; // input direction for Flip()
    private bool isFacingRight = true;

    [Header("Checks")]
    [SerializeField] private float playerAboveCheckDistance = 15f;
    [SerializeField] private float landAboveCheckDistance = 7f;
    [SerializeField] private float frontCheckDistance = 2f;
    [SerializeField] private float gapCheckDistance = 2f;

    [Header("References & Layers")]
    private Transform playerTransform;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private LayerMask landLayer;
    [SerializeField] private Transform landCheck;
    [SerializeField] private Vector2 landCheckSize = new Vector2(2f, 0.025f);

    private bool isOnLand;
    private bool isPlayerAbove;
    private bool shouldJump;
    private bool isJumping;
    private bool stopMoving;

    void Awake() {
        myRigidBody = GetComponent<Rigidbody2D>();
    }

    private void Start() {
        playerTransform = PlayerMovement.Instance.transform;
    }

    private void FixedUpdate() {
        if (!stopMoving && !isJumping) {
            HandleGroundMovement();
            DecideJump();
            ApplyJump();
            Flip();
        }

        OnLandCheck();
    }

    private void OnLandCheck() {
        isOnLand = Physics2D.OverlapBox(landCheck.position, landCheckSize, 0f, landLayer);
    }

    private void HandleGroundMovement() {
        if (!isOnLand || shouldJump || isPlayerAbove) return;

        // Calculate distance to player
        distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Stop moving if player is too far
        if (distanceToPlayer > chaseRange) {
            myRigidBody.linearVelocityX = 0f;
            return;
        }

        // Move toward the player
        // Use Mathf.Sign for simple horizontal chasing (most cases)
        direction = Mathf.Sign(playerTransform.position.x - transform.position.x); // return -1 or 1 depending on direction
        myRigidBody.linearVelocityX = moveSpeed * direction; // chase player

        // Flip character
        if (direction < 0f) {
            isMovingRight = false;
        }
        else if (direction > 0f) {
            isMovingRight = true;
        }
    }

    private void DecideJump() {
        if (!isOnLand || shouldJump) return;

        // Stop moving if player is too far
        if (distanceToPlayer > chaseRange) {
            myRigidBody.linearVelocityX = 0f;
            return;
        }

        // Player above detection
        isPlayerAbove = Physics2D.Raycast(transform.position, Vector2.up, playerAboveCheckDistance, playerLayer);

        // Check for land in front, gap ahead, and platform above
        RaycastHit2D landInFront = Physics2D.Raycast(transform.position, Vector2.right * direction, frontCheckDistance, landLayer);
        RaycastHit2D gapAhead = Physics2D.Raycast(transform.position + new Vector3(direction, 0, 0), Vector2.down, gapCheckDistance, landLayer);
        RaycastHit2D platformAbove = Physics2D.Raycast(transform.position, Vector2.up, landAboveCheckDistance, platformLayer);

        bool hasLandInFront = landInFront.collider;
        bool hasGapAhead = !gapAhead.collider;
        bool hasPlatformAbove = platformAbove.collider != null;

        // Jump conditions
        if (!hasLandInFront && hasGapAhead) {
            shouldJump = true; // jump for gap
        }
        else if (isPlayerAbove && hasPlatformAbove && PlayerMovement.Instance.GetIsOnPlatform()) {
            shouldJump = true; // jump for platform
        }

        else if (isPlayerAbove && PlayerMovement.Instance.GetIsOnWall()) {
            shouldJump = true; // jump for player on wall
        }
    }

    private void ApplyJump() {
        if (isOnLand && shouldJump && !isJumping) {
            shouldJump = false;
            Debug.Log("isOnLand: " + isOnLand);
            Debug.Log("Jump!");

            if (isPlayerAbove) {
                myRigidBody.linearVelocity = new Vector2(0f, jumpPower);
            }
            else {
                float sideJumpPower = 15f;

                myRigidBody.linearVelocity = new Vector2(direction * sideJumpPower, jumpPower);
            }
            StartCoroutine(JumpCooldown());
        }
    }

    private IEnumerator JumpCooldown() {
        float jumpCooldown = 1f;
        isJumping = true;
        yield return new WaitForSeconds(jumpCooldown);
        isJumping = false;
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

    public IEnumerator StopMoving() {
        stopMoving = true;
        float stopDuration = 1f;
        yield return new WaitForSeconds(stopDuration);
        stopMoving = false;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(landCheck.position, landCheckSize);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Vector2.up * playerAboveCheckDistance);
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

    public bool GetStopMoving() { return stopMoving; }
}