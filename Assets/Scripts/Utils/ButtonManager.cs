using System.Collections;
using LottiePlugin.UI;
using UnityEngine;
using UnityEngine.UI;


public enum AudioClick {
    KnockWood,
    None,
}

public class ButtonManager : MonoBehaviour {
    public GameObject Title;
    public GameObject Loading;
    private Button button;
    public AudioClick audioClick = AudioClick.KnockWood;
    private AnimatedImage animatedImage;


    void Awake() {
        button = gameObject.GetComponent<Button>();
        if (audioClick != AudioClick.None) {
            button.onClick.AddListener(PlaySound);
        }

        if (Loading != null) {
            animatedImage = Loading.GetComponent<AnimatedImage>();
        }
    }

    private void PlaySound() {
        if (audioClick == AudioClick.KnockWood) {
            SoundManager.Instance.PlayKnowWood();
        }
    }

    public void Disable() {
        if (button != null) {
            button.interactable = false;
        }
    }

    public void Enable() {
        if (button != null) {
            button.interactable = true;
        }
    }

    private IEnumerator WaitAndPlay() {
        yield return new WaitUntil(() => animatedImage.isActiveAndEnabled);
        animatedImage.Play();
    }


    public void StartLoading() {
        Disable();
        if (Loading != null && Title != null) {
            Title.SetActive(false);
            Loading.SetActive(true);
            StartCoroutine(WaitAndPlay());
        }
    }
    public void StopLoading() {
        Enable();
        if (Loading != null && Title != null) {
            Title.SetActive(true);
            Loading.SetActive(false);
            animatedImage.Stop();
        }
    }
}
