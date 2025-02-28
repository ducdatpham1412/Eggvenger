using System.Collections;
using UnityEngine;

public class UnrestLand : BaseSkill {
    protected override void Awake() {
        transform.localScale = Vector3.zero;
        base.Awake();
    }

    public override void Play(Vector3 direction) {
        RotateFollowDirection(direction);
        transform.localScale = Vector3.one;
        rb.AddForce(direction.normalized * speed, ForceMode2D.Impulse);
        base.Play(direction);
        Invoke(nameof(Destroy), lifeDuration);
    }

    bool IsPlayer(GameObject col) {
        return LayerMask.LayerToName(col.layer) == Helper.Layer.Player.ToString();
    }

    void Destroy() {
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut() {
        float elapsedTime = 0f;
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        while (elapsedTime < fadeDuration) {
            float alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            foreach (SpriteRenderer r in renderers) {
                r.color = new Color(
                    r.color.r,
                    r.color.g,
                    r.color.b,
                    alpha
                );
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (IsPlayer(collider.gameObject)) {
            PlayerMovement movement = collider.gameObject.GetComponent<PlayerMovement>();
            if (movement) {
                movement.AddSpeedRatio(effectSpeed, null);
            }
        }
    }

    void OnTriggerExit2D(Collider2D collider) {
        if (IsPlayer(collider.gameObject)) {
            PlayerMovement movement = collider.gameObject.GetComponent<PlayerMovement>();
            if (movement) {
                movement.RemoveSpeedRatio(effectSpeed);
            }
        }
    }
}
