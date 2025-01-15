using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Collections;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Linq;


public class ThrowEggLogic : NetworkBehaviour {
    public GameObject EggPrefab;
    public GameObject PlayerPrefab;
    public GameObject MatchScore;
    RoomThrowEgg roomThrowEgg;
    MatchState matchState;
    SynchronizationContext context;

    public LocalizeStringEvent c_TextNotice;
    public Image c_Hover;

    List<int> s_ConnectedClients = new List<int>();
    Dictionary<string, PlayerManager> s_Players = new Dictionary<string, PlayerManager>();
    GameObject s_Egg;

    public NetworkVariable<FixedString32Bytes> m_TargetID = new NetworkVariable<FixedString32Bytes>("");


    void Start() {
        context = SynchronizationContext.Current;
        // SoundManager.Instance.PlayMusic(SoundManager.MusicSource.lifeWandering);

        matchState = GameManager.Instance.gameState.matchState;
        roomThrowEgg = matchState.rooms.Find(r => r["status"].ToString() == "active").ToObject<RoomThrowEgg>();
    }

    public override void OnNetworkDespawn() {
        SoundManager.Instance.PauseUnPauseMusicBackground();
    }

    public override void OnNetworkSpawn() {
        if (IsClient) {
            ReadyServerRpc();
        }
    }


    private void S_InitEgg(PlayerManager playerManager) {
        int ownerClientID = matchState.players.Find(p => p.id == playerManager.m_Player.Value.id).clientID;
        bool isUnder = playerManager.m_Player.Value.init_pos.y < 0;
        s_Egg = Instantiate(EggPrefab, playerManager.m_Player.Value.init_pos, isUnder ? Quaternion.identity : Quaternion.Euler(180, 0, 0));
        EggManager manager = s_Egg.GetComponent<EggManager>();
        manager.Initialize(
            creatorID: playerManager.m_Player.Value.id,
            move_speed: playerManager.m_Player.Value.move_speed,
            shot_speed: playerManager.m_Player.Value.shot_speed * (isUnder ? 1 : -1), _logic: this,
            ownerClientID: (ulong)(ownerClientID < 0 ? 0 : ownerClientID)
        );
    }

    void S_InitObjects() {
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
    }


    [ServerRpc(RequireOwnership = false)]
    void ReadyServerRpc(ServerRpcParams rpcParams = default) {
        ulong clientID = rpcParams.Receive.SenderClientId;
        var allClients = matchState.players.Select(p => p.clientID).ToArray();
        if (!allClients.Contains((int)clientID)) {
            return;
        }

        s_ConnectedClients.Add((int)clientID);
        bool allReady = true;
        foreach (MatchState.Player p in matchState.players) {
            if (!s_ConnectedClients.Contains(p.clientID)) {
                allReady = false;
            }
        }

        if (allReady) {
            S_InitObjects();
            StartCoroutine(FadeOutHover());
            IntroduceRoomClientRpc();
        }
    }

    [ClientRpc]
    private void IntroduceRoomClientRpc() {
        context.Post(async _ => {
            bool isMyTurnFirst = m_TargetID.Value.ToString() != GameManager.Instance.appState.profile.id;
            await ShowNotice(isMyTurnFirst ? "youAreTheFirst" : "opponentBeFirst", 2000);
            await ShowNotice("areYouReady", 1500);
            await ShowNotice("fight", 1000, 50);
            StartCoroutine(FadeOutHover());
        }, null);
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
        MatchScore.GetComponent<CanvasGroup>().alpha = 1;
    }

    private async Task ShowNotice(string key, int duration = 3000, int fontSize = 30) {
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
                        S_EndGame(winnerID: Shot.m_Player.Value.id);
                        return;
                    }
                }
                else if (Target.m_Player.Value.point == 5) {
                    S_EndGame(winnerID: Target.m_Player.Value.id);
                    return;
                }
                S_InitEgg(Target);
                return;
            }
        }
    }

    public void S_SendResult(bool isHit) {
        EggManager manager = s_Egg.GetComponent<EggManager>();
        manager.m_Egg.Value.status = isHit ? "hit" : "missed";
        roomThrowEgg.eggs.Add(manager.m_Egg.Value);
        S_SwitchTurn(isHit);
    }


    [ClientRpc]
    public void C_ShowWinnerClientRpc(string winnerID) {
        Debug.Log($"Winner winner: {winnerID}");
        // TODO: Winner UI
    }

    private async void S_EndGame(string winnerID) {
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
