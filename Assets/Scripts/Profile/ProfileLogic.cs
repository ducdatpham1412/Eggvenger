using UnityEngine;
using UnityEngine.UI;

public class ProfileLogic : MonoBehaviour {
    public GameObject SettingDialog;
    public Transform Avatar;
    public Text Name;
    public Image EnglishButton;
    public Image VietnameseButton;

    private Color yellow;


    void Start() {
        Profile profile = GameManager.Instance.appState.profile;
        if (Avatar != null) {
            Helper.SetImageUrl(Avatar, profile.avatar);
        }
        if (Name != null) {
            Name.text = profile.name;
        }
        yellow = Helper.ColorFromHex(Configs.Color.yellow);

        EnglishButton.color = profile.localeID == Configs.LocaleID.En ? yellow : Color.white;
        VietnameseButton.color = profile.localeID == Configs.LocaleID.Vi ? yellow : Color.white;
    }

    public void GoBack() {
        Navigator.Instance.NavigateTo(Navigator.Scene.Home);
    }

    public void ShowHideSetting() {
        if (SettingDialog != null) {
            SettingDialog.SetActive(!SettingDialog.activeInHierarchy);
        }
    }


    public void EditProfile() {
        Navigator.Instance.Push(Navigator.Scene.ProfileEdit);
    }

    public void AccountInfo() {
        Navigator.Instance.Push(Navigator.Scene.AccountInfo);
    }

    public void LogOut() {
        Navigator.Instance.Push(Navigator.Scene.Welcome);
    }

    public void SetLocale(int localeID) {
        LocalizationManager.Instance.SetLocale(localeID);
        AppState appState = GameManager.Instance.UpdateAppState(appState => {
            appState.profile.localeID = localeID;
            return appState;
        });

        EnglishButton.color = appState.profile.localeID == Configs.LocaleID.En ? yellow : Color.white;
        VietnameseButton.color = appState.profile.localeID == Configs.LocaleID.Vi ? yellow : Color.white;
    }
}
