using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    int progressAmount;

    [SerializeField] GameObject LoadCanvas;
    [SerializeField] private Slider progressSlider;

    private void Awake() {
        Instance = this;
    }

    void Start() {
        Star.OnStarCollect += Star_OnStarCollect;
        PlayerMovement.Instance.OnPowerUp += PlayerMovement_OnPowerUp;

        progressAmount = 0;
        progressSlider.value = 0;
        LoadCanvas.SetActive(false);
    }

    private void Star_OnStarCollect(int starValue) {
        progressAmount += starValue;
        progressSlider.value = progressAmount;

        if (progressAmount >= 100) {
            LoadCanvas.SetActive(true);
            Debug.Log("Power Up Ready");
        }
    }

    private void PlayerMovement_OnPowerUp(object sender, System.EventArgs e) {
        LoadCanvas.SetActive(false);
        progressAmount = 0;
        progressSlider.value = 0;
    }

    private void OnDestroy() {
        Star.OnStarCollect -= Star_OnStarCollect;
    }
}
