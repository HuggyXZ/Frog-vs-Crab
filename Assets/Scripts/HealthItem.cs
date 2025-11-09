using UnityEngine;
using System;

public class HealthItem : MonoBehaviour, IItem {
    public static event Action<int, int> OnHealthCollect;

    [SerializeField] private int healthIncrease = 1;
    [SerializeField] private int maxHealthIncrease = 1;

    public void Collect() {
        if (maxHealthIncrease == 0 && PlayerHealth.Instance.GetCurrentHealth() == PlayerHealth.Instance.GetMaxHealth()) return;
        OnHealthCollect?.Invoke(healthIncrease, maxHealthIncrease);
        Destroy(gameObject);
    }
}
