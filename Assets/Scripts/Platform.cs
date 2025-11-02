using UnityEngine;

public class Platform : MonoBehaviour {
    public static Platform instance { get; private set; }

    private void Awake() {
        instance = this;
    }
}
