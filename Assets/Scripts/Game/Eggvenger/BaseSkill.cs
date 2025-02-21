using System;
using UnityEngine;

public abstract class BaseSkill : MonoBehaviour {
    [Header("Properties")]
    public PlayerManager Creator;

    protected float speed = 30f;
    public bool canReady = true;
    protected Effect effect = new Effect {
        fadeDuration = 0f,
        speed = 30f,
    };

    public virtual void Ready(Vector3 pos, Vector3 direction) { }

    public abstract void Play(Vector3 pos, Vector3 direction);

    public virtual void Remove() {
        gameObject.SetActive(false);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collider) { }


    [Serializable]
    public class Effect {
        public float speed; // Ratio speed to current speed. Ex: If you want decrease 20% speed -> Set "speed" = 0.8
        public float lifeDuration;
        public float fadeDuration;
        public float effectDuration;
        public AudioClip ReadySound;
        public AudioClip PlaySound;
    }
}
