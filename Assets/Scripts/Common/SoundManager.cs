using UnityEngine;

public class SoundManager : Singleton<SoundManager> {
    protected SoundManager() { }
    private AudioSource musicBackground;
    public AudioSource KnockWood;


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
