using UnityEngine;

public class Bullet : MonoBehaviour {

    private int bulletDamage;

    private void Start() {
        bulletDamage = PlayerShoot.Instance.GetBulletDamage();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.TryGetComponent(out EnemyStats enemy)) {
            enemy.TakeDamage(bulletDamage);
            Destroy(gameObject);
        }
    }
}
