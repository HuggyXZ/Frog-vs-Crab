using UnityEngine;

public class PlayerVisual : MonoBehaviour {

    private Animator animator;
    [SerializeField] private ParticleSystem smokeFX;
    [SerializeField] private float timeSinceLastSmokeFX = 0;
    private float minSmokeFXInterval = 1;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void Start() {
        // Subscribe to player events
        Player.Instance.OnJump += Player_OnJump;
        Player.Instance.OnAirJump += Player_OnAirJump;
        Player.Instance.OnWallJump += Player_OnWallJump;
        Player.Instance.OnLanded += Player_OnLanded;
        Player.Instance.OnFlip += Player_OnFlip;
        Player.Instance.OnStartMovingSameDirection += Player_OnStartMovingSameDirection;
    }

    // Update is called once per frame
    void Update() {
        // DON'T update physics-derived animator params here.
        timeSinceLastSmokeFX = Mathf.Min(timeSinceLastSmokeFX + Time.deltaTime, minSmokeFXInterval);
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
        bool isGrounded = Player.Instance.GetIsGrounded();
        if (isGrounded && timeSinceLastSmokeFX >= minSmokeFXInterval) {
            smokeFX.Play();
        }
        timeSinceLastSmokeFX = 0;
    }

    private void Player_OnStartMovingSameDirection(object sender, System.EventArgs e) {
        smokeFX.Play();
    }
}
