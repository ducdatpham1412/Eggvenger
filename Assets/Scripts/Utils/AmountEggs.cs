using UnityEngine;
using UnityEngine.UI;

public class AmountEggs : MonoBehaviour {
    private Text text;


    private void UpdateProfile(Profile newState) {
        if (text != null) {
            text.text = newState.eggs.ToString();
        }
    }

    void Start() {
        text = GetComponent<Text>();
        text.text = GameManager.Instance.ProfileState.eggs.ToString();
        GameManager.Instance.ListenProfileChanged(UpdateProfile);
    }


    void OnDestroy() {
        GameManager.Instance.RemoveListenProfileChanged(UpdateProfile);
    }
}
