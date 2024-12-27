using UnityEngine;
using TMPro;

public class LocalizedText : MonoBehaviour {
    public string key;
    private TMP_Text textCpn;


    private void Start() {
        textCpn = GetComponent<TMP_Text>();
        UpdateText();
        LocalizationManager.Instance.OnLanguageChanged += UpdateText;
    }


    private void OnDestroy() {
        LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
    }


    private void UpdateText() {
        textCpn.text = LocalizationManager.Instance.GetLocalizedValue(key);
    }
}
