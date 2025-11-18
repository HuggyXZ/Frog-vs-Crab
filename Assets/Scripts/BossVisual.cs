using UnityEngine;
using System.Collections;

public class BossVisual : MonoBehaviour {
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    private Coroutine flashPurpleCoroutine;

    void Start() {
        BossStats.Instance.OnAttackPlayer += BossStats_OnAttackPlayer;
        BossStats.Instance.OnGetHit += BossStats_OnGetHit;
    }

    private void Update() {
        RotateBoss();
    }

    private void RotateBoss() {
        Vector2 direction = PlayerMovement.Instance.transform.position - transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 200 * Time.deltaTime);
    }

    private void BossStats_OnAttackPlayer(object sender, System.EventArgs e) {
        animator.SetTrigger("attackTrigger");
    }

    private void BossStats_OnGetHit(object sender, System.EventArgs e) {
        TriggerFlashPurple();
    }

    public void TriggerFlashPurple() {
        if (flashPurpleCoroutine != null) {
            StopCoroutine(flashPurpleCoroutine);
        }
        flashPurpleCoroutine = StartCoroutine(FlashPurple());
    }

    private IEnumerator FlashPurple() {
        spriteRenderer.color = Color.purple;
        float flashDuration = 0.3f;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = Color.white;
    }
}
