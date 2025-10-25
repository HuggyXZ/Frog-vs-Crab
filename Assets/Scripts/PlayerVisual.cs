using UnityEngine;

public class PlayerVisual : MonoBehaviour {

    private Animator animator;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void Start() {
        // Subscribe to player events
        Player.Instance.OnJump += Player_OnJump;
        Player.Instance.OnAirJump += Player_OnAirJump;
        Player.Instance.OnWallJump += Player_OnWallJump;
        Player.Instance.OnLanded += Player_OnLanded;
    }

    // Update is called once per frame
    void Update() {
        // DON'T update physics-derived animator params here.
    }

    private void LateUpdate() {
        // Update animator AFTER physics (FixedUpdate) so velocity values are up-to-date
        animator.SetFloat("horizontalSpeed", Player.Instance.GetHorizontalSpeed());
        animator.SetBool("isGrounded", Player.Instance.GetIsGrounded());
        animator.SetBool("isFalling", Player.Instance.GetIsFalling());
        animator.SetBool("isWallSliding", Player.Instance.GetIsWallSliding());
    }

    private void Player_OnJump(object sender, System.EventArgs e) {
        animator.SetTrigger("jumpTrigger");
    }
    private void Player_OnAirJump(object sender, System.EventArgs e) {
        animator.SetTrigger("airJumpTrigger");
    }
    private void Player_OnWallJump(object sender, System.EventArgs e) {
        animator.SetTrigger("wallJumpTrigger");
    }
    private void Player_OnLanded(object sender, System.EventArgs e) {
        animator.SetTrigger("landTrigger");
    }
}
