using System;
using UnityEngine;
using UnityEngine.UI;

public class HoldToPowerUp : MonoBehaviour {
    public static event Action OnHoldComplete;

    public Image fillCircle;

    public float holdTime = 1; // How long to hold to activate
    private float holdTimer = 0;

    // Update is called once per frame
    void Update() {
        HandleHoldInput();
    }

    private void HandleHoldInput() {
        if (GameInput.Instance.IsHoldActionPressed()) {
            holdTimer += Time.deltaTime;
            fillCircle.fillAmount = holdTimer / holdTime;

            if (holdTimer >= holdTime) {
                OnHoldComplete?.Invoke();
                holdTimer = 0;
                fillCircle.fillAmount = 0;
            }
        }

        if (GameInput.Instance.IsHoldActionReleased()) {
            holdTimer = 0;
            fillCircle.fillAmount = 0;
        }
    }
}
