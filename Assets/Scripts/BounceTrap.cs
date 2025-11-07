using System.Collections;
using UnityEngine;

public class BounceTrap : MonoBehaviour {
    [SerializeField] private float bounceForce = 60f;
    private bool canTriggerTrap = true;

    [SerializeField] private Animator animator;

    private void OnTriggerEnter2D(Collider2D collision) {
        if (!canTriggerTrap) return;
        if (collision.gameObject.TryGetComponent(out PlayerHealth player)) {
            animator.SetTrigger("bounceTrapTrigger");
            PlayerVisual.Instance.PlayerMovement_OnJump(null, null);
            StartCoroutine(TriggerTrapCooldown());

            // apply bounce
            Rigidbody2D playerRigidBody = player.GetComponent<Rigidbody2D>();
            playerRigidBody.linearVelocityY = 0f;
            playerRigidBody.linearVelocity = Vector2.up * bounceForce;
        }
    }

    private IEnumerator TriggerTrapCooldown() {
        float trapCooldown = 10f;
        canTriggerTrap = false;
        yield return new WaitForSeconds(trapCooldown);
        canTriggerTrap = true;
        animator.SetTrigger("idleTrigger");
    }
}
