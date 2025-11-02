using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour {
    public static PlayerHealth Instance { get; private set; }

    public event EventHandler OnPlayerDied;

    public int maxHealth = 5;
    private int currentHealth;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        currentHealth = maxHealth;
        HealthUI.Instance.SetMaxHearts(maxHealth);
    }

    public void TakeDamge(int damage) {
        currentHealth -= damage;
        HealthUI.Instance.UpdateHearts(currentHealth);

        if (currentHealth <= 0) {
            Debug.Log("Player died!");
            OnPlayerDied?.Invoke(this, EventArgs.Empty);
            this.enabled = false;
        }
    }
}
