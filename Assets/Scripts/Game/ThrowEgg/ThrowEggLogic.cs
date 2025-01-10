using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using System.Threading;
using Newtonsoft.Json;
using Unity.Netcode;

public class ThrowEggLogic : NetworkBehaviour {
    public SpriteRenderer bgRenderer;
    public GameObject EggPrefab;
    public GameObject PlayerPrefab;
    public LocalizeStringEvent TextNotice;

    public PlayerManager Target;

    public event Action<ThrowEggState> OnUpdateState;

    private UdpClient udpClient;
    private SynchronizationContext context;
    private Profile profile;
    private Dictionary<string, PlayerManager> players = new Dictionary<string, PlayerManager>();
    private List<string> eggIDs = new List<string>();
    public string matchID = "Ln5If3U";
    public string roomID = "N1JPQvM";


    public override void OnNetworkSpawn() {
        if (IsClient) {
            if (!bgRenderer.sprite && GameManager.Instance.background) {
                bgRenderer.sprite = GameManager.Instance.background;
            }
            profile = GameManager.Instance.appState.profile;
            Initialize(profile.id);
        }

        if (IsServer) {
            // NetworkManager.Singleton.OnClientConnectedCallback += OnConnected;
            InitGame();
        }


        base.OnNetworkSpawn();
    }


    // private void OnConnected(ulong clientId) {
    //     Debug.Log("Connected: " + clientId);
    // }


    private void InitGame() {
        // string player_01 = Environment.GetEnvironmentVariable("player_01");
        // string player_02 = Environment.GetEnvironmentVariable("player_02");
        GameObject Player01 = Instantiate(PlayerPrefab, Vector2.zero, Quaternion.identity);
        PlayerManager player = Player01.GetComponent<PlayerManager>();
        player.Initialize("player_1");
        players.Add("player_1", player);
        NetworkObject network = Player01.GetComponent<NetworkObject>();
        network.Spawn();
    }


    [ServerRpc(RequireOwnership = false)]
    private void InitializeServerRpc(string ID, ServerRpcParams rpcParams = default) {
        PlayerManager player = (PlayerManager)Helper.Get(players, ID);
        if (player != null) {
            NetworkObject network = player.gameObject.GetComponent<NetworkObject>();
            network.ChangeOwnership(rpcParams.Receive.SenderClientId);
        }
    }
    private void Initialize(string ID) {
        InitializeServerRpc(ID);
    }



    private void SendData(object data) {
        string jsonData = JsonConvert.SerializeObject(data);
        byte[] dataBytes = Encoding.UTF8.GetBytes(jsonData);
        udpClient.Send(dataBytes, dataBytes.Length);
    }



    private void Invoke(ThrowEggState _state) {
        context.Post(_ => {
            OnUpdateState?.Invoke(_state);
        }, null);
    }

    public void SendEventShot(string eggId) {
        Event.Shot shot = new Event.Shot {
            match_id = matchID,
            room_id = roomID,
            egg_id = eggId,
        };
        SendData(shot);
    }

    public void SendEventEggMove(string eggID, Coordinate velocity) {
        Event.EggMove eggMove = new Event.EggMove {
            match_id = matchID,
            room_id = roomID,
            id = eggID,
            velocity = velocity,
        };
        SendData(eggMove);
    }

    public void SendEventTurnResult(string eggID, bool isHit) {
        Event.TurnResult res = new Event.TurnResult {
            match_id = matchID,
            room_id = roomID,
            egg_id = eggID,
            status = isHit ? "hit" : "missed"
        };
        SendData(res);
    }

    private async Task ShowNotice(string key, int duration = 3000, int fontSize = 20) {
        TextNotice.StringReference.SetReference(LocalizationManager.Table.Game.ToString(), key);
        Text text = TextNotice.gameObject.GetComponent<Text>();
        text.fontSize = fontSize;
        TextNotice.gameObject.SetActive(true);
        await Task.Delay(duration);
        TextNotice.gameObject.SetActive(false);
    }


    private void SwitchTurn(ThrowEggState _state) {

    }

    private void EndGame(ThrowEggState _state) {

    }


    private void CreateEgg(ThrowEggState.Egg egg, bool panable = false) {
        context.Post(_ => {
            eggIDs.Add(egg.id);
            int rotationX = egg.last_pos.y > 0 ? 180 : 0;
            GameObject Egg = Instantiate(EggPrefab, new Vector2(egg.last_pos.x, egg.last_pos.y), Quaternion.Euler(rotationX, 0, 0));
            EggManager eggManager = Egg.GetComponent<EggManager>();
            // eggManager.SetThrowEggLogic(this);
            eggManager.Initialize(egg, panable);
        }, null);
    }

    public void RemoveEgg(string eggID) {
        eggIDs.Remove(eggID);
    }
}
