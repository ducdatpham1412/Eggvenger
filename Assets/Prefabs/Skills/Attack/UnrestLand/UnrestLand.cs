using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class UnrestLand : BaseSkill {
    Rigidbody2D rb;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        effect.speed = 0.6f;
        effect.lifeDuration = 7f;
        effect.fadeDuration = 1f;
    }

    public override async void Play(Vector3 pos, Vector3 direction) {
        gameObject.SetActive(true);
        transform.position = pos;
        float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, -angle);
        rb.AddForce(direction.normalized * speed, ForceMode2D.Force);
        await Task.Delay((int)(effect.lifeDuration * 1000));
        StartCoroutine(FadeOut());
    }

    bool IsPlayer(GameObject col) {
        return LayerMask.LayerToName(col.layer) == Helper.Layer.Player.ToString();
    }

    IEnumerator FadeOut() {
        float elapsedTime = 0f;
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        while (elapsedTime < effect.fadeDuration) {
            float alpha = Mathf.Lerp(1, 0, elapsedTime / effect.fadeDuration);
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

        gameObject.SetActive(false);
        transform.localPosition = Vector3.zero;
        foreach (SpriteRenderer r in renderers) {
            r.color = new Color(
                r.color.r,
                r.color.g,
                r.color.b,
                1
            );
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collider) {
        if (IsPlayer(collider.gameObject)) {
            PlayerMovement movement = collider.gameObject.GetComponent<PlayerMovement>();
            if (movement) {
                movement.AddSpeedRatio(effect.speed, null);
            }
        }
    }

    void OnTriggerExit2D(Collider2D collider) {
        if (IsPlayer(collider.gameObject)) {
            PlayerMovement movement = collider.gameObject.GetComponent<PlayerMovement>();
            if (movement) {
                movement.RemoveSpeedRatio(effect.speed);
            }
        }
    }
}
