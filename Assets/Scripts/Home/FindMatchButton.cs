using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Localization.Components;

public class FindMatchButton : MonoBehaviour {
    public LocalizeStringEvent ButtonTitle;
    SynchronizationContext context;

    void Start() {
        context = SynchronizationContext.Current;
        GameState gameState = GameManager.Instance.gameState;
        if (gameState.status == GameState.Status.findingMatch) {
            ButtonTitle.StringReference.SetReference(LocalizationManager.Table.Home.ToString(), "finding");
            UpdateTextCountUp(gameState.data.secondsElapsed);
            GameManager.Instance.OnGameStateChanged += ListenCountUp;
        }
        SocketManager.OnHandleData += MatchFound;
    }

    void OnDestroy() {
        GameManager.Instance.OnGameStateChanged -= ListenCountUp;
        SocketManager.OnHandleData -= MatchFound;
    }

    void ListenCountUp(GameState newState) {
        FindingMatchState data = newState.data;
        UpdateTextCountUp(data.secondsElapsed);
    }

    void UpdateTextCountUp(int secondsElapsed) {
        int minutes = secondsElapsed / 60;
        int seconds = secondsElapsed % 60;
        string formattedTime = $"{minutes:00}:{seconds:00}";
        ButtonTitle.StringReference.Arguments = new object[] { formattedTime };
        ButtonTitle.RefreshString();
    }

    void MatchFound(JObject evt) {
        if (evt["type"]?.ToString() == Event.Name.match_found.ToString()) {
            MatchState match = JsonConvert.DeserializeObject<MatchState>(evt["data"].ToString());
            context.Post(_ => {
                GameManager.Instance.StopCountUp();
                GameManager.Instance.UpdateAppState(state => {
                    state.client.match_ip = match.configs.ip;
                    state.client.match_port = match.configs.port;
                    return state;
                });
                GameManager.Instance.UpdateGameState(state => {
                    state.status = GameState.Status.inGame;
                    return state;
                });
                Navigator.Instance.NavigateTo(Navigator.Scene.MatchScene);
            }, null);
        }
    }

    public void FindMatch() {
        if (GameManager.Instance.gameState.status == GameState.Status.findingMatch) {
            return;
        }
        ButtonTitle.StringReference.SetReference(LocalizationManager.Table.Home.ToString(), "finding");
        GameManager.Instance.OnGameStateChanged += ListenCountUp;
        GameManager.Instance.StartCountUp();
        SocketManager.Send(new Event.Send.FindMatch());
    }

    public void StopFinding() {
        ButtonTitle.StringReference.SetReference(LocalizationManager.Table.Home.ToString(), "findMatch");
        GameManager.Instance.OnGameStateChanged -= ListenCountUp;
        GameManager.Instance.StopCountUp();
        SocketManager.Send(new Event.Send.StopFindMatch());
    }
}
