using UnityEngine;
using System;

public class EnemyVisual : MonoBehaviour {
    private EnemyStats enemyStats;
    private EnemyMovement enemyMovement;

    [SerializeField] private Animator animator;

    private void Awake() {
        enemyStats = GetComponent<EnemyStats>();
        enemyMovement = GetComponent<EnemyMovement>();
    }

    private void Start() {
        EnemyStats.OnAttackPlayer += EnemyStats_OnAttackPlayer;
        PlayerHealth.Instance.OnPlayerDied += PlayerHealth_OnPlayerDied;
    }

    void LateUpdate() {
        animator.SetFloat("horizontalSpeed", enemyMovement.GetHorizontalSpeed());
    }

    private void EnemyStats_OnAttackPlayer() {
        animator.SetTrigger("attackTrigger");
    }

    private void PlayerHealth_OnPlayerDied(object sender, EventArgs e) {
        this.enabled = false;
    }

    private void OnDestroy() {
        EnemyStats.OnAttackPlayer -= EnemyStats_OnAttackPlayer;
    }

}
