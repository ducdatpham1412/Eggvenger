using System.Collections;
using UnityEngine;

public class ShakeManager : MonoBehaviour {
    Vector3 originalLocalPosition;
    Coroutine shakeCoroutine;

    IEnumerator Shake() {
        originalLocalPosition = transform.localPosition;
        while (true) {
            float shakeAmountX = Mathf.Sin(Time.time * 70f) * 0.02f;
            float shakeAmountY = Mathf.Cos(Time.time * 70f) * 0.02f;
            transform.localPosition = originalLocalPosition + new Vector3(shakeAmountX, shakeAmountY, 0);
            yield return null;
        }
    }

    IEnumerator Scale(float duration, float scale) {
        RectTransform uiElement = GetComponent<RectTransform>();
        Vector3 originalScale = uiElement.localScale;
        Vector3 targetScale = originalScale * scale;

        float elapsedTime = 0f;
        while (elapsedTime < duration) {
            uiElement.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < duration) {
            uiElement.localScale = Vector3.Lerp(targetScale, originalScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        uiElement.localScale = originalScale;
    }

    public void StartShake() {
        shakeCoroutine = StartCoroutine(Shake());
    }

    public void StopShake() {
        if (shakeCoroutine != null) {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
            transform.localPosition = originalLocalPosition;
        }
    }

    public void StartScale(float duration = 0.15f, float scale = 1.5f) {
        StartCoroutine(Scale(duration, scale));
    }
}
