using Unity.VisualScripting;
using UnityEngine;

public class EnemyVisual : MonoBehaviour {

    [SerializeField] private Animator animator;

    void LateUpdate() {
        animator.SetFloat("horizontalSpeed", EnemyMovement.Instance.GetHorizontalSpeed());
    }
}
