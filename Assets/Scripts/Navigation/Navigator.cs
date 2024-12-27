using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Navigator : Singleton<Navigator> {
    public enum Scene {
        Welcome,
        Home,
        GameOverview,
        ThrowEgg,
    }
    private List<string> history = new List<string>();

    public void NavigateTo(Scene scene) {
        string temp = scene.ToString();
        history.Add(temp);
        SceneManager.LoadScene(temp);
    }
}
