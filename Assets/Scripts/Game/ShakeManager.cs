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
}
