using System;
using UnityEngine;

public class Star : MonoBehaviour, IItem {
    public static event Action<int> OnStarCollect;

    [SerializeField] private int starValue = 5;

    public void Collect() {
        if (GameManager.Instance.GetProgressAmount() < 100) {
            OnStarCollect?.Invoke(starValue);
            Destroy(gameObject);
        }
    }
}
