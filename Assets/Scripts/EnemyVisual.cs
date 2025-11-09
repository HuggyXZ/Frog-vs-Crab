using UnityEngine;
using System;
using System.Collections;

public class EnemyVisual : MonoBehaviour {
    private EnemyStats enemyStats;
    private EnemyMovement enemyMovement;

    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private Animator animator;

    private Coroutine flashPurpleCoroutine;

    private void Awake() {
        enemyStats = GetComponent<EnemyStats>();
        enemyMovement = GetComponent<EnemyMovement>();
    }

    private void Start() {
        enemyStats.OnAttackPlayer += EnemyStats_OnAttackPlayer;
        enemyStats.OnGetHit += EnemyStats_OnGetHit;
        enemyStats.OnDie += EnemyStats_OnDie;
        PlayerHealth.Instance.OnPlayerDie += PlayerHealth_OnPlayerDied;
    }

    void LateUpdate() {
        animator.SetFloat("horizontalSpeed", enemyMovement.GetHorizontalSpeed());
    }

    private void EnemyStats_OnAttackPlayer() {
        animator.SetTrigger("attackTrigger");
    }

    private void EnemyStats_OnGetHit() {
        animator.SetTrigger("hitTrigger");
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

    private void EnemyStats_OnDie() {
        if (flashPurpleCoroutine != null) {
            StopCoroutine(flashPurpleCoroutine);
        }
        spriteRenderer.color = Color.purple;
        animator.SetTrigger("dieTrigger");
        this.enabled = false;
    }

    private void PlayerHealth_OnPlayerDied(object sender, EventArgs e) {
        this.enabled = false;
    }

    private void OnDisable() {
        enemyStats.OnAttackPlayer -= EnemyStats_OnAttackPlayer;
        enemyStats.OnGetHit -= EnemyStats_OnGetHit;
        enemyStats.OnDie -= EnemyStats_OnDie;
        PlayerHealth.Instance.OnPlayerDie -= PlayerHealth_OnPlayerDied;
    }
}
