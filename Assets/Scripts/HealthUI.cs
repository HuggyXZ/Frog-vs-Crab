using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour {
    public static HealthUI Instance { get; private set; }

    [SerializeField] private Image heartPrefab;
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;

    private List<Image> hearts = new List<Image>();

    private void Awake() {
        Instance = this;
    }

    public void SetMaxHearts(int maxHearts) {
        foreach (Image heart in hearts) {
            Destroy(heart.gameObject);
        }
        hearts.Clear();

        for (int i = 0; i < maxHearts; i++) {
            Image newHeart = Instantiate(heartPrefab, transform);
            newHeart.sprite = fullHeartSprite;
            hearts.Add(newHeart);
        }
    }

    public void UpdateHearts(int currentHealth) {
        for (int i = 0; i < hearts.Count; i++) {
            if (i < currentHealth) {
                hearts[i].sprite = fullHeartSprite;
            }
            else {
                hearts[i].sprite = emptyHeartSprite;
            }
        }
    }
}
