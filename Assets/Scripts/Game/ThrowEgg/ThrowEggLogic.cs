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

public class ThrowEggLogic : MonoBehaviour {
    public SpriteRenderer bgRenderer;
    public GameObject EggPrefab;
    public GameObject PlayerPrefab;
    public LocalizeStringEvent TextNotice;

    public PlayerManager Target;

    public event Action<ThrowEggState> OnUpdateState;

    private UdpClient udpClient;
    private SynchronizationContext context;
    private Profile profile;
    private bool initiated = false;
    private Dictionary<string, PlayerManager> players = new Dictionary<string, PlayerManager>();
    private List<string> eggIDs = new List<string>();
    public string matchID = "Ln5If3U";
    public string roomID = "N1JPQvM";


    void Start() {
        if (!bgRenderer.sprite && GameManager.Instance.background) {
            bgRenderer.sprite = GameManager.Instance.background;
        }

        udpClient = new UdpClient();
        udpClient.Connect("127.0.0.1", 5000);
        Task.Run(HandleState);

        context = SynchronizationContext.Current;
        profile = GameManager.Instance.appState.profile;

        Event.Connect connect = new Event.Connect {
            player_id = profile.id,
        };
        SendData(connect);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            TestPing();
        }
    }


    private void SendData(object data) {
        string jsonData = JsonConvert.SerializeObject(data);
        byte[] dataBytes = Encoding.UTF8.GetBytes(jsonData);
        udpClient.Send(dataBytes, dataBytes.Length);
    }

    void TestPing() {
        Event.LoadRoom loadRoom = new Event.LoadRoom {
            match_id = matchID,
            room_id = roomID,
        };
        SendData(loadRoom);
    }


    private void Invoke(ThrowEggState _state) {
        context.Post(_ => {
            OnUpdateState?.Invoke(_state);
        }, null);
    }

    private async void HandleState() {
        while (true) {
            var res = await udpClient.ReceiveAsync();
            string json = Encoding.UTF8.GetString(res.Buffer);
            ThrowEggState state = JsonConvert.DeserializeObject<ThrowEggState>(json);

            if (!initiated) {
                initiated = true;
                InitGame(state);
            }

            else if (state.status == "ended") {
                EndGame(state);
            }

            else if (state.data.turn == Target.ID) {
                SwitchTurn(state);
            }

            else {
                Invoke(state);
            }
        }
    }

    public void SendEventReady() {
        Event.Ready ready = new Event.Ready { };
        SendData(ready);
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

    private void InitGame(ThrowEggState _state) {
        context.Post(async _ => {
            foreach (ThrowEggState.Player player in _state.data.players.Values) {
                GameObject PlayerGameObject = Instantiate(PlayerPrefab, new Vector2(player.last_pos.x, player.last_pos.y), Quaternion.identity); // TODO: Denormal
                PlayerManager playerManager = PlayerGameObject.GetComponent<PlayerManager>();
                // playerManager.SetThrowEggLogic(this);
                playerManager.Initialize(player, _state);
                players[player.id] = playerManager;
                if (playerManager.ID != _state.data.turn) {
                    Target = playerManager;
                }
            }

            foreach (ThrowEggState.Egg egg in _state.data.eggs.Values) {
                CreateEgg(egg);
            }

            await ShowNotice("youAreTheFirst", 1000);
            await ShowNotice("areYouReady", 1500, 30);
            await ShowNotice("fight", 1000, 50);
            SendEventReady();
        }, null);
    }


    private void SwitchTurn(ThrowEggState _state) {
        context.Post(_ => {
            foreach (string playerID in players.Keys) {
                if (playerID != _state.data.turn) {
                    Target = players[playerID];
                    break;
                }
            }
            foreach (KeyValuePair<string, ThrowEggState.Egg> kvp in _state.data.eggs) {
                if (!eggIDs.Contains(kvp.Key)) {
                    CreateEgg(kvp.Value, true);
                }
            }
            Invoke(_state);
        }, null);
    }

    private void EndGame(ThrowEggState _state) {
        Invoke(_state);
        // TODO: Show result
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


    // void Init() {
    //     Camera camera = Camera.main;

    //     Debug.Log("Screen size: " + Screen.width + " - " + Screen.height);
    //     Debug.Log("Local max: " + Screen.width / 2 + " - " + Screen.height / 2);

    //     Vector3 screenTopRight = camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, camera.nearClipPlane));
    //     Debug.Log("Max point: " + screenTopRight.x + " - " + screenTopRight.y);

    //     // Transform check = new Transform();
    //     // // World point to local point
    //     // check.InverseTransformPoint(new Vector3()); // vector3 is world point


    //     // Calculate normalized width relative to orthographic size
    //     Camera mainCamera = Camera.main;
    //     float normalizedWidth = mainCamera.orthographicSize * 2 * mainCamera.aspect; // Width in world space
    //     Debug.Log("Aspect: " + mainCamera.aspect);


    //     float inverseLerp = Mathf.InverseLerp(-1, 1, 0.2f);
    //     Debug.Log("InverseLerp" + inverseLerp);
    //     Debug.Log("Lert: " + Mathf.Lerp(-2, 2, inverseLerp));
    // }
}
