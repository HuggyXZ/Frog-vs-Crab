using UnityEngine;
using System;
using System.Collections;

public class EnemyVisual : MonoBehaviour {
    private EnemyStats enemyStats;
    private EnemyMovement enemyMovement;

    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private Animator animator;

    private void Awake() {
        enemyStats = GetComponent<EnemyStats>();
        enemyMovement = GetComponent<EnemyMovement>();
    }

    private void Start() {
        enemyStats.OnAttackPlayer += EnemyStats_OnAttackPlayer;
        enemyStats.OnGetHit += EnemyStats_OnGetHit;
        enemyStats.OnDie += EnemyStats_OnDie;
        PlayerHealth.Instance.OnPlayerDied += PlayerHealth_OnPlayerDied;
    }

    void LateUpdate() {
        animator.SetFloat("horizontalSpeed", enemyMovement.GetHorizontalSpeed());
    }

    private void EnemyStats_OnAttackPlayer() {
        animator.SetTrigger("attackTrigger");
    }

    private void EnemyStats_OnGetHit() {
        animator.SetTrigger("hitTrigger");
        StartCoroutine(FlashPurple());
    }

    private void EnemyStats_OnDie() {
        animator.SetTrigger("dieTrigger");
        this.enabled = false;
    }

    private IEnumerator FlashPurple() {
        spriteRenderer.color = Color.purple;
        float flashDuration = 0.5f;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = Color.white;
    }

    private void PlayerHealth_OnPlayerDied(object sender, EventArgs e) {
        this.enabled = false;
    }

    private void OnDisable() {
        spriteRenderer.color = Color.purple;

        enemyStats.OnAttackPlayer -= EnemyStats_OnAttackPlayer;
        enemyStats.OnGetHit -= EnemyStats_OnGetHit;
        enemyStats.OnDie -= EnemyStats_OnDie;
        PlayerHealth.Instance.OnPlayerDied -= PlayerHealth_OnPlayerDied;
    }
}
