using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BindingRoot : BaseSkill {
    float radius = 5f;
    int playerLayer;
    bool exploded = false;

    float expandDuration = 2f;

    Rigidbody2D rb;

    [Header("GameObjects")]
    [SerializeField] GameObject BindingCycle;
    [SerializeField] GameObject Ball;

    [Header("VFXs")]
    [SerializeField] Sprite BindingEffect;

    Color originColor;
    Coroutine BindingCycleCoroutine;
    Coroutine ExpandCoroutine;

    List<PlayerManager> tiedPlayers = new List<PlayerManager>();

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        originColor = BindingCycle.GetComponent<SpriteRenderer>().color;
        playerLayer = LayerMask.GetMask(Helper.Layer.Player.ToString());
    }

    void Start() {
        effect.speed = 0f;
        effect.lifeDuration = 2f;
        effect.fadeDuration = 2.5f;
        effect.effectDuration = 1.5f;
    }

    void OnEnable() {
        BindingCycle.GetComponent<SpriteRenderer>().color = originColor;
    }

    public override void Ready(Vector3 pos, Vector3 direction) {
        gameObject.SetActive(true);
        Ball.SetActive(true);
        transform.position = pos;
    }

    public override void Play(Vector3 pos, Vector3 direction) {
        transform.position = pos;
        rb.AddForce(direction.normalized * speed, ForceMode2D.Force);
        ExpandCoroutine = StartCoroutine(WaitForStopAndExpand());
    }

    protected override void OnTriggerEnter2D(Collider2D collider) {
        if (exploded) return;

        void ExpandSoon() {
            exploded = true;
            rb.linearVelocity = Vector3.zero;
            StopCoroutine(ExpandCoroutine);
            StartCoroutine(WaitForStopAndExpand());
        }

        string layerName = LayerMask.LayerToName(collider.gameObject.layer);

        if (layerName == Helper.Layer.Obstacle.ToString()) {
            ExpandSoon();
        }
        else if (layerName == Helper.Layer.Player.ToString()) {
            PlayerManager player = collider.gameObject.GetComponent<PlayerManager>();
            if (Creator.team != player.team) {
                ExpandSoon();
                BindPlayer(player);
            }
        }
    }

    IEnumerator WaitForStopAndExpand() {
        yield return new WaitUntil(() => rb.linearVelocity.magnitude <= 0.1f);

        /*
        Step 01: Expand BindingCycle when velocity reach nearly 0
        */
        SpriteRenderer renderer = BindingCycle.GetComponent<SpriteRenderer>();
        Material material = renderer.material;
        BindingCycle.SetActive(true);

        float currentRadius;
        float elapsedTime = 0f;
        BindingCycle.transform.localScale = Vector3.one;// have to change localScale to 1 to get original size
        float originalSpriteSize = renderer.bounds.size.x;
        Vector3 rotationSpeed = new Vector3(0, 0, 120);
        float scaleFactor;

        BindingCycleCoroutine = StartCoroutine(RotateBindingCycle());

        while (elapsedTime < expandDuration) {
            currentRadius = Mathf.Lerp(0f, radius, elapsedTime / expandDuration);
            scaleFactor = currentRadius * 2 / originalSpriteSize;
            BindingCycle.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);

            float currentFade = Mathf.Lerp(0f, 1f, elapsedTime / expandDuration);
            material.SetFloat("_Fade", currentFade);

            elapsedTime += Time.deltaTime;
            DetectEnemies(currentRadius);
            yield return null;
        }

        /*
        Step 02: Rotate BindingCycle in lifeDuration, and always detect enemies in this duration
        */
        Ball.SetActive(false);
        currentRadius = radius;
        scaleFactor = currentRadius * 2 / originalSpriteSize;
        BindingCycle.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);

        elapsedTime = 0f;
        while (elapsedTime < effect.lifeDuration) {
            elapsedTime += Time.deltaTime;
            DetectEnemies(currentRadius);
            yield return null;
        }

        StartCoroutine(FadeOut());
    }

    void DetectEnemies(float checkRadius) {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, checkRadius, playerLayer);
        foreach (Collider2D e in enemies) {
            PlayerManager player = e.gameObject.GetComponent<PlayerManager>();
            if (player.team != Creator.team) {
                BindPlayer(player);
            }
        }
    }

    async void BindPlayer(PlayerManager player) {
        if (!tiedPlayers.Contains(player)) {
            tiedPlayers.Add(player);
            PlayerVFX playerVFX = player.GetComponent<PlayerVFX>();
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();

            playerVFX.SetEffect(BindingEffect, 0.5f);
            playerMovement.AddSpeedRatio(effect.speed, effect.effectDuration);
            await Task.Delay((int)(effect.effectDuration * 1000));
            playerMovement.RemoveSpeedRatio(effect.speed);
            playerVFX.ResetEffect(BindingEffect, 0f);
        }
    }

    IEnumerator RotateBindingCycle() {
        Vector3 rotationSpeed = new Vector3(0, 0, 120);
        while (true) {
            BindingCycle.transform.Rotate(rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator FadeOut() {
        float elapsedTime = 0f;

        SpriteRenderer renderer = BindingCycle.GetComponent<SpriteRenderer>();
        Material material = renderer.material;

        while (elapsedTime < effect.fadeDuration) {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / effect.fadeDuration);
            float currentFade = Mathf.Lerp(1f, 0f, elapsedTime / expandDuration);
            renderer.color = new Color(
                originColor.r,
                originColor.g,
                originColor.b,
                alpha
            );
            material.SetFloat("_Fade", currentFade);
            yield return null;
        }

        StopCoroutine(BindingCycleCoroutine);
        BindingCycle.transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;
        tiedPlayers.Clear();
        exploded = false;
        gameObject.SetActive(false);
        BindingCycle.SetActive(false);
    }
}
