using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Collections;
using System.Collections;
using Newtonsoft.Json.Linq;


public class ThrowEggLogic : NetworkBehaviour {
    public GameObject EggPrefab;
    public GameObject PlayerPrefab;
    private RoomThrowEgg roomThrowEgg;
    private MatchState matchState;

    private Dictionary<string, PlayerManager> s_Players = new Dictionary<string, PlayerManager>();
    private GameObject s_Egg;

    public LocalizeStringEvent c_TextNotice;
    public GameObject c_MatchScore;
    public Image c_Hover;

    public NetworkVariable<FixedString32Bytes> m_TargetID = new NetworkVariable<FixedString32Bytes>("");


    void Awake() {
        matchState = GameManager.Instance.gameState.matchState;
    }


    void Start() {
        SoundManager.Instance.PlayMusic(SoundManager.MusicSource.lifeWandering);
    }

    public override void OnNetworkDespawn() {
        SoundManager.Instance.PauseUnPauseMusicBackground();
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        InitGame();
    }

    private void S_InitEgg(PlayerManager playerManager) {
        bool isUnder = playerManager.m_Player.Value.init_pos.y < 0;
        s_Egg = Instantiate(EggPrefab, playerManager.m_Player.Value.init_pos, isUnder ? Quaternion.identity : Quaternion.Euler(180, 0, 0));
        EggManager manager = s_Egg.GetComponent<EggManager>();
        int ownerClientID = matchState.players.Find(p => p.id == playerManager.m_Player.Value.id).clientID;
        manager.Initialize(
            creatorID: playerManager.m_Player.Value.id,
            move_speed: playerManager.m_Player.Value.move_speed,
            shot_speed: playerManager.m_Player.Value.shot_speed * (isUnder ? 1 : -1), _logic: this,
            ownerClientID: ownerClientID < 0 ? null : (ulong)ownerClientID
        );
    }

    private void InitServer() {
        PlayerManager InitPlayer(RoomThrowEgg.Player player) {
            GameObject newGameObject = Instantiate(PlayerPrefab, player.init_pos, Quaternion.identity);
            PlayerManager manager = newGameObject.GetComponent<PlayerManager>();
            manager.Initialize(player, this);
            s_Players.Add(player.id, manager);
            return manager;
        }

        m_TargetID.Value = roomThrowEgg.players[1].id;
        PlayerManager manager01 = InitPlayer(roomThrowEgg.players[0]);
        InitPlayer(roomThrowEgg.players[1]);
        S_InitEgg(manager01);

        // TODO: Remove
        Introduce();
    }

    private void InitGame() {
        List<JObject> rooms = matchState.rooms;
        roomThrowEgg = rooms.Find(r => r["status"].ToString() == "active").ToObject<RoomThrowEgg>();

        if (IsServer) {
            InitServer();
            return;
        }

        if (IsClient) {
            InitializeServerRpc();
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void InitializeServerRpc(ServerRpcParams rpcParams = default) {
        ulong clientID = rpcParams.Receive.SenderClientId;
        MatchState.Player fPlayer = matchState.players.Find(p => p.clientID == (int)clientID);
        if (fPlayer != null) {
            PlayerManager player = (PlayerManager)Helper.Get(s_Players, fPlayer.id);
            if (player != null) {
                NetworkObject network = player.gameObject.GetComponent<NetworkObject>();
                network.ChangeOwnership(clientID);
                PlayerManager manager = player.gameObject.GetComponent<PlayerManager>();

                if (s_Egg) {
                    EggManager eggManager = s_Egg.GetComponent<EggManager>();
                    if (eggManager) {
                        if (eggManager.m_Egg.Value.creator == fPlayer.id) {
                            NetworkObject eggNetwork = s_Egg.GetComponent<NetworkObject>();
                            eggNetwork.ChangeOwnership(clientID);
                        }
                    }
                }

                IntroduceRoomClientRpc();
            }
        }

    }


    [ClientRpc]
    private void IntroduceRoomClientRpc() {
        Introduce();
    }

    private async void Introduce() {
        bool isMyTurnFirst = m_TargetID.Value.ToString() != GameManager.Instance.appState.profile.id;
        await ShowNotice(isMyTurnFirst ? "youAreTheFirst" : "opponentBeFirst", 2000);
        await ShowNotice("areYouReady", 1500, 30);
        await ShowNotice("fight", 1000, 50);
        StartCoroutine(FadeOutHover());
    }

    private IEnumerator FadeOutHover() {
        Color color = c_Hover.color;
        float originalA = color.a;
        float elapsedTime = 0f;
        float duration = 0.7f;
        while (elapsedTime < duration) {
            float alpha = Mathf.Lerp(originalA, 0f, elapsedTime / duration);
            c_Hover.color = new Color(color.r, color.g, color.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        c_Hover.color = new Color(color.r, color.g, color.b, 0f);
        c_Hover.gameObject.SetActive(false);
        c_MatchScore.GetComponent<CanvasGroup>().alpha = 1;
    }

    private async Task ShowNotice(string key, int duration = 3000, int fontSize = 20) {
        c_TextNotice.StringReference.SetReference(LocalizationManager.Table.Game.ToString(), key);
        Text text = c_TextNotice.gameObject.GetComponent<Text>();
        text.fontSize = fontSize;
        c_TextNotice.gameObject.SetActive(true);
        await Task.Delay(duration);
        c_TextNotice.gameObject.SetActive(false);
    }

    private bool S_PlusPointAndCheckEnded(PlayerManager Shot, PlayerManager Target) {
        int newPoint = Shot.m_Player.Value.point + 1;
        bool equalTurn = roomThrowEgg.eggs.Count % 2 == 0;
        if (newPoint == 5) {
            if (equalTurn) {
                if (Target.m_Player.Value.point < 5) {
                    Shot.UpdatePoint(newPoint);
                    return true; // ended
                }
                //Both 2 players have 5 points, reset to 3 point for all
                Shot.UpdatePoint(3);
                Target.UpdatePoint(3);
                return false;
            }
            if ((newPoint - Target.m_Player.Value.point) >= 2) {
                Shot.UpdatePoint(newPoint);
                return true;
            }
        }
        Shot.UpdatePoint(newPoint);
        return false;
    }

    private async void S_SwitchTurn(bool isHit) {
        // TODO: Do smile, sound effect here
        await Task.Delay(1300);
        NetworkObject network = s_Egg.GetComponent<NetworkObject>();
        network.Despawn();
        PlayerManager Target = s_Players[m_TargetID.Value.ToString()];
        foreach (KeyValuePair<string, PlayerManager> kpv in s_Players) {
            if (kpv.Key != m_TargetID.Value.ToString()) {
                m_TargetID.Value = kpv.Key;
                PlayerManager Shot = kpv.Value;
                if (isHit) {
                    bool ended = S_PlusPointAndCheckEnded(Shot, Target);
                    if (ended) {
                        EndGame(winnerID: Shot.m_Player.Value.id);
                        return;
                    }
                }
                else if (Target.m_Player.Value.point == 5) {
                    EndGame(winnerID: Target.m_Player.Value.id);
                    return;
                }
                S_InitEgg(Target);
                return;
            }
        }
    }

    public void SendResult(bool isHit) {
        EggManager manager = s_Egg.GetComponent<EggManager>();
        manager.m_Egg.Value.status = isHit ? "hit" : "missed";
        if (manager != null) {
            roomThrowEgg.eggs.Add(manager.m_Egg.Value);
        }
        S_SwitchTurn(isHit);
    }


    [ClientRpc]
    public void C_ShowWinnerClientRpc(string winnerID) {
        Debug.Log($"Winner winner: {winnerID}");
        // TODO: Winner UI
    }

    private async void EndGame(string winnerID) {
        foreach (RoomThrowEgg.Player player in roomThrowEgg.players) {
            player.CopyProperties(s_Players[player.id].m_Player.Value);
            s_Players[player.id].gameObject.GetComponent<NetworkObject>().Despawn();
        }
        roomThrowEgg.status = BaseRoom.Status.ended.ToString();

        for (int i = 0; i < matchState.rooms.Count; i++) {
            if (matchState.rooms[i]["id"].ToString() == roomThrowEgg.id) {
                matchState.rooms[i] = JObject.FromObject(roomThrowEgg);
                break;
            }
        }

        // Reset status of all player to "active", prepare for next room or ending match
        foreach (MatchState.Player player in matchState.players) {
            player.status = MatchState.Player.Status.active.ToString();
        }

        C_ShowWinnerClientRpc(winnerID);
        await Task.Delay(2500);
        Navigator.Instance.NetworkLoad(Navigator.Scene.MatchScene);
    }
}
