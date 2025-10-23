using System;
using UnityEngine;
using UnityEngine.Rendering;

public class Player : MonoBehaviour {

    private Rigidbody2D myRigidBody;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 10f;
    [SerializeField] private float jumpPower = 30f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;
    [SerializeField] private int maxJumpCount = 2;
    [SerializeField] private int jumpRemaining;

    [Header("GroundCheck")]
    [SerializeField] private Transform groundCheckPosition;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    [SerializeField] private LayerMask groundLayer;

    [Header("Gravity")]
    [SerializeField] private float baseGravity = 3f;
    [SerializeField] private float maxFallSpeed = 30f;
    [SerializeField] private float fallMultiplier = 3f;

    private bool isJumping;
    private bool wasGrounded;

    private void Awake() {
        myRigidBody = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        // check if the player is on the ground
        GroundCheck();

        // Jump pressed (start jump)
        if (GameInput.Instance.IsJumpActionPressed() && jumpRemaining > 0) {
            isJumping = true;
            Debug.Log("Jump");
            jumpRemaining--;
        }

        // Jump released early → Light jump
        // Reduce the jump while the player is still going upward
        if (GameInput.Instance.IsJumpActionReleased() && myRigidBody.linearVelocityY > 0) {
            myRigidBody.linearVelocityY *= jumpCutMultiplier;
        }
    }

    private void FixedUpdate() {
        // Gravity
        Gravity();

        // Horizontal movement
        if (GameInput.Instance.IsRightActionPressed()) {
            myRigidBody.linearVelocityX = walkSpeed;
        }

        if (GameInput.Instance.IsLeftActionPressed()) {
            myRigidBody.linearVelocityX = -walkSpeed;
        }

        // Jump
        if (isJumping) {
            myRigidBody.linearVelocityY = jumpPower;
            isJumping = false;
        }
    }

    private void Gravity() {
        // Apply stronger gravity when falling
        if (myRigidBody.linearVelocityY < 0 && !wasGrounded) {
            myRigidBody.gravityScale = baseGravity * fallMultiplier;
        }
        // Apply normal gravity when rising or grounded
        else {
            myRigidBody.gravityScale = baseGravity;
        }

        // Clamp fall speed
        myRigidBody.linearVelocityY = Mathf.Max(myRigidBody.linearVelocityY, -maxFallSpeed);
    }

    private void GroundCheck() {
        bool groundNow = Physics2D.OverlapBox(groundCheckPosition.position, groundCheckSize, 0, groundLayer);

        if (groundNow && !wasGrounded) {
            jumpRemaining = maxJumpCount;
        }

        wasGrounded = groundNow;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheckPosition.position, groundCheckSize);
    }
}
