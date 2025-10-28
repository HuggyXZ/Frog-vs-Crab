using System;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    int progressAmount;
    [SerializeField] private Slider progressSlider;

    void Start() {
        progressAmount = 0;
        progressSlider.value = 0;
        Star.OnStarCollect += Star_OnStarCollect;
    }

    private void Star_OnStarCollect(int starValue) {
        progressAmount += starValue;
        progressSlider.value = progressAmount;

        if (progressAmount >= 100) {
            Debug.Log("You win!");
        }
    }
}
