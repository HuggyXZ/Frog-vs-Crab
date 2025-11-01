using UnityEngine;

public class EnemyVisual : MonoBehaviour {

    private void Start() {
        EnemyStats.Instance.OnAttackPlayer += EnemyStats_OnAttackPlayer;
    }

    [SerializeField] private Animator animator;

    void LateUpdate() {
        animator.SetFloat("horizontalSpeed", EnemyMovement.Instance.GetHorizontalSpeed());
    }

    private void EnemyStats_OnAttackPlayer(object sender, System.EventArgs e) {
        animator.SetTrigger("attackTrigger");
    }
}
