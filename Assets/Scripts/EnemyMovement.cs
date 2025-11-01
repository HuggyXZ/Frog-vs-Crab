using System.Collections;
using UnityEngine;

public class EnemyMovement : MonoBehaviour {
    public static EnemyMovement Instance { get; private set; }
    private Rigidbody2D myRigidBody;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpPower = 15f;
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
    private bool stopMoving;

    void Awake() {
        Instance = this;
        myRigidBody = GetComponent<Rigidbody2D>();
    }

    private void Start() {
        EnemyStats.Instance.OnPlayerHit += EnemyStats_OnPlayerHit;
        playerTransform = PlayerMovement.Instance.transform;
    }

    // Update is called once per frame
    private void Update() {
        HandleGroundMovement();
        HandleJump();
        Flip();

        OnLandCheck();
    }

    private void FixedUpdate() {
        ApplyJump();
    }

    private void OnLandCheck() {
        isOnLand = Physics2D.OverlapBox(landCheck.position, landCheckSize, 0f, landLayer);
    }

    private void HandleGroundMovement() {
        if (!isOnLand || shouldJump || isPlayerAbove || stopMoving) return;

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

    private void HandleJump() {
        if (!isOnLand || shouldJump || stopMoving) return;

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
            shouldJump = true;
        }
        else if (isPlayerAbove && hasPlatformAbove && PlayerMovement.Instance.GetIsOnPlatform()) {
            shouldJump = true;
        }

        else if (isPlayerAbove && PlayerMovement.Instance.GetIsOnWall()) {
            shouldJump = true;
        }
    }

    private void ApplyJump() {
        if (isOnLand && shouldJump) {
            shouldJump = false;

            // Use .normalized if you need the enemy to chase in both X and Y directions
            // or if you’re implementing diagonal movement toward the player.
            Vector2 direction = (playerTransform.position - transform.position).normalized; // return -1 or 1 depending on direction
            myRigidBody.linearVelocity = new Vector2(direction.x * moveSpeed, jumpPower);
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

    private void EnemyStats_OnPlayerHit(object sender, System.EventArgs e) {
        StartCoroutine(StopMoving());
    }

    private IEnumerator StopMoving() {
        stopMoving = true;
        float stopDuration = 2f;
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