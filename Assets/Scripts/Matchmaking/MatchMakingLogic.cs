using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MatchMakingLogic : MonoBehaviour {
    public GameObject TeamView;
    public GameObject StopFindButton;
    public GameObject ItemPlayerFormation;
    public FindMatchButton FindButton;

    float AnimatedUpDistance = 50f;

    void Start() {
        Profile profile = GameManager.Instance.appState.profile;
        if (ItemPlayerFormation != null && profile != null) {
            Text name = ItemPlayerFormation.transform.Find("Name").GetComponent<Text>();
            Transform avatar = Helper.FindChildRecursive(ItemPlayerFormation.transform, "Avatar");
            name.text = profile.name;
            Helper.SetImageUrl(avatar, profile.avatar);
        }

        GameState gameState = GameManager.Instance.gameState;
        if (gameState.status == GameState.Status.findingMatch) {
            TeamView.transform.localPosition = TeamView.transform.localPosition + new Vector3(0, AnimatedUpDistance, 0);
            StopFindButton.SetActive(true);
        }

        SocketManager.OnHandleData += MatchFound;
    }


    void OnDestroy() {
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
            Navigator.Instance.NavigateTo(Navigator.Scene.MatchScene);
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

    public void GoBack() {
        Navigator.Instance.NavigateTo(Navigator.Scene.Home);
    }


    public void FindMatch() {
        if (GameManager.Instance.gameState.status == GameState.Status.findingMatch) {
            return;
        }
        StartCoroutine(AnimateLinearUp(TeamView.transform, AnimatedUpDistance, 0.25f, true));
        FindButton.FindMatch();
    }


    public void StopFinding() {
        StartCoroutine(AnimateLinearUp(TeamView.transform, -AnimatedUpDistance, 0.25f, false));
        FindButton.StopFinding();
    }
}
