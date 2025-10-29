using UnityEngine;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    public static event Action OnPowerUpReady;

    int progressAmount;

    [SerializeField] private Slider progressSlider;

    private void Awake() {
        Instance = this;
    }

    void Start() {
        Star.OnStarCollect += Star_OnStarCollect;
        PlayerMovement.Instance.OnPowerUp += PlayerMovement_OnPowerUp;

        progressAmount = 0;
        progressSlider.value = 0;
    }

    private void Star_OnStarCollect(int starValue) {
        progressAmount += starValue;
        progressSlider.value = progressAmount;

        if (progressAmount >= 100) {
            OnPowerUpReady?.Invoke();
            Debug.Log("Power Up Ready");
        }
    }

    private void PlayerMovement_OnPowerUp(object sender, System.EventArgs e) {
        progressAmount = 0;
        progressSlider.value = 0;
    }

    public int GetProgressAmount() {
        return progressAmount;
    }

    private void OnDestroy() {
        Star.OnStarCollect -= Star_OnStarCollect;
    }
}
