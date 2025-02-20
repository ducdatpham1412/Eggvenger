using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BindingRoot : BaseSkill {
    float radius = 5f;
    float speed = 30f;
    int playerLayer;

    float expandDuration = 2f;
    float lifeDuration = 3f;
    float fadeDuration = 2.5f;
    int bindDuration = 1500;

    Rigidbody2D rigidbody;

    [Header("GameObjects")]
    [SerializeField] GameObject BindingCycle;
    [SerializeField] GameObject Ball;

    [Header("VFXs")]
    [SerializeField] Sprite BindingEffect;

    [Header("Properties")]
    public PlayerManager Creator;

    Color originColor;
    Coroutine BindingCycleCoroutine;
    Coroutine ExpandCoroutine;

    List<PlayerManager> bindPlayers = new List<PlayerManager>();

    void Awake() {
        rigidbody = GetComponent<Rigidbody2D>();
        originColor = BindingCycle.GetComponent<SpriteRenderer>().color;
        playerLayer = LayerMask.GetMask(Helper.Layer.Player.ToString());
    }

    void OnEnable() {
        BindingCycle.GetComponent<SpriteRenderer>().color = originColor;
    }

    public override void Play(Vector3 pos, Vector3 direction) {
        gameObject.SetActive(true);
        Ball.SetActive(true);
        transform.position = pos;
        rigidbody.AddForce(direction.normalized * speed, ForceMode2D.Force);
        ExpandCoroutine = StartCoroutine(WaitForStopAndExpand());
    }

    protected override void OnTriggerEnter2D(Collider2D collider) {
        void ExpandSoon() {
            rigidbody.linearVelocity = Vector3.zero;
            StopCoroutine(ExpandCoroutine);
            StartCoroutine(WaitForStopAndExpand());
        }

        if (LayerMask.LayerToName(collider.gameObject.layer) != Helper.Layer.Player.ToString()) {
            ExpandSoon();
        }
        else {
            PlayerManager player = collider.gameObject.GetComponent<PlayerManager>();
            if (Creator.team != player.team) {
                ExpandSoon();
                BindPlayer(player);
            }
        }
    }

    IEnumerator WaitForStopAndExpand() {
        yield return new WaitUntil(() => rigidbody.linearVelocity.magnitude <= 0.1f);

        /*
        Step 01: Expand BindingCycle when velocity reach nearly 0
        */
        SpriteRenderer renderer = BindingCycle.GetComponent<SpriteRenderer>();
        Material material = renderer.material;
        BindingCycle.SetActive(true);

        float currentRadius;
        float elapsedTime = 0f;
        BindingCycle.transform.localScale = Vector3.one;
        float originalSpriteSize = renderer.bounds.size.x; // have to change localScale to 1 to get original size
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
        while (elapsedTime < lifeDuration) {
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
        if (!bindPlayers.Contains(player)) {
            bindPlayers.Add(player);
            PlayerVFX playerVFX = player.GetComponent<PlayerVFX>();

            // TODO: Disable Character Moving here

            playerVFX.SetEffect(BindingEffect, 0.5f);
            await Task.Delay(bindDuration);
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

        while (elapsedTime < fadeDuration) {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
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
        bindPlayers.Clear();
        gameObject.SetActive(false);
        BindingCycle.SetActive(false);
    }
}
