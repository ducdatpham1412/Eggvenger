using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager> {
    public enum MusicSource {
        background,
        matchScene,
    }
    protected SoundManager() { }
    private Dictionary<MusicSource, AudioClip> Sources = new Dictionary<MusicSource, AudioClip>();
    private AudioSource Music;
    public AudioSource KnockWood;


    void Awake() {
        Music = gameObject.AddComponent<AudioSource>();
        KnockWood = gameObject.AddComponent<AudioSource>();

        Music.playOnAwake = false;
        Music.loop = true;

        KnockWood.playOnAwake = false;
        KnockWood.loop = false;

        Sources[MusicSource.background] = LoadMusic("mc_newdayagain");
        Sources[MusicSource.matchScene] = LoadMusic("mc_life_wandering");

        KnockWood.clip = LoadSF("sf_knockwood");
    }

    private AudioClip LoadMusic(string name) {
        return Resources.Load<AudioClip>($"Sounds/Musics/{name}");
    }

    private AudioClip LoadSF(string name) {
        return Resources.Load<AudioClip>($"Sounds/SFs/{name}");
    }


    public void PauseUnPauseMusicBackground() {
        if (Music != null) {
            if (Music.isPlaying) {
                Music.Pause();
            }
            else {
                Music.UnPause();
            }
        }
    }


    public void PlayKnowWood() {
        if (KnockWood != null) {
            KnockWood.Play();
        }
    }


    public void PlayMusic(MusicSource source) {
        if (Sources.ContainsKey(source)) {
            if (Music.isPlaying) {
                Music.Pause();
            }
            Music.clip = Sources[source];
            Music.Play();
        }
    }
}
