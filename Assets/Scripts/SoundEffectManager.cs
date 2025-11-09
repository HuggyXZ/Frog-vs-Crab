using UnityEngine;
using UnityEngine.UI;

public class SoundEffectManager : MonoBehaviour {
    public static SoundEffectManager Instance { get; private set; }

    private AudioSource audioSource;
    private SoundEffectLibrary soundEffectLibrary;
    [SerializeField] private Slider sfxSlider;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();
        soundEffectLibrary = GetComponent<SoundEffectLibrary>();
    }

    void Start() {
        SetVolume(sfxSlider.value);
        sfxSlider.onValueChanged.AddListener(delegate { SetVolume(sfxSlider.value); });
    }

    public void Play(string soundName) {
        AudioClip audioClip = soundEffectLibrary.GetRandomClip(soundName);
        if (audioClip != null) {
            audioSource.PlayOneShot(audioClip);
        }
    }

    public void SetVolume(float volume) {
        audioSource.volume = volume;
    }
}
