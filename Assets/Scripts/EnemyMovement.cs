using UnityEngine;

public class EnemyMovement : MonoBehaviour {
    public static EnemyMovement Instance { get; private set; }
    private Rigidbody2D myRigidBody;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpPower = 15f;
    [SerializeField] private float chaseRange = 15f; // how far the enemy can detect/chase the player
    private float direction;

    [Header("Checks")]
    [SerializeField] private float playerAboveCheckDistance = 15f;
    [SerializeField] private float landAboveCheckDistance = 7f;
    [SerializeField] private float frontCheckDistance = 2f;
    [SerializeField] private float gapCheckDistance = 2f;

    [Header("References & Layers")]
    private Transform playerTransform;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Transform landCheck;
    [SerializeField] private Vector2 landCheckSize = new Vector2(2f, 0.025f);
    [SerializeField] private LayerMask landLayer;
    [SerializeField] private LayerMask platformLayer;

    private bool isOnLand;
    private bool isPlayerAbove;
    private bool shouldJump;

    void Awake() {
        Instance = this;
        myRigidBody = GetComponent<Rigidbody2D>();
    }

    private void Start() {
        playerTransform = PlayerMovement.Instance.transform;
    }

    // Update is called once per frame
    private void Update() {
        OnLandCheck();
        HandleGroundMovement();
        HandleJump();
    }

    private void FixedUpdate() {
        ApplyJump();
    }

    private void OnLandCheck() {
        isOnLand = Physics2D.OverlapBox(landCheck.position, landCheckSize, 0f, landLayer);
    }

    private void HandleGroundMovement() {
        if (!isOnLand || shouldJump) return;

        // Calculate distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        Debug.Log(distanceToPlayer);

        // Stop moving if player is too far
        if (distanceToPlayer > chaseRange) {
            myRigidBody.linearVelocityX = 0f;
            return;
        }

        // Stop if player is above
        if (isPlayerAbove) return;

        // Move toward the player
        // Use Mathf.Sign for simple horizontal chasing (most cases)
        direction = Mathf.Sign(playerTransform.position.x - transform.position.x); // return -1 or 1 depending on direction
        myRigidBody.linearVelocityX = moveSpeed * direction; // chase player
    }

    private void HandleJump() {
        if (!isOnLand || shouldJump) return;
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
        else if (isPlayerAbove && hasPlatformAbove) {
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
}