using UnityEngine;

public class MovingPlatform : MonoBehaviour {
    public static MovingPlatform Instance { get; private set; }
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float moveSpeed = 2f;

    private Vector3 targetPosition;
    private bool movingToB = true;

    void Awake() {
        Instance = this;
    }

    void Start() {
        targetPosition = pointB.position;
    }

    void Update() {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Check if close enough to switch
        if (transform.position == targetPosition) {
            movingToB = !movingToB; // flip direction
            targetPosition = movingToB ? pointB.position : pointA.position;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.TryGetComponent(out PlayerMovement playerMovement)) {
            playerMovement.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.TryGetComponent(out PlayerMovement playerMovement)) {
            playerMovement.transform.SetParent(null);
        }
    }
}
