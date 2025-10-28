using UnityEngine;

public class PlayerVisual : MonoBehaviour {

    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem smokeFX;

    [Header("SmokeFX")]
    private float timeSinceLastFlip = 0; // time since last flip
    [SerializeField] private float minSmokeFXTime = 1; // minimum time between smokeFX plays

    private void Start() {
        // Subscribe to player events
        PlayerMovement.Instance.OnJump += Player_OnJump;
        PlayerMovement.Instance.OnAirJump += Player_OnAirJump;
        PlayerMovement.Instance.OnWallJump += Player_OnWallJump;
        PlayerMovement.Instance.OnLanded += Player_OnLanded;
        PlayerMovement.Instance.OnFlip += Player_OnFlip;
        PlayerMovement.Instance.OnStartMovingSameDirection += Player_OnStartMovingSameDirection;
    }

    // Update is called once per frame
    void Update() {
        // DON'T update physics-derived animator params here.

        if (timeSinceLastFlip <= minSmokeFXTime) {
            timeSinceLastFlip += Time.deltaTime;
        }
    }

    private void LateUpdate() {
        // Update animator AFTER physics (FixedUpdate) so velocity values are up-to-date
        animator.SetFloat("horizontalSpeed", PlayerMovement.Instance.GetHorizontalSpeed());
        animator.SetBool("isGrounded", PlayerMovement.Instance.GetIsGrounded());
        animator.SetBool("isFalling", PlayerMovement.Instance.GetIsFalling());
        animator.SetBool("isWallSliding", PlayerMovement.Instance.GetIsWallSliding());
    }

    private void Player_OnJump(object sender, System.EventArgs e) {
        animator.SetTrigger("jumpTrigger");
        smokeFX.Play();
    }
    private void Player_OnAirJump(object sender, System.EventArgs e) {
        animator.SetTrigger("airJumpTrigger");
        smokeFX.Play();
    }
    private void Player_OnWallJump(object sender, System.EventArgs e) {
        animator.SetTrigger("wallJumpTrigger");
        smokeFX.Play();
    }
    private void Player_OnLanded(object sender, System.EventArgs e) {
        animator.SetTrigger("landTrigger");
    }

    private void Player_OnFlip(object sender, System.EventArgs e) {
        bool isGrounded = PlayerMovement.Instance.GetIsGrounded();
        if (isGrounded && timeSinceLastFlip > minSmokeFXTime) {
            smokeFX.Play();
        }
        timeSinceLastFlip = 0;
    }

    private void Player_OnStartMovingSameDirection(object sender, System.EventArgs e) {
        smokeFX.Play();
    }
}
