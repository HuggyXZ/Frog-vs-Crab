using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour {

    [SerializeField] private Transform player;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpPower = 15f;

    private Rigidbody2D myRigidBody;
    public LayerMask groundLayer;
    public LayerMask platformLayer;
    private bool isOnLand;
    private bool shouldJump;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        myRigidBody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    /*     void Update() {
            // Is standing on land
            isOnLand = Physics2D.Raycast(transform.position, Vector2.down, 1f, groundLayer) ||
                        Physics2D.Raycast(transform.position, Vector2.down, 1f, platformLayer);

            // Player direction
            float direction = Mathf.Sign(player.position.x - transform.position.x);
            // Player above detection
            bool isPlayerAbove = Physics2D.Raycast(transform.position, Vector2.up, 3f, 1 << player.gameObject.layer);

            if (isOnLand) {
                myRigidBody.linearVelocity = new Vector2(direction * moveSpeed, myRigidBody.linearVelocity.y);

                RaycastHit2D groundInfront = Physics2D.Raycast(transform.position, Vector2.right * direction, 2f, groundLayer);
                RaycastHit2D platformInfront = Physics2D.Raycast(transform.position, Vector2.right * direction, 2f, platformLayer);

                RaycastHit2D gapAhead = Physics2D.Raycast(transform.position, Vector2.right * direction, 1f, groundLayer);
                RaycastHit2D gapAheadPlatform = Physics2D.Raycast(transform.position, Vector2.right * direction, 1f, platformLayer);


            }
        } */
}
