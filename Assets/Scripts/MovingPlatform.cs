using UnityEngine;

public class MovingPlatform : MonoBehaviour {
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float moveSpeed = 2f;

    private Vector3 targetPosition;

    // Store the world positions at start
    private Vector3 pointAWorld;
    private Vector3 pointBWorld;

    void Start() {
        pointAWorld = pointA.position;
        pointBWorld = pointB.position;

        transform.position = pointAWorld; // Start at pointA
        targetPosition = pointBWorld;
    }

    void Update() {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Check if close enough to switch
        if (transform.position == targetPosition) {
            targetPosition = (targetPosition == pointAWorld) ? pointBWorld : pointAWorld;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.TryGetComponent(out PlayerMovement player)) {
            player.transform.SetParent(transform);
            player.GetComponent<Rigidbody2D>().interpolation = RigidbodyInterpolation2D.None;
        }

        if (collision.gameObject.TryGetComponent(out EnemyMovement enemy)) {
            enemy.transform.SetParent(transform);
            enemy.GetComponent<Rigidbody2D>().interpolation = RigidbodyInterpolation2D.None;
        }
    }

    private void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.TryGetComponent(out PlayerMovement player)) {
            player.transform.SetParent(null);
            player.GetComponent<Rigidbody2D>().interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        if (collision.gameObject.TryGetComponent(out EnemyMovement enemy)) {
            enemy.transform.SetParent(null);
            enemy.GetComponent<Rigidbody2D>().interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }
}
