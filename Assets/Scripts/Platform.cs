using UnityEngine;

public class Platform : MonoBehaviour {
    public static Platform Instance { get; private set; }

    private void Awake() {
        Instance = this;
    }
}
