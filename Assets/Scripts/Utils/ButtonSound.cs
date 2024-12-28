using UnityEngine;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour {
    public Texture IconSound;
    public Texture IconSoundMute;

    private RawImage iconSoundImg;
    private bool playing = true;


    void Awake() {
        Transform iconSound = transform.Find("IconSound");
        if (iconSound != null) {
            iconSoundImg = iconSound.GetComponent<RawImage>();
        }

        Button button = GetComponent<Button>();
        if (button != null) {
            button.onClick.AddListener(ChangePlaying);
        }
    }

    public void SetPlaying(bool value) {
        playing = value;
        iconSoundImg.texture = playing ? IconSound : IconSoundMute;
    }

    public void ChangePlaying() {
        SetPlaying(!playing);
    }
}
