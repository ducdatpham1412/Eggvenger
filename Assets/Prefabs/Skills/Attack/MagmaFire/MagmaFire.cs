using System.Collections;
using UnityEngine;

public class MagmaFire : BaseSkill {
    bool exploded = false;

    [Header("GameObjects")]
    [SerializeField] GameObject Ball;
    [SerializeField] ParticleSystem FireWall;

    Coroutine BurnCoroutine;

    public override void Ready(Vector3 pos, Vector3 direction) {
        transform.position = pos;
        rb.linearVelocity = Vector2.zero;
        RotateFollowDirection(direction);
    }

    public override void Play(Vector3 pos, Vector3 direction) {
        transform.position = pos;
        rb.AddForce(direction.normalized * speed, ForceMode2D.Impulse);
        BurnCoroutine = StartCoroutine(WaitForStopAndBurn());
    }

    IEnumerator WaitForStopAndBurn() {
        yield return WaitToStop();
        FireWall.gameObject.SetActive(true);
        Ball.SetActive(false);
        yield return new WaitUntil(() => !FireWall.IsAlive(true));
        Destroy(gameObject);
    }

    protected override void OnTriggerEnter2D(Collider2D collider) {
        if (exploded) return;

        void BurnSoon() {
            exploded = true;
            rb.linearVelocity = Vector3.zero;
            StopCoroutine(BurnCoroutine);
            StartCoroutine(WaitForStopAndBurn());
        }

        string layerName = GetLayerName(collider.gameObject);

        if (layerName == Helper.Layer.Obstacle.ToString()) {
            BurnSoon();
        }
        else if (layerName == Helper.Layer.Player.ToString()) {
            PlayerManager player = collider.gameObject.GetComponent<PlayerManager>();
            if (Creator.team != player.team) {
                BurnSoon();
                FireWall.GetComponent<MagmaParticles>().BurnPlayer(player);
            }
        }
    }

    public float GetEffectDuration() {
        return effectDuration;
    }

    public PlayerManager GetCreator() {
        return Creator;
    }
}
