using UnityEngine;

public class EnemyAudio : MonoBehaviour {
    private EnemyStats enemyStats;

    private void Awake() {
        enemyStats = GetComponent<EnemyStats>();
    }

    private void Start() {
        enemyStats.OnAttackPlayer += EnemyStats_OnAttackPlayer;
        enemyStats.OnGetHit += EnemyStats_OnGetHit;
        enemyStats.OnDie += EnemyStats_OnDie;
    }

    private void EnemyStats_OnAttackPlayer() {
        SoundEffectManager.Instance.Play("EnemyAttack");
    }

    private void EnemyStats_OnGetHit() {
        SoundEffectManager.Instance.Play("EnemyGetHit");
    }

    private void EnemyStats_OnDie() {
        SoundEffectManager.Instance.Play("EnemyDie");
        this.enabled = false;
    }

    private void OnDisable() {
        enemyStats.OnAttackPlayer -= EnemyStats_OnAttackPlayer;
        enemyStats.OnGetHit -= EnemyStats_OnGetHit;
        enemyStats.OnDie -= EnemyStats_OnDie;
    }
}
