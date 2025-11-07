using UnityEngine;
using System;

public class WizardHat : MonoBehaviour, IItem {
    public static event Action<float, float, int> OnWizardHatCollect;

    [SerializeField] private float bulletSpeedIncrease = 5f;
    [SerializeField] private float fireRateIncrease = 0.5f;
    [SerializeField] private int bulletDamageIncrease = 5;

    public void Collect() {
        OnWizardHatCollect?.Invoke(bulletSpeedIncrease, fireRateIncrease, bulletDamageIncrease);
        Destroy(gameObject);
    }
}
