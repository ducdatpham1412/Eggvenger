using System.Collections;
using UnityEngine;

public class LoginLogic : MonoBehaviour {
    private string username = "";
    private string password = "";
    public ButtonManager buttonManager;

    public void onChangeUsername(string value) {
        username = value;
    }

    public void onChangePassword(string value) {
        password = value;
    }

    private IEnumerator PerformLogin() {
        yield return new WaitForSeconds(2f);
        buttonManager.StopLoading();
    }

    public void Login() {
        buttonManager.StartLoading();
        StartCoroutine(PerformLogin());
        Debug.Log($"Start login\nUsername: {username}\nPassword: {password}");
    }
}
