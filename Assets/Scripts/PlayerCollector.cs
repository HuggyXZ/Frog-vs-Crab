using UnityEngine;
using System;

public class PlayerCollector : MonoBehaviour {
    public static PlayerCollector Instance { get; private set; }

    public event EventHandler OnItemCollect;
    private void Awake() {
        Instance = this;
    }

    private void Start() {
        PlayerHealth.Instance.OnPlayerDied += PlayerHealth_OnPlayerDied;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.TryGetComponent(out IItem item)) {
            OnItemCollect?.Invoke(this, EventArgs.Empty);
            item.Collect();
        }
    }

    private void PlayerHealth_OnPlayerDied(object sender, EventArgs e) {
        this.enabled = false;
    }

    private void OnDisable() {
        PlayerHealth.Instance.OnPlayerDied -= PlayerHealth_OnPlayerDied;
    }
}
