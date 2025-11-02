using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour {
    public static PlayerShoot Instance { get; private set; }

    public event EventHandler OnShoot;

    private Vector2 mousePosition;
    private Vector2 offset;
    private Vector3 screenPoint;

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 15f;

    private void Awake() {
        Instance = this;
    }

    void Update() {
        if (GameInput.Instance.WasShootActionPerformed()) {
            Shoot();
        }
    }

    private void Shoot() {
        OnShoot?.Invoke(this, EventArgs.Empty);
        // Get the mouse position in screen space and convert it to world space
        mousePosition = Mouse.current.position.ReadValue();
        screenPoint = Camera.main.WorldToScreenPoint(transform.position);

        // Calculate shoot direction
        offset = new Vector2(mousePosition.x - screenPoint.x, mousePosition.y - screenPoint.y).normalized;

        // Spawn bullet
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().linearVelocity = offset * bulletSpeed;

        Destroy(bullet, 3f);
    }
}
