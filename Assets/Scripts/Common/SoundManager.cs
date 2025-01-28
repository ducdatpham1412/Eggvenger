using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager> {
    public enum MusicSource {
        background,
        lifeWandering,
    }
    protected SoundManager() { }
    Dictionary<MusicSource, AudioClip> Sources = new Dictionary<MusicSource, AudioClip>();
    public AudioSource Music;
    AudioSource KnockWood;
    AudioSource Whoosh;
    AudioSource Stretch;
    AudioSource Bonk;
    AudioSource Splat;
    AudioSource Nope;
    AudioSource HaHa;
    AudioSource NiceShot;

    public event Action<bool> OnMusicPlaying;

    void Awake() {
        Music = gameObject.AddComponent<AudioSource>();
        KnockWood = gameObject.AddComponent<AudioSource>();
        Whoosh = gameObject.AddComponent<AudioSource>();
        Stretch = gameObject.AddComponent<AudioSource>();
        Bonk = gameObject.AddComponent<AudioSource>();
        Splat = gameObject.AddComponent<AudioSource>();
        Nope = gameObject.AddComponent<AudioSource>();
        HaHa = gameObject.AddComponent<AudioSource>();
        NiceShot = gameObject.AddComponent<AudioSource>();

        Sources[MusicSource.background] = LoadMusic("mc_newdayagain");
        Sources[MusicSource.lifeWandering] = LoadMusic("mc_life_wandering");

        KnockWood.clip = LoadSF("sf_knockwood");
        Whoosh.clip = LoadSF("sf_whoosh");
        Stretch.clip = LoadSF("sf_stretch");
        Bonk.clip = LoadSF("sf_bonk");
        Splat.clip = LoadSF("sf_splat");
        Nope.clip = LoadSF("sf_nope");
        HaHa.clip = LoadSF("sf_haha");
        NiceShot.clip = LoadSF("sf_nice_shot");

        AudioSource[] sources = GetComponents<AudioSource>();
        for (int i = 0; i < sources.Length; i++) {
            if (i == 0) {
                sources[i].loop = true;
                sources[i].playOnAwake = true;
            }
            else {
                sources[i].loop = false;
                sources[i].playOnAwake = false;
            }
        }
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
                OnMusicPlaying?.Invoke(false);
            }
            else {
                Music.UnPause();
                OnMusicPlaying?.Invoke(true);
            }
        }
    }

    public void PlaySF(SF sf) {
        if (sf == SF.KnockWood) {
            KnockWood.Play();
            return;
        }
        if (sf == SF.Whoosh) {
            Whoosh.Play();
            return;
        }
        if (sf == SF.Stretch) {
            Stretch.Play();
            return;
        }
        if (sf == SF.Bonk) {
            Bonk.Play();
            return;
        }
        if (sf == SF.Splat) {
            Splat.Play();
            return;
        }
        if (sf == SF.Nope) {
            Nope.Play();
            return;
        }
        if (sf == SF.Haha) {
            HaHa.Play();
            return;
        }
        if (sf == SF.NiceShot) {
            NiceShot.Play();
            return;
        }
    }


    public void PlayMusic(MusicSource source) {
        if (Sources.ContainsKey(source)) {
            if (Music.isPlaying) {
                Music.Pause();
            }
            Music.clip = Sources[source];
            Music.Play();
            OnMusicPlaying?.Invoke(true);
        }
    }


    public enum SF {
        KnockWood,
        Whoosh,
        Stretch,
        Bonk,
        Splat,
        Nope,
        Haha,
        NiceShot,
        None,
    }
}
