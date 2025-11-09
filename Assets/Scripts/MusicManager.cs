using UnityEngine;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour {
    public static MusicManager Instance { get; private set; }
    private AudioSource audioSource;
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private Slider musicSlider;

    void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();
    }

    void Start() {
        SetVolume(musicSlider.value);

        if (backgroundMusic != null) {
            PlayBackgroundMusic(false, backgroundMusic);
        }

        musicSlider.onValueChanged.AddListener(delegate { SetVolume(musicSlider.value); });
    }

    public void SetVolume(float volume) {
        audioSource.volume = volume;
    }

    public void PlayBackgroundMusic(bool resetSong, AudioClip audioClip = null) {
        if (audioClip != null) {
            audioSource.clip = audioClip;
        }
        if (audioSource.clip != null) {
            if (resetSong) {
                audioSource.Stop();
            }
            audioSource.Play();
        }
    }

    public void PauseBackgroundMusic() {
        audioSource.Pause();
    }
}
