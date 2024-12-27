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
        if (CreateAccountError.activeInHierarchy) {
            CreateAccountError.SetActive(false);
        }
    }

    public void onChangePassword(string value) {
        password = value;
    }

    private IEnumerator PerformCreateAccount() {
        yield return new WaitForSeconds(5f);
        buttonManager.StopLoading();
        welcomeLogic.ShowCanvas(CanvasName.EnterName.ToString());
    }


    public void CreateAccount() {
        Debug.Log($"Create account\nUsername: {username}\nPassword: {password}");
        if (username.Length < 8) {
            CreateAccountError.SetActive(true);
            return;
        }
        buttonManager.StartLoading();
        StartCoroutine(PerformCreateAccount());
    }
}
