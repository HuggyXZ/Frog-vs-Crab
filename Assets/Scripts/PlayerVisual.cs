using UnityEngine;
using System.Collections;
using System;

public class PlayerVisual : MonoBehaviour {
    public static PlayerVisual Instance { get; private set; }

    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ParticleSystem smokeFX;
    [SerializeField] private ParticleSystem powerUpFX;
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
        PlayerMovement.Instance.OnJump += PlayerMovement_OnJump;
        PlayerMovement.Instance.OnAirJump += PlayerMovement_OnAirJump;
        PlayerMovement.Instance.OnWallJump += PlayerMovement_OnWallJump;
        PlayerMovement.Instance.OnDash += PlayerMovement_OnDash;
        PlayerMovement.Instance.OnDashEnd += PlayerMovement_OnDashEnd;
        PlayerMovement.Instance.OnLanded += PlayerMovement_OnLanded;
        PlayerMovement.Instance.OnFlip += PlayerMovement_OnFlip;
        PlayerCollector.Instance.OnItemCollect += PlayerCollector_OnItemCollect;
        PlayerShoot.Instance.OnShoot += PlayerShoot_OnShoot;
        HoldToPowerUp.Instance.OnPowerUpStart += HoldToPowerUp_OnPowerUpStart;
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

        if (HoldToPowerUp.Instance.GetCanHold() && !powerUp) {
            spriteRenderer.color = Color.Lerp(Color.white, Color.yellow, HoldToPowerUp.Instance.GetCircleFillAmount());
        }
    }

    private void LateUpdate() {
        // Update animator AFTER physics (FixedUpdate) so velocity values are up-to-date
        animator.SetFloat("horizontalSpeed", PlayerMovement.Instance.GetHorizontalSpeed());
        animator.SetBool("isOnLand", PlayerMovement.Instance.GetIsOnLand());
        animator.SetBool("isFalling", PlayerMovement.Instance.GetIsFalling());
        animator.SetBool("isWallSliding", PlayerMovement.Instance.GetIsWallSliding());
    }

    public void PlayerMovement_OnJump(object sender, EventArgs e) {
        animator.SetTrigger("jumpTrigger");
        smokeFX.Play();
    }
    private void PlayerMovement_OnAirJump(object sender, EventArgs e) {
        animator.SetTrigger("airJumpTrigger");
        smokeFX.Play();
    }
    private void PlayerMovement_OnWallJump(object sender, EventArgs e) {
        animator.SetTrigger("wallJumpTrigger");
        smokeFX.Play();
    }

    private void PlayerMovement_OnDash(object sender, EventArgs e) {
        animator.SetTrigger("dashTrigger");
        trailRenderer.emitting = true;
    }

    private void PlayerMovement_OnDashEnd(object sender, EventArgs e) {
        trailRenderer.emitting = false;
    }

    private void PlayerMovement_OnLanded(object sender, EventArgs e) {
        animator.SetTrigger("landTrigger");
    }

    private void PlayerMovement_OnFlip(object sender, EventArgs e) {
        bool isGrounded = PlayerMovement.Instance.GetIsOnLand();
        if (isGrounded && timeSinceLastFlip > minSmokeFXTime) {
            smokeFX.Play();
        }
        timeSinceLastFlip = 0;
    }

    private void PlayerCollector_OnItemCollect(object sender, EventArgs e) {
        animator.SetTrigger("collectTrigger");
    }

    private void PlayerShoot_OnShoot(object sender, EventArgs e) {
        animator.SetTrigger("shootTrigger");
    }

    private void HoldToPowerUp_OnPowerUpStart(object sender, EventArgs e) {
        spriteRenderer.color = Color.yellow;
        powerUpFX.Play();
        var emission = smokeFX.emission;
        emission.enabled = false;

        powerUp = true;
    }

    private void HoldToPowerUp_OnPowerUpEnd(object sender, EventArgs e) {
        StartCoroutine(PowerUpEndFade());
        powerUpFX.Stop();
        smokeFX.Play();
        var emission = smokeFX.emission;
        emission.enabled = true;
        powerUp = false;
    }

    private IEnumerator PowerUpEndFade() {
        float fadeTime = 3f;
        float fadeTimer = 0f;
        while (fadeTimer < fadeTime) {
            spriteRenderer.color = Color.Lerp(Color.yellow, Color.white, fadeTimer / fadeTime);
            fadeTimer += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = Color.white;
    }

    private void Player_OnStartMovingSameDirection(object sender, EventArgs e) {
        smokeFX.Play();
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
        PlayerMovement.Instance.OnJump -= PlayerMovement_OnJump;
        PlayerMovement.Instance.OnAirJump -= PlayerMovement_OnAirJump;
        PlayerMovement.Instance.OnWallJump -= PlayerMovement_OnWallJump;
        PlayerMovement.Instance.OnLanded -= PlayerMovement_OnLanded;
        PlayerMovement.Instance.OnFlip -= PlayerMovement_OnFlip;
        PlayerMovement.Instance.OnStartMovingSameDirection -= Player_OnStartMovingSameDirection;
        PlayerShoot.Instance.OnShoot -= PlayerShoot_OnShoot;
        PlayerHealth.Instance.OnPlayerDied -= PlayerHealth_OnPlayerDied;
    }
}
