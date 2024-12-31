using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditProfileLogic : MonoBehaviour {
    [Serializable]
    public class State {
        public string avatar;
        public string name;
    }

    public TMP_InputField input;
    public GameObject AvatarPrefab;
    public Transform AvatarsCont;

    public RectTransform Content;
    public ButtonManager buttonManager;

    private State state;
    private AppState appState;
    private List<GameObject> avatars = new List<GameObject>();


    void Start() {
        LayoutRebuilder.ForceRebuildLayoutImmediate(Content);

        appState = GameManager.Instance.appState;

        input.text = appState.profile.name;

        foreach (string bg in appState.resource.backgrounds) {
            GameObject newAvatar = Instantiate(AvatarPrefab, Vector3.zero, Quaternion.identity, AvatarsCont);

            ImageLoader imgLoader = Helper.FindChildRecursive(newAvatar.transform, "Avatar").GetComponent<ImageLoader>();
            imgLoader.SetImageUrl(bg);

            Button avtButton = newAvatar.AddComponent<Button>();
            avtButton.onClick.AddListener(() => {
                ChangeAvatar(bg);
            });

            avatars.Add(newAvatar);
        };

        state = new State {
            name = appState.profile.name,
            avatar = appState.profile.avatar,
        };

        UpdateFocusAvatar();
    }


    private void UpdateFocusAvatar() {
        foreach (GameObject avt in avatars) {
            string imgUrl = Helper.FindChildRecursive(avt.transform, "Avatar").GetComponent<ImageLoader>().ImageUrl;
            Transform check = Helper.FindChildRecursive(avt.transform, "Check");
            check.gameObject.SetActive(imgUrl == state.avatar);
        }
    }


    private void ChangeAvatar(string url) {
        if (state != null) {
            state.avatar = url;
            UpdateFocusAvatar();
        }
    }

    public void OnChangeName(string newName) {
        if (state != null) {
            state.name = newName;
        }
    }

    public async void Save() {
        buttonManager.StartLoading();
        appState.profile.avatar = state.avatar;
        appState.profile.name = state.name;
        GameManager.Instance.UpdateAppState(appState);
        await Task.Delay(3000);
        buttonManager.StopLoading();
        GoBack();
    }

    public void GoBack() {
        if (Navigator.Instance.CanGoBack()) {
            Navigator.Instance.GoBack();
        }
        else {
            Navigator.Instance.NavigateTo(Navigator.Scene.Profile);
        }
    }


    public void Test() {
        Debug.Log("Test hello");
    }
}
