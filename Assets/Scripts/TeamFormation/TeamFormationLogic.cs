using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class TeamFormationLogic : MonoBehaviour {
    public GameObject TeamView;
    public LocalizeStringEvent ButtonTitle;
    public GameObject StopFindButton;
    public GameObject ItemPlayerFormation;

    private float AnimatedUpDistance = 50f;



    void Start() {
        Profile profile = GameManager.Instance.profileState;
        if (ItemPlayerFormation != null && profile != null) {
            ImageLoader avatarLoader = Helper.FindChildRecursive(ItemPlayerFormation.transform, "Avatar").GetComponent<ImageLoader>();
            Text name = ItemPlayerFormation.transform.Find("Name").GetComponent<Text>();
            name.text = profile.name;
            avatarLoader.SetImageUrl(profile.avatar);
        }

        GameState gameState = GameManager.Instance.gameState;
        if (gameState != null && gameState.status == GameState.Status.findingMatch) {
            ButtonTitle.StringReference.SetReference(LocalizationManager.Table.Home.ToString(), "finding");
            UpdateTextCountUp(((FindingMatchState)gameState.data).secondsElapsed);
            GameManager.Instance.ListenGameStateChanged(ListenCountUp);
            TeamView.transform.localPosition = TeamView.transform.localPosition + new Vector3(0, AnimatedUpDistance, 0);
            StopFindButton.SetActive(true);
        }
    }


    void OnDestroy() {
        GameManager.Instance.RemoveListenGameStateChanged(ListenCountUp);
    }

    private IEnumerator AnimateLinearUp(Transform target, float distance, float duration, bool active) {
        if (!active) {
            StopFindButton.SetActive(false);
        }

        Vector3 startPos = target.localPosition;
        Vector3 endPost = startPos + new Vector3(0, distance, 0);
        float elapsedTime = 0f;
        while (elapsedTime < duration) {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            target.localPosition = Vector3.Lerp(startPos, endPost, t);
            yield return null;
        }
        target.localPosition = endPost;

        if (active) {
            StopFindButton.SetActive(true);
        }
    }


    private void UpdateTextCountUp(int secondsElapsed) {
        int minutes = secondsElapsed / 60;
        int seconds = secondsElapsed % 60;
        string formattedTime = $"{minutes:00}:{seconds:00}";
        ButtonTitle.StringReference.Arguments = new object[] { formattedTime };
        ButtonTitle.RefreshString();
    }
    private void ListenCountUp(GameState newState) {
        FindingMatchState data = (FindingMatchState)newState.data;
        UpdateTextCountUp(data.secondsElapsed);
    }

    public void GoBack() {
        if (Navigator.Instance.CanGoBack()) {
            Navigator.Instance.GoBack();
        }
        else {
            Navigator.Instance.NavigateTo(Navigator.Scene.Home);
        }
    }


    public void FindMatch() {
        GameManager.Instance.ListenGameStateChanged(ListenCountUp);
        ButtonTitle.StringReference.SetReference(LocalizationManager.Table.Home.ToString(), "finding");
        StartCoroutine(AnimateLinearUp(TeamView.transform, AnimatedUpDistance, 0.25f, true));
        StopFindButton.SetActive(true);
        GameManager.Instance.StartCountUp();
    }


    public void StopFinding() {
        GameManager.Instance.RemoveListenGameStateChanged(ListenCountUp);
        ButtonTitle.StringReference.SetReference(LocalizationManager.Table.Home.ToString(), "findMatch");
        ButtonTitle.RefreshString();
        GameManager.Instance.StopCountUp();
        StartCoroutine(AnimateLinearUp(TeamView.transform, -AnimatedUpDistance, 0.25f, false));
    }
}
