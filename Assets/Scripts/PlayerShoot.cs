using UnityEngine;
using System;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerShoot : MonoBehaviour {
    public static PlayerShoot Instance { get; private set; }

    public event EventHandler OnShoot;

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 15f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private int bulletDamage = 5;
    private bool canShoot = true;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        WizardHat.OnWizardHatCollect += WizardHat_OnWizardHatCollect;
        PlayerHealth.Instance.OnPlayerDie += PlayerHealth_OnPlayerDied;
    }

    void Update() {
        if (GameInput.Instance.WasShootActionPerformed() && canShoot) {
            StartCoroutine(Shoot());
        }
    }

    private IEnumerator Shoot() {
        canShoot = false;
        OnShoot?.Invoke(this, EventArgs.Empty);

        // Get the mouse position in screen space and convert it to world space
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePosition.z = 0f; // Make sure it’s in the same 2D plane as the player
        // Calculate shoot direction
        // Use .normalized when only need the direction, not the magnitude (length, distance). Magnitude defaults to 1.
        Vector2 shootDirection = (mousePosition - transform.position).normalized; // Vector2

        // Calculate rotation angle (in degrees)
        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
        Quaternion bulletRotation = Quaternion.Euler(0, 0, angle);

        // Spawn bullet
        GameObject bullet = Instantiate(bulletPrefab, transform.position, bulletRotation);
        bullet.GetComponent<Rigidbody2D>().linearVelocity = shootDirection * bulletSpeed;

        Destroy(bullet, 0.5f);

        yield return new WaitForSeconds(1f / fireRate);
        canShoot = true;
    }

    private void WizardHat_OnWizardHatCollect(float bulletSpeedIncrease, float fireRateIncrease, int bulletDamageIncrease) {
        bulletSpeed += bulletSpeedIncrease;
        fireRate += fireRateIncrease;
        bulletDamage += bulletDamageIncrease;
    }

    public int GetBulletDamage() { return bulletDamage; }

    private void PlayerHealth_OnPlayerDied(object sender, EventArgs e) {
        this.enabled = false;
    }
}
