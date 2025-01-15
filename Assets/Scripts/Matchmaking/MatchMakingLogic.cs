using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class MatchMakingLogic : MonoBehaviour {
    public GameObject TeamView;
    public LocalizeStringEvent ButtonTitle;
    public GameObject StopFindButton;
    public GameObject ItemPlayerFormation;

    private float AnimatedUpDistance = 50f;



    void Start() {
        Profile profile = GameManager.Instance.appState.profile;
        if (ItemPlayerFormation != null && profile != null) {
            Text name = ItemPlayerFormation.transform.Find("Name").GetComponent<Text>();
            Transform avatar = Helper.FindChildRecursive(ItemPlayerFormation.transform, "Avatar");
            name.text = profile.name;
            Helper.SetImageUrl(avatar, profile.avatar);
        }

        GameState gameState = GameManager.Instance.gameState;
        if (gameState != null && gameState.status == GameState.Status.findingMatch) {
            ButtonTitle.StringReference.SetReference(LocalizationManager.Table.Home.ToString(), "finding");
            UpdateTextCountUp(gameState.data.secondsElapsed);
            GameManager.Instance.OnGameStateChanged += ListenCountUp;
            TeamView.transform.localPosition = TeamView.transform.localPosition + new Vector3(0, AnimatedUpDistance, 0);
            StopFindButton.SetActive(true);
        }

        SocketManager.OnHandleData += MatchFound;
    }


    void OnDestroy() {
        GameManager.Instance.OnGameStateChanged -= ListenCountUp;
        SocketManager.OnHandleData -= MatchFound;
    }


    private void MatchFound(JObject evt) {
        if (evt["type"]?.ToString() == "match_found") {
            MatchState match = JsonConvert.DeserializeObject<MatchState>(evt["data"].ToString());
            GameManager.Instance.UpdateAppState(state => {
                state.client.match_ip = match.configs.ip;
                state.client.match_port = match.configs.port;
                return state;
            });
            Navigator.Instance.NavigateTo(Navigator.Scene.GameThrowEgg);
        }
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
        GameManager.Instance.OnGameStateChanged += ListenCountUp;
        ButtonTitle.StringReference.SetReference(LocalizationManager.Table.Home.ToString(), "finding");
        StartCoroutine(AnimateLinearUp(TeamView.transform, AnimatedUpDistance, 0.25f, true));
        StopFindButton.SetActive(true);
        GameManager.Instance.StartCountUp();
        SocketManager.Send(new Event.Send.FindMatch());
    }


    public void StopFinding() {
        GameManager.Instance.OnGameStateChanged -= ListenCountUp;
        ButtonTitle.StringReference.SetReference(LocalizationManager.Table.Home.ToString(), "findMatch");
        ButtonTitle.RefreshString();
        GameManager.Instance.StopCountUp();
        StartCoroutine(AnimateLinearUp(TeamView.transform, -AnimatedUpDistance, 0.25f, false));
        SocketManager.Send(new Event.Send.StopFindMatch());
    }
}
