using System.Collections;
using UnityEngine;
using System;

public class SpikeTrap : MonoBehaviour {
    public static event Action SpikeTrapTrigger;
    [SerializeField] private float bounceForce = 30f;
    [SerializeField] private int damage = 1;

    private bool canTriggerTrap = true;

    private void Start() {
        PlayerHealth.Instance.OnPlayerDie += PlayerHealth_OnPlayerDied;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (!canTriggerTrap) return;
        if (collision.gameObject.TryGetComponent(out PlayerHealth player)) {
            Debug.Log("Player hit");
            StartCoroutine(PlayerMovement.Instance.HandleSpikeTrapKnockback());
            StartCoroutine(TriggerTrapCooldown());
            SpikeTrapTrigger?.Invoke();

            // apply bounce
            Rigidbody2D playerRigidBody = player.GetComponent<Rigidbody2D>();
            playerRigidBody.linearVelocityY = 0f;
            playerRigidBody.linearVelocity = Vector2.up * bounceForce;

            // apply damage
            PlayerHealth.Instance.TakeDamge(damage);
        }
    }

    private IEnumerator TriggerTrapCooldown() {
        float trapCooldown = 0.5f;
        canTriggerTrap = false;
        yield return new WaitForSeconds(trapCooldown);
        canTriggerTrap = true;
    }

    private void PlayerHealth_OnPlayerDied(object sender, System.EventArgs e) {
        StopAllCoroutines();
        canTriggerTrap = false;
        this.enabled = false;
        Debug.Log("Player died");
    }

    private void OnDisable() {
        PlayerHealth.Instance.OnPlayerDie -= PlayerHealth_OnPlayerDied;
    }
}
