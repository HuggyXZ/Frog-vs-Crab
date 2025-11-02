using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    public event EventHandler OnPowerUpReady;

    int progressAmount;

    [SerializeField] private Slider progressSlider;
    [SerializeField] private GameObject gameOverScreen;

    private void Awake() {
        Instance = this;
    }

    void Start() {
        Star.OnStarCollect += Star_OnStarCollect;
        PlayerMovement.Instance.OnPowerUp += PlayerMovement_OnPowerUp;
        PlayerHealth.Instance.OnPlayerDied += PlayerHealth_OnPlayerDied;

        progressAmount = 0;
        progressSlider.value = 0;

        gameOverScreen.SetActive(false);
    }

    private void Star_OnStarCollect(int starValue) {
        progressAmount += starValue;
        progressSlider.value = progressAmount;

        if (progressAmount >= 100) {
            OnPowerUpReady?.Invoke(this, EventArgs.Empty);
            Debug.Log("Power Up Ready");
        }
    }

    private void PlayerMovement_OnPowerUp(object sender, EventArgs e) {
        progressAmount = 0;
        progressSlider.value = 0;
    }

    private void PlayerHealth_OnPlayerDied(object sender, EventArgs e) {
        StartCoroutine(ShowGameOverScreen());
    }

    private IEnumerator ShowGameOverScreen() {
        yield return new WaitForSeconds(3f);
        gameOverScreen.SetActive(true);
    }

    public void ResetGame() {
        gameOverScreen.SetActive(false);
        SceneManager.LoadScene(0);
    }

    public int GetProgressAmount() {
        return progressAmount;
    }

    private void OnDestroy() {
        Star.OnStarCollect -= Star_OnStarCollect;
    }
}
