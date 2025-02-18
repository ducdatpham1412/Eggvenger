using System.Collections;
using UnityEngine;

public class BindingRoot : BaseSkill {
    float radius = 3f;
    float expandDuration = 0.6f;
    float speed = 30f;
    float fadeDuration = 0.5f;
    LineRenderer lineRenderer;
    Rigidbody2D rigidbody;

    [SerializeField] GameObject CircleEffect;

    SpriteRenderer[] spriteRenderers;
    Color[] originalColors;

    void Awake() {
        lineRenderer = GetComponent<LineRenderer>();
        rigidbody = GetComponent<Rigidbody2D>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++) {
            originalColors[i] = spriteRenderers[i].color;
        }
    }

    void OnEnable() {
        for (int i = 0; i < spriteRenderers.Length; i++) {
            if (spriteRenderers[i] != null) {
                spriteRenderers[i].color = originalColors[i];
            }
        }
    }

    public override void Play(Vector3 pos, Vector3 direction) {
        gameObject.SetActive(true);
        rigidbody.AddForce(direction.normalized * speed, ForceMode2D.Force);
        // TODO: Add flag check when collider or when velocity reach 0
        StartCoroutine(ExpandRadius());
    }

    IEnumerator ExpandRadius() {
        float currentRadius;
        float elapsedTime = 0f;
        float originalSpriteSize = CircleEffect.GetComponent<SpriteRenderer>().bounds.size.x;
        CircleEffect.SetActive(true);

        while (elapsedTime < expandDuration) {
            currentRadius = Mathf.Lerp(0f, radius, elapsedTime / expandDuration);

            if (CircleEffect) {
                float scaleFactor = currentRadius * 2 / originalSpriteSize;
                CircleEffect.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);
            }

            elapsedTime += Time.deltaTime;
            DetectEnemies(currentRadius);
            yield return null;
        }

        currentRadius = radius;
        if (CircleEffect) {
            float scaleFactor = currentRadius * 2 / originalSpriteSize;
            CircleEffect.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);
        }
        yield return new WaitForSeconds(2);
        StartCoroutine(FadeOut());
    }

    void DetectEnemies(float checkRadius) {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, checkRadius, LayerMask.NameToLayer(Helper.Layer.Player.ToString()));
        foreach (Collider2D e in enemies) {
            SetLineToEnemy(e.transform);
        }
    }

    void SetLineToEnemy(Transform enemy) {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, enemy.position);
    }

    IEnumerator FadeOut() {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration) {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            for (int i = 0; i < spriteRenderers.Length; i++) {
                spriteRenderers[i].color = new Color(
                    originalColors[i].r,
                    originalColors[i].g,
                    originalColors[i].b,
                    alpha
                );
            }
            yield return null;
        }

        gameObject.SetActive(false);
        CircleEffect.SetActive(false);
        CircleEffect.transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;
        lineRenderer.positionCount = 0;
    }
}
