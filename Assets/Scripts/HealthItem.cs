using UnityEngine;
using System;

public class HealthItem : MonoBehaviour, IItem {
    public static event Action<int> OnHealthCollect;

    [SerializeField] private int healthAmount = 1;

    public void Collect() {
        if (PlayerHealth.Instance.GetCurrentHealth() >= PlayerHealth.Instance.GetMaxHealth()) return;
        OnHealthCollect?.Invoke(healthAmount);
        Destroy(gameObject);
    }
}
