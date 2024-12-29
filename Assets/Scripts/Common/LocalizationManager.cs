using System.Collections;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;



public class LocalizationManager : Singleton<LocalizationManager> {
    protected LocalizationManager() { }

    public enum Table {
        Home,
        Welcome,
    }

    private bool active = false;


    public void SetLocale(int localeID) {
        if (active) {
            return;
        }
        IEnumerator _SetLocale(int localeID) {
            active = true;
            yield return LocalizationSettings.InitializationOperation;
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeID];
            active = false;
        }
        StartCoroutine(_SetLocale(localeID));
    }
}
