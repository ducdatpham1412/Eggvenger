using System.Collections.Generic;
using UnityEngine;


public enum CanvasName {
    SelectLanguage,
    LoginCreateAccount,
    Login,
    CreateAccount,
    EnterName,
}


[System.Serializable]
public class CanvasEntry {
    public CanvasName name;
    public GameObject gameObject;
}


public class WelcomeLogic : MonoBehaviour {
    public List<CanvasEntry> canvases = new List<CanvasEntry>();
    private List<string> histories = new List<string>();
    // private CanvasName currentCanvas;
    public GameObject CreateAccountError;
    private bool shouldAddNewCanvas = true;


    void Start() {
        ShowCanvas(CanvasName.SelectLanguage.ToString());
    }


    public void ShowCanvas(string canvasName) {
        if (shouldAddNewCanvas) {
            histories.Add(canvasName);
        }
        else {
            shouldAddNewCanvas = true;
        }
        foreach (CanvasEntry cv in canvases) {
            if (cv.name.ToString() == canvasName) {
                cv.gameObject.SetActive(true);
                // currentCanvas = cv.name;
            }
            else {
                cv.gameObject.SetActive(false);
            }
        }
    }


    public void GoBack() {
        if (histories.Count >= 2) {
            shouldAddNewCanvas = false;
            histories.RemoveAt(histories.Count - 1);
            ShowCanvas(histories[histories.Count - 1]);
        }
    }
}
