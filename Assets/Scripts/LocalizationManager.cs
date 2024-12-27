using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : Singleton<LocalizationManager> {
    protected LocalizationManager() { }
    private Dictionary<string, string> localizedData;
    private string currentLanguage = "en";
    public delegate void LanguageChanged();
    public event LanguageChanged OnLanguageChanged;



    private void Awake() {
        LoadLocalizationData();

    }


    private void LoadLocalizationData() {
        string resourcePath = $"i18n/{currentLanguage}";
        TextAsset jsonDataAsset = Resources.Load<TextAsset>(resourcePath);
        if (jsonDataAsset != null) {
            string jsonData = jsonDataAsset.text;
            Debug.Log($"Json data: {jsonData}");
            localizedData = JsonUtility.FromJson<Wrapper>(jsonData).ToDictionary();
            Debug.Log($"Localized data length: {localizedData.Keys.Count}");
            foreach (var item in localizedData) {
                Debug.Log("Check: " + item.Key + ": " + item.Value);
            }

        }
        else {
            Debug.LogError($"Localization file not found in Resources: {resourcePath}.json");
            localizedData = new Dictionary<string, string>();
        }
    }


    public string GetLocalizedValue(string key) {
        Debug.Log($"Key is: {key} - {localizedData["selectLanguage"]}");
        if (localizedData != null && localizedData.ContainsKey(key)) {
            return localizedData[key];
        }
        return key;
    }


    public void SetLanguage(string language) {
        Debug.Log($"Language: {language}");
        currentLanguage = language;
        LoadLocalizationData();
        OnLanguageChanged?.Invoke();
    }

    public void Check() {
        Debug.Log("Check log language");
    }


    [System.Serializable]
    private class Wrapper {
        public List<KeyValuePair> items;

        public Dictionary<string, string> ToDictionary() {
            Debug.Log("Check items: " + items + "Count: " + items.Count);
            var dictionary = new Dictionary<string, string>();
            foreach (var kvp in items) {
                dictionary[kvp.key] = kvp.value;
                Debug.Log($"Key: {kvp.key}, Value: {kvp.value}");
            }
            return dictionary;
        }
    }

    [System.Serializable]
    private class KeyValuePair {
        public string key;
        public string value;
    }
}
