using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour {

    public int maxHealth = 5;
    private int currentHealth;

    private void Start() {
        currentHealth = maxHealth;
        HealthUI.Instance.SetMaxHearts(maxHealth);
    }

    public void TakeDamge(int damage) {
        currentHealth -= damage;
        HealthUI.Instance.UpdateHearts(currentHealth);

        if (currentHealth <= 0) {
            Debug.Log("Player died!");
        }
    }

}
