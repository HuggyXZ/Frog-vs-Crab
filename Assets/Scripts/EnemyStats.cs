using System;
using System.Collections;
using UnityEngine;

public class EnemyStats : MonoBehaviour {
    public static event Action OnAttackPlayer;

    private EnemyMovement enemyMovement;

    [SerializeField] private int damage = 1;
    [SerializeField] private int health = 100;

    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Transform sidecheck;
    [SerializeField] private Vector2 sideCheckSize = new Vector2(4f, 2f);

    private bool inAttackRange;
    private bool canAttack = true;
    private bool canDamage = true;

    private void Awake() {
        enemyMovement = GetComponent<EnemyMovement>();
    }

    private void Start() {
        PlayerHealth.Instance.OnPlayerDied += PlayerHealth_OnPlayerDied;
    }

    private void Update() {
        OnSideCheck();
    }

    private void OnSideCheck() {
        inAttackRange = Physics2D.OverlapBox(sidecheck.position, sideCheckSize, 0f, playerLayer);

        if (inAttackRange && canAttack) {
            OnAttackPlayer?.Invoke();
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
        if (!this.enabled) return;  // stop executing if disabled

        if (collision.gameObject.TryGetComponent(out PlayerHealth playerHealth) && canDamage) {
            Debug.Log("Hit Player");
            PlayerMovement.Instance.OnHitByEnemy(transform.position);
            PlayerVisual.Instance.OnPlayerHit();
            playerHealth.TakeDamge(damage);
            StartCoroutine(StopHiting());
            enemyMovement.TriggerStopMoving();
        }
    }

    public IEnumerator StopHiting() {
        float stopDuration = 1f;
        canDamage = false;
        yield return new WaitForSeconds(stopDuration);
        canDamage = true;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(sidecheck.position, sideCheckSize);
    }

    private void PlayerHealth_OnPlayerDied(object sender, EventArgs e) {
        this.enabled = false;
    }
}
