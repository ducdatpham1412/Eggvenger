using System.Collections.Generic;
using UnityEngine;

public abstract class BaseSkill : MonoBehaviour {
    [Header("Properties")]
    [SerializeField] protected float speed = 0.6f;
    public bool canReady = true;

    [Header("Durations")]
    [SerializeField] protected float lifeDuration = 1f;
    [SerializeField] protected float fadeDuration = 0f;
    [SerializeField] public float effectDuration = 1f;

    [Header("Effects")]
    [SerializeField] public float effectSpeed; // Ratio speed to current speed. Ex: If you want decrease 20% speed -> Set "speed" = 0.8

    [Header("Sounds")]
    [SerializeField] AudioClip ReadySound;
    [SerializeField] AudioClip PlaySound;

    [Header("Others")]
    public PlayerManager Creator;
    public GameObject OriginalPrefab;

    // Others
    protected Rigidbody2D rb;
    protected bool collided = false;
    protected bool played = false;
    PlayerSkill playerSkill;
    [HideInInspector] public List<PlayerManager> effectedPlayers = new List<PlayerManager>();

    protected virtual void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    public virtual void Ready(Vector3 direction) {
        // TODO: Playing sound ready
    }

    public virtual void Play(Vector3 direction) {
        // TODO: Playing sound play
        played = true;
    }

    void LateUpdate() {
        if (!played) {
            if (playerSkill == null) {
                playerSkill = Creator.GetComponent<PlayerSkill>();
            }
            transform.position = playerSkill.GetSkillSpawnPos(isLocal: false);
        }
    }

    protected void RotateFollowDirection(Vector3 direction) {
        float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, -angle);
    }

    protected string GetLayerName(GameObject gameObject) {
        return LayerMask.LayerToName(gameObject.layer);
    }

    protected WaitUntil WaitToStop(float magnitudeThreshold = 0.4f) {
        return new WaitUntil(() => rb.linearVelocity.magnitude <= magnitudeThreshold);
    }
}
