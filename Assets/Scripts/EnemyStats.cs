using System;
using System.Collections;
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

    private void Awake() {
        Instance = this;
    }

    private void Update() {
        OnSideCheck();
    }

    private void OnSideCheck() {
        playerSideCheck = Physics2D.OverlapBox(sidecheck.position, sideCheckSize, 0f, playerLayer);

        if (playerSideCheck && !EnemyMovement.Instance.GetStopMoving()) {
            OnAttackPlayer?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        TryDamagePlayer(collision);
    }

    private void OnCollisionStay2D(Collision2D collision) {
        TryDamagePlayer(collision);
    }

    private void TryDamagePlayer(Collision2D collision) {
        if (collision.gameObject.TryGetComponent(out PlayerHealth playerHealth) && !EnemyMovement.Instance.GetStopMoving()) {
            PlayerMovement.Instance.OnHitByEnemy(transform.position);
            OnPlayerHit?.Invoke(this, EventArgs.Empty);
            playerHealth.TakeDamge(damage);
            StartCoroutine(EnemyMovement.Instance.StopMoving());
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(sidecheck.position, sideCheckSize);
    }
}
