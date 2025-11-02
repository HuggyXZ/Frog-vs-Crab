using UnityEngine;

public class Bullet : MonoBehaviour {

    public int bulletDamage = 1;

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.TryGetComponent(out EnemyStats enemy)) {
            enemy.TakeDamage(bulletDamage);
            Destroy(gameObject);
        }
    }
}
