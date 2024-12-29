using System.Collections;
using UnityEngine;

public class CreateAccountLogic : MonoBehaviour {
    private string username = "";
    private string password = "";
    public GameObject CreateAccountError;
    public ButtonManager buttonManager;
    public WelcomeLogic welcomeLogic;


    public void onChangeUsername(string value) {
        username = value;
        if (CreateAccountError != null && CreateAccountError.activeInHierarchy) {
            CreateAccountError.SetActive(false);
        }
    }

    public void onChangePassword(string value) {
        password = value;
    }

    public void onChangePlayerName(string value) {
        username = value;
    }

    private IEnumerator PerformCreateAccount() {
        yield return new WaitForSeconds(3f);
        buttonManager.StopLoading();
        welcomeLogic.ShowCanvas(CanvasName.EnterName.ToString());
    }

    private IEnumerator PerformChangePlayerName() {
        yield return new WaitForSeconds(3f);
        buttonManager.StopLoading();
        Navigator.Instance.NavigateTo(Navigator.Scene.Home);
    }

    public void CreateAccount() {
        Debug.Log($"Create account\nUsername: {username}\nPassword: {password}");
        if (username.Length < 8) {
            if (CreateAccountError != null) {
                CreateAccountError.SetActive(true);
            }
            return;
        }
        buttonManager.StartLoading();
        StartCoroutine(PerformCreateAccount());
    }

    public void ChangePlayerName() {
        Debug.Log($"Change player's name: {username}");
        buttonManager.StartLoading();
        StartCoroutine(PerformChangePlayerName());
    }
}
