using System;
using UnityEngine;
using UnityEngine.UI;

public class HoldToPowerUp : MonoBehaviour {

    [SerializeField] private Image fillCircle;
    [SerializeField] private RectTransform fillCircleRectTransform;

    private bool canHold = false;
    [SerializeField] private float holdTime = 1; // How long to hold to activate
    private float holdTimer;

    private Transform playerTransform;

    private void Start() {
        GameManager.OnPowerUpReady += GameManager_OnPowerUpReady;
        PlayerMovement.Instance.OnPowerUpCounterUpdate += PlayerMovement_OnPowerUpCounterUpdate;
    }

    // Update is called once per frame
    void Update() {
        playerTransform = PlayerMovement.Instance.transform;
        gameObject.transform.position = playerTransform.position;
        HandleHoldInput();
    }

    private void HandleHoldInput() {
        if (PlayerMovement.Instance.GetPowerUpCounter() > 0 || !canHold) return;

        if (GameInput.Instance.IsHoldActionPressed()) {
            fillCircleRectTransform.localScale = Vector3.one;

            holdTimer += Time.deltaTime;
            fillCircle.fillAmount = holdTimer / holdTime;

            if (holdTimer >= holdTime) {
                fillCircle.fillAmount = 1;
                fillCircleRectTransform.localScale = new Vector3(-1, 1, 1);

                PlayerMovement.Instance.ActivatePowerUp();
                holdTimer = 0;
                canHold = false;
            }
        }

        if (GameInput.Instance.IsHoldActionReleased()) {
            holdTimer = 0;
            fillCircle.fillAmount = 0;
        }
    }

    private void GameManager_OnPowerUpReady() {
        canHold = true;
    }

    private void PlayerMovement_OnPowerUpCounterUpdate(object sender, EventArgs e) {
        float ratio = PlayerMovement.Instance.GetPowerUpCounter() / PlayerMovement.Instance.GetPowerUpTime();
        fillCircle.fillAmount = ratio;
    }
}
