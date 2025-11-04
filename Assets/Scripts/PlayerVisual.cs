using UnityEngine;
using System.Collections;
using System;

public class PlayerVisual : MonoBehaviour {
    public static PlayerVisual Instance { get; private set; }

    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ParticleSystem smokeFX;
    [SerializeField] private TrailRenderer trailRenderer;

    private bool powerUp;

    [Header("SmokeFX")]
    private float timeSinceLastFlip = 0; // time since last flip
    [SerializeField] private float minSmokeFXTime = 1; // minimum time between smokeFX plays

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        // Subscribe to player events
        PlayerMovement.Instance.OnJump += Player_OnJump;
        PlayerMovement.Instance.OnAirJump += Player_OnAirJump;
        PlayerMovement.Instance.OnWallJump += Player_OnWallJump;
        PlayerMovement.Instance.OnDash += Player_OnDash;
        PlayerMovement.Instance.OnDashEnd += Player_OnDashEnd;
        PlayerMovement.Instance.OnLanded += Player_OnLanded;
        PlayerMovement.Instance.OnFlip += Player_OnFlip;
        Star.OnStarCollect += Star_OnStarCollect;
        HealthItem.OnHealthCollect += HealthItem_OnHealthCollect;
        PlayerShoot.Instance.OnShoot += PlayerShoot_OnShoot;
        HoldToPowerUp.Instance.OnPowerUp += HoldToPowerUp_OnPowerUp;
        HoldToPowerUp.Instance.OnPowerUpEnd += HoldToPowerUp_OnPowerUpEnd;
        PlayerMovement.Instance.OnStartMovingSameDirection += Player_OnStartMovingSameDirection;
        PlayerHealth.Instance.OnPlayerDied += PlayerHealth_OnPlayerDied;
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
        animator.SetBool("isOnLand", PlayerMovement.Instance.GetIsOnLand());
        animator.SetBool("isFalling", PlayerMovement.Instance.GetIsFalling());
        animator.SetBool("isWallSliding", PlayerMovement.Instance.GetIsWallSliding());
    }

    private void Player_OnJump(object sender, EventArgs e) {
        animator.SetTrigger("jumpTrigger");
        smokeFX.Play();
    }
    private void Player_OnAirJump(object sender, EventArgs e) {
        animator.SetTrigger("airJumpTrigger");
        smokeFX.Play();
    }
    private void Player_OnWallJump(object sender, EventArgs e) {
        animator.SetTrigger("wallJumpTrigger");
        smokeFX.Play();
    }

    private void Player_OnDash(object sender, EventArgs e) {
        animator.SetTrigger("dashTrigger");
        trailRenderer.emitting = true;
    }

    private void Player_OnDashEnd(object sender, EventArgs e) {
        trailRenderer.emitting = false;
    }

    private void Player_OnLanded(object sender, EventArgs e) {
        animator.SetTrigger("landTrigger");
    }

    private void Player_OnFlip(object sender, EventArgs e) {
        bool isGrounded = PlayerMovement.Instance.GetIsOnLand();
        if (isGrounded && timeSinceLastFlip > minSmokeFXTime) {
            smokeFX.Play();
        }
        timeSinceLastFlip = 0;
    }

    private void Star_OnStarCollect(int starValue) {
        animator.SetTrigger("collectTrigger");
    }

    private void HealthItem_OnHealthCollect(int healthValue) {
        animator.SetTrigger("collectTrigger");
    }

    public void OnPlayerHit() {
        animator.SetTrigger("hitTrigger");
        StartCoroutine(FlashRed());
    }

    private IEnumerator FlashRed() {
        spriteRenderer.color = Color.indianRed;
        float flashDuration = 0.5f;
        yield return new WaitForSeconds(flashDuration);
        if (powerUp) {
            spriteRenderer.color = Color.yellow;
        }
        else {
            spriteRenderer.color = Color.white;
        }
    }

    private void PlayerShoot_OnShoot(object sender, EventArgs e) {
        animator.SetTrigger("shootTrigger");
    }

    private void HoldToPowerUp_OnPowerUp(object sender, EventArgs e) {
        spriteRenderer.color = Color.yellow;
        powerUp = true;
    }

    private void HoldToPowerUp_OnPowerUpEnd(object sender, EventArgs e) {
        spriteRenderer.color = Color.white;
        powerUp = false;
    }

    private void Player_OnStartMovingSameDirection(object sender, EventArgs e) {
        smokeFX.Play();
    }

    private void PlayerHealth_OnPlayerDied(object sender, EventArgs e) {
        StartCoroutine(PlayerDied());
    }

    private IEnumerator PlayerDied() {
        animator.SetTrigger("dieTrigger");
        yield return new WaitForSeconds(3f);
        animator.enabled = false;
        this.enabled = false;
    }

    private void OnDisable() {
        PlayerMovement.Instance.OnJump -= Player_OnJump;
        PlayerMovement.Instance.OnAirJump -= Player_OnAirJump;
        PlayerMovement.Instance.OnWallJump -= Player_OnWallJump;
        PlayerMovement.Instance.OnLanded -= Player_OnLanded;
        PlayerMovement.Instance.OnFlip -= Player_OnFlip;
        PlayerMovement.Instance.OnStartMovingSameDirection -= Player_OnStartMovingSameDirection;
        PlayerShoot.Instance.OnShoot -= PlayerShoot_OnShoot;
        PlayerHealth.Instance.OnPlayerDied -= PlayerHealth_OnPlayerDied;
    }
}
