using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json.Linq;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;



public class MatchManager : NetworkBehaviour {
    public RectTransform Content;
    public GameObject CardPrefab;
    public GameObject Line;
    public GameObject FinalLeaderBoard;
    public GameObject BackToLobby;
    string READY_STATUS = MatchState.Player.Status.ready.ToString();
    bool hasLoadedMatch = false;
    bool ended = false;

    AppState appState;
    GameState gameState;
    SynchronizationContext context;
    List<RoomCardManager> roomManagers = new List<RoomCardManager>();

    void Start() {
        appState = GameManager.Instance.appState;
        gameState = GameManager.Instance.gameState;
        context = SynchronizationContext.Current;
        InitGame();
    }

    public override void OnNetworkSpawn() {
        if (IsClient) {
            // We have to check like on Start for comeback from room to match, OnNetworkSpawn will run before Start
            if (appState == null) {
                appState = GameManager.Instance.appState;
                gameState = GameManager.Instance.gameState;
                context = SynchronizationContext.Current;
            }
            LoadMatchServerRpc(appState.profile.id);
            return;
        }
    }

    public override void OnNetworkDespawn() {
        if (IsServer) {
            SocketManager.OnHandleData -= S_LoadMatch;
        }
    }

    void S_LoadMatch(JObject evt) {
        string eventType = evt["type"].ToString();
        if (eventType == "load_match") {
            gameState.status = GameState.Status.inGame;
            gameState.matchState = evt["data"].ToObject<MatchState>();
            context.Post(_ => {
                DisplayCards();
            }, null);
        }
    }

    void DisplayCards() {
        if (ended) {
            return;
        }

        if (!hasLoadedMatch) {
            hasLoadedMatch = true;
            for (int i = 0; i < gameState.matchState.rooms.Count; i++) {
                JObject room = gameState.matchState.rooms[i];
                string roomType = room["type"].ToString();
                GameObject NewCard = Instantiate(CardPrefab);
                NewCard.transform.SetParent(Content, false);

                if (roomType == BaseRoom.Type.throw_egg.ToString()) {
                    RoomCardManager manager = NewCard.GetComponent<RoomCardManager>();
                    manager.Initialize(room["id"].ToString());
                    roomManagers.Add(manager);
                }
            }
        }
        if (IsClient) {
            CheckContinueOrEndGame();
        }
    }

    private void S_InitServer(string match_id) {
        SocketManager.OnHandleData += S_LoadMatch;
        NetworkManager.Singleton.StartServer();
        SocketManager.Send(new Event.Send.LoadMatch { match_id = match_id });
    }

    [ServerRpc(RequireOwnership = false)]
    private void LoadMatchServerRpc(string playerID, ServerRpcParams rpcParams = default) {
        foreach (MatchState.Player player in gameState.matchState.players) {
            if (player.id == playerID) {
                player.status = READY_STATUS;
                player.clientID = (int)rpcParams.Receive.SenderClientId;
            }
        }
        PassMatchStateClientRpc(gameState.matchState);
        CheckContinueOrEndGame();
    }

    [ClientRpc]
    void PassMatchStateClientRpc(MatchState state) {
        gameState.matchState = state;
        context.Post(_ => {
            DisplayCards();
        }, null);
    }

    void CheckContinueOrEndGame() {
        bool allReady = gameState.matchState.players.Find(p => p.status != READY_STATUS) == null;
        if (
            allReady
            // 1 == 1 // TODO: Remove this, only for testing
            ) {
            JObject room = gameState.matchState.rooms.Find(r => r["status"].ToString() == BaseRoom.Status.active.ToString());

            // Case 1: Still having active room in mathState => continue
            if (room != null) {
                string roomID = room["id"].ToString();
                RoomCardManager roomCard = roomManagers.Find(r => r.roomID == roomID);
                roomCard.CountDownToBegin();
                return;
            }


            // Case 2: All rooms end => Endgame
            ended = true;
            EndGame();
        }
    }

    void InitClient() {
        string IP = appState.client.match_ip;
        int? PORT = appState.client.match_port;
        if (IP != "" && PORT != null) {
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null) {
                transport.SetConnectionData(IP, (ushort)PORT);
                NetworkManager.Singleton.StartClient();
            }
        }
    }

    void InitGame() {
        if (!NetworkManager.Singleton.IsListening) {
            // string match_id = Environment.GetEnvironmentVariable("match_id");

            // if (match_id != null) {
            //     InitServer(match_id);
            //     return;
            // }

            // InitClient();

            // TODO: Remove this
            TestCreate.gameObject.SetActive(true);
            return;
        }
        else {
            TestCreate.gameObject.SetActive(false);
            if (IsServer) {
                DisplayCards();
            }
        }
    }

    void EndGame() {
        if (IsClient) {
            NetworkManager.Singleton.Shutdown();
            GameObject LineObject = Instantiate(Line);
            GameObject Final = Instantiate(FinalLeaderBoard);
            LineObject.transform.SetParent(Content, false);
            Final.transform.SetParent(Content, false);
            Final.GetComponent<FinalLeaderboard>().SetData(gameState.matchState);
            BackToLobby.transform.SetAsLastSibling();
            BackToLobby.SetActive(true);
            return;
        }

        if (IsServer) {
            // TODO: Send data to UDP server to save
            return;
        }
    }

    public void GoBackLobby() {
        if (NetworkManager.Singleton.IsClient) {
            Navigator.Instance.NavigateTo(Navigator.Scene.MatchMaking);
        }
    }

    // This is only for testing
    public GameObject TestCreate;

    private string player_client_id;
    public void ChangePlayerClientID(string value) {
        player_client_id = value;
    }
    public void SubmitStartClient() {
        TestCreate.SetActive(false);
        appState.profile.id = player_client_id;
        NetworkManager.Singleton.StartClient();
    }

    private string match_id;
    public void ChangeMatchID(string value) {
        match_id = value;
    }
    public void SubmitStartServer() {
        TestCreate.SetActive(false);
        S_InitServer(match_id);
    }
}
