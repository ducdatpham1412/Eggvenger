using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour {
    public GameObject Title;
    public GameObject Loading;
    private Button button;


    void Start() {
        button = gameObject.GetComponent<Button>();
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


    public void StartLoading() {
        Disable();
        if (Loading != null && Title != null) {
            Title.SetActive(false);
            Loading.SetActive(true);
        }
    }
    public void StopLoading() {
        Enable();
        if (Loading != null && Title != null) {
            Title.SetActive(true);
            Loading.SetActive(false);
        }
    }
}
