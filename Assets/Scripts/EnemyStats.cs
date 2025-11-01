using System;
using UnityEngine;

public class EnemyStats : MonoBehaviour {
    public static EnemyStats Instance { get; private set; }

    public event EventHandler OnAttackPlayer;
    public event EventHandler OnPlayerHit;

    [SerializeField] private int damage = 1;
    [SerializeField] private int health = 100;

    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Transform sidecheck;
    [SerializeField] private Vector2 sideCheckSize = new Vector2(2f, 2f);

    private bool playerSideCheck;
    private bool isAttacking;

    private void Awake() {
        Instance = this;
    }

    private void Update() {
        OnSideCheck();
    }

    private void OnSideCheck() {
        playerSideCheck = Physics2D.OverlapBox(sidecheck.position, sideCheckSize, 0f, playerLayer);

        if (playerSideCheck && !isAttacking && !EnemyMovement.Instance.GetStopMoving()) {
            OnAttackPlayer?.Invoke(this, EventArgs.Empty);
            isAttacking = true;
        }

        if (!playerSideCheck) {
            isAttacking = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.TryGetComponent(out PlayerHealth playerHealth) && !EnemyMovement.Instance.GetStopMoving()) {
            OnPlayerHit?.Invoke(this, EventArgs.Empty);
            playerHealth.TakeDamge(damage);
            isAttacking = false;
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(sidecheck.position, sideCheckSize);
    }
}
