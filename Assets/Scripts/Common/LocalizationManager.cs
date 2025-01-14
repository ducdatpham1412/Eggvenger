using System.Collections;
using UnityEngine.Localization.Settings;



public class LocalizationManager : Singleton<LocalizationManager> {
    protected LocalizationManager() { }

    public enum Table {
        Home,
        Welcome,
        Game,
        Error,
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

    public string GetLocale() {
        // return LocalizationSettings.SelectedLocale == 0 ? "en" : "vi";
        return "en";
    }
}
