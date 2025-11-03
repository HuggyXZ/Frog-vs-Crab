using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HoldToPowerUp : MonoBehaviour {
    public static HoldToPowerUp Instance { get; private set; }

    public event EventHandler OnPowerUp;
    public event EventHandler OnPowerUpEnd;

    [SerializeField] private Image fillCircle;
    [SerializeField] private RectTransform fillCircleRectTransform;
    [SerializeField] private Transform playerTransform;

    private bool canHold = false;
    [SerializeField] private float holdTime = 3f; // How long to hold to activate
    private float holdTimer;

    [SerializeField] private float powerUpTime = 15f;
    private float powerUpCounter;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        GameManager.Instance.OnPowerUpReady += GameManager_OnPowerUpReady;
        playerTransform = PlayerMovement.Instance.transform;
    }

    // Update is called once per frame
    void Update() {
        gameObject.transform.position = playerTransform.position;
        HandleHoldInput();
    }

    private void HandleHoldInput() {
        if (!canHold || powerUpCounter > 0) return;

        if (GameInput.Instance.IsHoldActionPressed()) {
            holdTimer += Time.deltaTime;
            fillCircle.fillAmount = holdTimer / holdTime;

            if (holdTimer >= holdTime) {
                OnPowerUp?.Invoke(this, EventArgs.Empty);
                fillCircle.fillAmount = 1;
                fillCircleRectTransform.localScale = new Vector3(-1, 1, 1);
                StartCoroutine(PowerUpDuration());
                holdTimer = 0;
                canHold = false;
            }
        }

        if (GameInput.Instance.IsHoldActionReleased()) {
            holdTimer = 0;
            fillCircle.fillAmount = 0;
        }
    }

    private IEnumerator PowerUpDuration() {
        powerUpCounter = powerUpTime;
        while (powerUpCounter > 0) {
            powerUpCounter -= Time.deltaTime;
            fillCircle.fillAmount = powerUpCounter / powerUpTime;
            yield return null;
        }
        OnPowerUpEnd?.Invoke(this, EventArgs.Empty);
        fillCircle.fillAmount = 0;
        fillCircleRectTransform.localScale = Vector3.one;
        powerUpCounter = 0;
    }

    private void GameManager_OnPowerUpReady(object sender, EventArgs e) {
        canHold = true;
    }

    private void OnDisable() {
        GameManager.Instance.OnPowerUpReady -= GameManager_OnPowerUpReady;
    }
}
