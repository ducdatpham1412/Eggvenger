using UnityEngine;

public class SoundManager : Singleton<SoundManager> {
    // public static SoundManager Instance;
    protected SoundManager() { }
    private AudioSource musicBackground;
    public AudioSource KnockWood;


    // void Awake() {
    //     if (Instance == null) {
    //         Instance = this;
    //         DontDestroyOnLoad(gameObject); // Prevent this GameObject from being destroyed
    //     }
    //     else if (Instance != this) {
    //         Destroy(gameObject); // Destroy duplicate SoundManager instances
    //     }
    // }


    void Start() {
        musicBackground = GetComponent<AudioSource>();
    }

    public void PauseUnPauseMusicBackground() {
        if (musicBackground.isPlaying) {
            musicBackground.Pause();
        }
        else {
            musicBackground.UnPause();
        }
    }

    public void PlayKnowWood() {
        if (KnockWood != null) {
            KnockWood.Play();
        }
    }
}
