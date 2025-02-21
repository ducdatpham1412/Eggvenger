using System.Collections;
using UnityEngine;

public class PlayerVFX : MonoBehaviour {
    Material material;
    [SerializeField] Sprite NullSkin;
    Coroutine Skin01Coroutine;
    Coroutine Effect01Coroutine;

    void Start() {
        material = GetComponent<SpriteRenderer>().material;
    }

    /*
    Handle Skins
    */
    public void SetSkin(Sprite sprite, float duration) {
        if (Skin01Coroutine != null) {
            StopCoroutine(Skin01Coroutine);
        }
        Skin01Coroutine = StartCoroutine(_SetSkin(sprite, duration));
    }
    IEnumerator _SetSkin(Sprite sprite, float duration) {
        material.SetTexture("_Skin01", sprite.texture);
        float elapsedTime = 0f;
        while (elapsedTime < duration) {
            float intensity = Mathf.Lerp(0f, 0.7f, elapsedTime / duration);
            material.SetFloat("_Skin01Intensity", intensity);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        material.SetFloat("_Skin01Intensity", 0.7f);
        Skin01Coroutine = null;
    }

    public void ResetSkin(Sprite sprite, float duration) {
        if (Skin01Coroutine != null) {
            StopCoroutine(Skin01Coroutine);
        }
        Skin01Coroutine = StartCoroutine(_ResetSkin(sprite, duration));
    }
    IEnumerator _ResetSkin(Sprite sprite, float duration) {
        float elapsedTime = 0f;
        float currentIntensity = material.GetFloat("_Skin01Intensity");
        while (elapsedTime < duration) {
            float intensity = Mathf.Lerp(currentIntensity, 0f, elapsedTime / duration);
            material.SetFloat("_Skin01Intensity", intensity);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        material.SetFloat("_Skin01Intensity", 0);
        material.SetTexture("_Skin01", sprite.texture);
        Skin01Coroutine = null;
    }


    /*
    Handle Effects
    */
    public void SetEffect(Sprite sprite, float duration) {
        if (Effect01Coroutine != null) {
            StopCoroutine(Effect01Coroutine);
        }
        Effect01Coroutine = StartCoroutine(_SetEffect(sprite, duration));
    }
    IEnumerator _SetEffect(Sprite sprite, float duration) {
        material.SetTexture("_Effect01", sprite.texture);
        float elapsedTime = 0f;
        while (elapsedTime < duration) {
            float intensity = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            material.SetFloat("_Effect01Intensity", intensity);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        material.SetFloat("_Effect01Intensity", 1f);
        Effect01Coroutine = null;
    }


    public void ResetEffect(Sprite sprite, float duration) {
        if (Effect01Coroutine != null) {
            StopCoroutine(Effect01Coroutine);
        }
        Effect01Coroutine = StartCoroutine(_ResetEffect(sprite, duration));
    }
    IEnumerator _ResetEffect(Sprite sprite, float duration) {
        float elapsedTime = 0f;
        float currentIntensity = material.GetFloat("_Effect01Intensity");
        while (elapsedTime < duration) {
            float intensity = Mathf.Lerp(currentIntensity, 0f, elapsedTime / duration);
            material.SetFloat("_Effect01Intensity", intensity);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        material.SetFloat("_Effect01Intensity", 0);
        material.SetTexture("_Effect01", sprite.texture);
        Effect01Coroutine = null;
    }
}
