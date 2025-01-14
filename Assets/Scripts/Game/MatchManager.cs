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

    private AppState appState;
    private GameState gameState;
    private SynchronizationContext context;
    private List<RoomCardManager> roomManagers = new List<RoomCardManager>();

    void Start() {
        appState = GameManager.Instance.appState;
        gameState = GameManager.Instance.gameState;
        context = SynchronizationContext.Current;
        InitGame();
    }

    void Destroy() {
        SocketManager.OnHandleData -= LoadMatch;
    }

    public override void OnNetworkSpawn() {
        if (IsClient) {
            LoadMatchServerRpc(appState.profile.id);
            return;
        }
    }

    private void LoadMatch(JObject evt) {
        string eventType = evt["type"].ToString();
        if (eventType == "load_match") {
            gameState.status = GameState.Status.inGame;
            gameState.matchState = evt["data"].ToObject<MatchState>();
            context.Post(_ => {
                S_DisplayCards();
            }, null);
        }
    }

    private void S_DisplayCards() {
        for (int i = 0; i < gameState.matchState.rooms.Count; i++) {
            JObject room = gameState.matchState.rooms[i];
            string roomType = room["type"].ToString();
            GameObject NewCard = Instantiate(CardPrefab);
            NewCard.transform.SetParent(Content, false);

            if (roomType == BaseRoom.Type.throw_egg.ToString()) {
                RoomCardManager manager = NewCard.GetComponent<RoomCardManager>();
                RoomThrowEgg roomThrowEgg = room.ToObject<RoomThrowEgg>();
                roomThrowEgg.players[0].point = Random.Range(1, 9);
                roomThrowEgg.players[1].point = Random.Range(1, 9);
                gameState.matchState.rooms[i] = JObject.FromObject(roomThrowEgg);

                manager.Initialize(room["id"].ToString());
                roomManagers.Add(manager);
            }


        }

        EndGame();

        // TODO: Remove this, we begin in function ServerRPC: LoadMatchServerRpc
        // JObject _room = gameState.matchState.rooms.Find(r => r["status"].ToString() == "active");
        // if (_room != null) {
        //     int index = roomManagers.FindIndex(r => r.roomID == _room["id"].ToString());
        //     roomManagers[index].CountDownToBegin(index == 0 ? 3 : 6);
        // }
        // else {
        //     EndGame();
        // }
    }

    private void InitServer(string match_id) {
        SocketManager.OnHandleData += LoadMatch;
        SocketManager.Send(new Event.Send.LoadMatch { match_id = match_id });
        NetworkManager.Singleton.StartServer();
    }

    [ServerRpc(RequireOwnership = false)]
    private void LoadMatchServerRpc(string playerID, ServerRpcParams rpcParams = default) {
        foreach (MatchState.Player player in gameState.matchState.players) {
            player.clientID = (int)rpcParams.Receive.SenderClientId;
            if (player.id == playerID) {
                player.status = MatchState.Player.Status.ready.ToString();
            }
        }

        bool allReady = gameState.matchState.players.Find(p => p.status != MatchState.Player.Status.ready.ToString()) == null;

        PassMatchStateClientRpc(gameState.matchState);
        if (allReady) {
            JObject room = gameState.matchState.rooms.Find(r => r["status"].ToString() == BaseRoom.Status.active.ToString());
            // Case 1: Still having active room in mathState => continue
            if (room != null) {
                RoomCardManager roomCard = roomManagers.Find(r => r.roomID == room["id"].ToString());
                roomCard.CountDownToBegin();
                return;
            }

            // Case 2: All rooms end => End match and display Final Leaderboard
            EndGame();
        }
    }

    [ClientRpc]
    private void PassMatchStateClientRpc(MatchState state) {
        gameState.matchState = state;
        if (state.status == "ended") {
            EndGame();
        }
    }

    private void InitClient() {
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

    private void InitGame() {
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
        else if (IsServer) {
            S_DisplayCards();
        }
    }

    private void EndGame() {
        GameObject LineObject = Instantiate(Line);
        GameObject Final = Instantiate(FinalLeaderBoard);
        LineObject.transform.SetParent(Content, false);
        Final.transform.SetParent(Content, false);
        Final.GetComponent<FinalLeaderboard>().SetData(gameState.matchState);
    }

    // This is only for testing
    public GameObject TestCreate;

    private string player_01;
    private string player_02;
    public void ChangePlayer01(string value) {
        player_01 = value;
    }
    public void ChangePlayer02(string value) {
        player_02 = value;
    }
    public void SubmitCreateServer() {
        TestCreate.SetActive(false);
        InitServer("123");
    }


    private string match_id;
    public void ChangeMatchID(string value) {
        match_id = value;
    }
    public void SubmitStartServer() {
        TestCreate.SetActive(false);
        InitServer(match_id);
    }
}
