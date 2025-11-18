using System.Collections;
using UnityEngine;
using System;

public class BounceTrap : MonoBehaviour {
    public static event Action BounceTrapTrigger;

    [SerializeField] private float bounceForce = 60f;
    private bool canTriggerTrap = true;

    [SerializeField] private Animator animator;

    private void Start() {
        PlayerHealth.Instance.OnPlayerDie += PlayerHealth_OnPlayerDied;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (!canTriggerTrap) return;
        if (collision.gameObject.TryGetComponent(out PlayerHealth player)) {
            PlayerVisual.Instance.PlayerMovement_OnJump(null, null);
            animator.SetTrigger("bounceTrapTrigger");
            BounceTrapTrigger?.Invoke();

            // apply bounce
            Rigidbody2D playerRigidBody = player.GetComponent<Rigidbody2D>();
            playerRigidBody.linearVelocityY = 0f;
            playerRigidBody.linearVelocity = Vector2.up * bounceForce;

            StartCoroutine(TriggerTrapCooldown());
        }
    }

    private IEnumerator TriggerTrapCooldown() {
        float trapCooldown = 10f;
        canTriggerTrap = false;
        yield return new WaitForSeconds(trapCooldown);
        canTriggerTrap = true;
        animator.SetTrigger("idleTrigger");
    }

    private void PlayerHealth_OnPlayerDied(object sender, System.EventArgs e) {
        StopAllCoroutines();
        canTriggerTrap = false;
        this.enabled = false;
    }

    private void OnDisable() {
        PlayerHealth.Instance.OnPlayerDie -= PlayerHealth_OnPlayerDied;
    }
}
