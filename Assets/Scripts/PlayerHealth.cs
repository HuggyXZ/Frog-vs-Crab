using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour {
    public static PlayerHealth Instance { get; private set; }

    public event EventHandler OnPlayerDied;

    [SerializeField] private int maxHealth = 5;
    private int currentHealth;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        HealthItem.OnHealthCollect += HealthItem_OnHealthCollect;
        currentHealth = maxHealth;
        HealthUI.Instance.SetMaxHearts(maxHealth);
    }

    public void TakeDamge(int damage) {
        currentHealth -= damage;
        HealthUI.Instance.UpdateHearts(currentHealth);

        if (currentHealth <= 0) {
            OnPlayerDied?.Invoke(this, EventArgs.Empty);
            this.enabled = false;
        }
    }

    private void HealthItem_OnHealthCollect(int healthIncrease, int maxHealthIncrease) {
        if (maxHealthIncrease > 0) {
            maxHealth += maxHealthIncrease;
            HealthUI.Instance.SetMaxHearts(maxHealth);
        }

        currentHealth += healthIncrease;
        if (currentHealth > maxHealth) {
            currentHealth = maxHealth;
        }
        HealthUI.Instance.UpdateHearts(currentHealth);
    }

    public int GetMaxHealth() { return maxHealth; }

    public int GetCurrentHealth() { return currentHealth; }

    private void OnDisable() {
        HealthItem.OnHealthCollect -= HealthItem_OnHealthCollect;
    }
}
