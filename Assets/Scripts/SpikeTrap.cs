using System.Collections;
using UnityEngine;

public class SpikeTrap : MonoBehaviour {
    [SerializeField] private float bounceForce = 30f;
    [SerializeField] private int damage = 1;

    private bool canTriggerTrap = true;

    private void OnTriggerEnter2D(Collider2D collision) {
        if (!canTriggerTrap) return;
        if (collision.gameObject.TryGetComponent(out PlayerHealth player)) {
            StartCoroutine(PlayerMovement.Instance.HandleSpikeTrapKnockback());
            StartCoroutine(TriggerTrapCooldown());

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
}
