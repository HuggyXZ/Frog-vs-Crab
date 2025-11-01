using UnityEngine;

public class EnemyVisual : MonoBehaviour {
    private EnemyStats enemyStats;
    private EnemyMovement enemyMovement;

    [SerializeField] private Animator animator;

    private void Awake() {
        enemyStats = GetComponent<EnemyStats>();
        enemyMovement = GetComponent<EnemyMovement>();
    }

    private void Start() {
        enemyStats.OnAttackPlayer += EnemyStats_OnAttackPlayer;
    }

    void LateUpdate() {
        animator.SetFloat("horizontalSpeed", enemyMovement.GetHorizontalSpeed());
    }

    private void EnemyStats_OnAttackPlayer(object sender, System.EventArgs e) {
        animator.SetTrigger("attackTrigger");
    }

    void OnDestroy() {
        enemyStats.OnAttackPlayer -= EnemyStats_OnAttackPlayer;
    }

}
