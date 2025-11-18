using UnityEngine;
using System.Collections;
using System;

public class BossStats : MonoBehaviour {
    public static BossStats Instance { get; private set; }
    public event EventHandler OnGetHit;
    public event EventHandler OnAttackPlayer;

    [SerializeField] private int Maxhealth = 30;
    [SerializeField] private int damage = 3;
    [SerializeField] private int currentHealth;

    private bool canDamage = true;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        currentHealth = Maxhealth;
        Physics2D.IgnoreLayerCollision(12, 9, true);
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.TryGetComponent(out PlayerHealth player) && canDamage) {
            TryDamagePlayer();
        }
    }

    private void OnCollisionStay2D(Collision2D collision) {
        if (collision.gameObject.TryGetComponent(out PlayerHealth player) && canDamage) {
            TryDamagePlayer();
        }
    }

    private void TryDamagePlayer() {
        OnAttackPlayer?.Invoke(this, EventArgs.Empty);
        PlayerMovement.Instance.OnHitByEnemy(transform.position);
        PlayerHealth.Instance.TakeDamge(damage);
        StartCoroutine(StopHiting());
    }

    public IEnumerator StopHiting() {
        canDamage = false;
        float stopDuration = 1f;
        yield return new WaitForSeconds(stopDuration);
        canDamage = true;
    }

    public void TakeDamage(int bulletDamage) {
        currentHealth -= bulletDamage;
        if (currentHealth <= 0) {
            Debug.Log("Boss died");
            //StartCoroutine(Die());
        }
        else {
            OnGetHit?.Invoke(this, EventArgs.Empty);
        }
    }
}
