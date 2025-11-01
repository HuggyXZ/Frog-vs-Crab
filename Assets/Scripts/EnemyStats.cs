using System;
using System.Collections;
using UnityEngine;

public class EnemyStats : MonoBehaviour {
    public event EventHandler OnAttackPlayer;

    private EnemyMovement enemyMovement;

    [SerializeField] private int damage = 1;
    [SerializeField] private int health = 100;

    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Transform sidecheck;
    [SerializeField] private Vector2 sideCheckSize = new Vector2(2f, 2f);

    private bool playerSideCheck;
    private bool canAttack = true;

    private void Awake() {
        enemyMovement = GetComponent<EnemyMovement>();
    }

    private void Update() {
        OnSideCheck();
    }

    private void OnSideCheck() {
        playerSideCheck = Physics2D.OverlapBox(sidecheck.position, sideCheckSize, 0f, playerLayer);

        if (playerSideCheck && canAttack) {
            OnAttackPlayer?.Invoke(this, EventArgs.Empty);
            StartCoroutine(AttackCooldown());
        }
    }

    private IEnumerator AttackCooldown() {
        float attackCooldown = 1f; // time between damage ticks
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        TryDamagePlayer(collision);
    }

    private void OnCollisionStay2D(Collision2D collision) {
        TryDamagePlayer(collision);
    }

    private void TryDamagePlayer(Collision2D collision) {
        if (collision.gameObject.TryGetComponent(out PlayerHealth playerHealth) && !enemyMovement.GetStopMoving()) {
            PlayerMovement.Instance.OnHitByEnemy(transform.position);
            PlayerVisual.Instance.OnPlayerHit();
            playerHealth.TakeDamge(damage);
            enemyMovement.StartCoroutine(enemyMovement.StopMoving());
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(sidecheck.position, sideCheckSize);
    }
}
