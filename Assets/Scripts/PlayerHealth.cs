using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour {

    public int maxHealth = 5;
    private int currentHealth;

    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Start() {
        currentHealth = maxHealth;
        HealthUI.Instance.SetMaxHearts(maxHealth);
    }

    public void TakeDamge(int damage) {
        currentHealth -= damage;
        HealthUI.Instance.UpdateHearts(currentHealth);

        StartCoroutine(FlashRed());

        if (currentHealth <= 0) {
            Debug.Log("Player died!");
        }
    }

    private IEnumerator FlashRed() {
        spriteRenderer.color = Color.red;

        float flashDuration = 0.3f;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = Color.white;
    }
}
