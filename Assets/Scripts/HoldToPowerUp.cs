using System;
using UnityEngine;
using UnityEngine.UI;

public class HoldToPowerUp : MonoBehaviour {
    //public static event Action OnHoldComplete;

    public Image fillCircle;

    private bool canHold = false;
    public float holdTime = 1; // How long to hold to activate
    private float holdTimer;

    private void Start() {
        GameManager.OnPowerUpReady += GameManager_OnPowerUpReady;
        PlayerMovement.Instance.OnPowerUpCounterUpdate += PlayerMovement_OnPowerUpCounterUpdate;
    }

    // Update is called once per frame
    void Update() {
        HandleHoldInput();
    }

    private void HandleHoldInput() {
        if (!canHold) {
            return;
        }
        if (GameInput.Instance.IsHoldActionPressed()) {
            holdTimer += Time.deltaTime;
            fillCircle.fillAmount = holdTimer / holdTime;

            if (holdTimer >= holdTime) {
                PlayerMovement.Instance.ActivatePowerUp();
                holdTimer = 0;
                fillCircle.fillAmount = 0;
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
