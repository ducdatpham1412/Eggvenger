using UnityEngine;
using UnityEngine.UI;

public class ProfileLogic : MonoBehaviour {
    public GameObject SettingDialog;
    public Transform Avatar;
    public Text Name;


    void Start() {
        Profile profile = GameManager.Instance.appState.profile;
        if (Avatar != null) {
            Helper.SetImageUrl(Avatar, profile.avatar);
        }
        if (Name != null) {
            Name.text = profile.name;
        }
    }

    public void GoBack() {
        if (Navigator.Instance.CanGoBack()) {
            Navigator.Instance.GoBack();
        }
        else {
            Navigator.Instance.NavigateTo(Navigator.Scene.Home);
        }
    }

    public void ShowHideSetting() {
        if (SettingDialog != null) {
            SettingDialog.SetActive(!SettingDialog.activeInHierarchy);
        }
    }


    public void EditProfile() {
        Navigator.Instance.NavigateTo(Navigator.Scene.ProfileEdit);
    }

    public void AccountInfo() {
        Navigator.Instance.NavigateTo(Navigator.Scene.AccountInfo);
    }

    public void LogOut() {
        Navigator.Instance.NavigateTo(Navigator.Scene.Welcome);
    }
}
