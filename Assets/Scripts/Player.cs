using UnityEngine;

public class Player : MonoBehaviour {

    private Rigidbody2D myRigidBody;
    [SerializeField] private float walkSpeed = 10f;
    [SerializeField] private float jumpStrength = 20f;
    private bool isJumping;
    private bool isGrounded;

    private void Awake() {
        myRigidBody = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        if (GameInput.Instance.IsJumpActionPressed() && isGrounded) {
            isJumping = true;
        }
    }
    private void FixedUpdate() {
        if (GameInput.Instance.IsRightActionPressed()) {
            myRigidBody.linearVelocityX = walkSpeed;
        }

        if (GameInput.Instance.IsLeftActionPressed()) {
            myRigidBody.linearVelocityX = -walkSpeed;
        }

        if (isJumping) {
            myRigidBody.linearVelocityY = jumpStrength;
            isJumping = false;
            isGrounded = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Ground") {
            Debug.Log("Grounded");
            isGrounded = true;
        }
    }
}
