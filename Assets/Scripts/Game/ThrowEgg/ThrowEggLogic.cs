using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Collections;
using System.Collections;


public class ThrowEggLogic : NetworkBehaviour {
    public SpriteRenderer bgRenderer;
    public GameObject EggPrefab;
    public GameObject PlayerPrefab;

    public Dictionary<string, PlayerManager> s_Players = new Dictionary<string, PlayerManager>();
    private GameObject s_Egg;
    private List<EggNetwork> s_DataEggs = new List<EggNetwork>();

    public LocalizeStringEvent c_TextNotice;
    public GameObject c_MatchScore;
    public Image c_Hover;

    public NetworkVariable<FixedString32Bytes> m_TargetID = new NetworkVariable<FixedString32Bytes>("");


    void Start() {
        SoundManager.Instance.PlayMusic(SoundManager.MusicSource.matchScene);
        if (!bgRenderer.sprite && GameManager.Instance.background) {
            bgRenderer.sprite = GameManager.Instance.background;
        }
        // InitGame();
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        if (IsClient) {
            InitializeServerRpc(GameManager.Instance.appState.profile.id);
        }
    }

    private void InitEgg(PlayerManager playerManager, Vector2 initPos) {
        bool isUnder = initPos.y < 0;
        s_Egg = Instantiate(EggPrefab, initPos, isUnder ? Quaternion.identity : Quaternion.Euler(180, 0, 0));
        EggManager manager = s_Egg.GetComponent<EggManager>();
        bool playerHasOwner = playerManager.m_Player.Value.clientID >= 0;
        ulong? owner = playerHasOwner ? (ulong)playerManager.m_Player.Value.clientID : null;
        manager.Initialize(
            creatorID: playerManager.m_Player.Value.id,
            move_speed: playerManager.m_Player.Value.move_speed,
            shot_speed: playerManager.m_Player.Value.shot_speed * (isUnder ? 1 : -1), _logic: this,
            owner: owner
        );
    }

    private void InitServer(string player_01, string player_02) {
        PlayerManager InitPlayer(string playerID, Vector2 pos) {
            GameObject player = Instantiate(PlayerPrefab, pos, Quaternion.identity);
            PlayerManager manager = player.GetComponent<PlayerManager>();
            manager.Initialize(playerID, pos, this);
            return manager;
        }

        NetworkManager.Singleton.StartServer();
        m_TargetID.Value = player_02;
        PlayerManager manager01 = InitPlayer(player_01, new Vector2(0f, -4f));
        InitPlayer(player_02, new Vector2(0f, 3f));
        InitEgg(manager01, new Vector2(0f, -4f));

        // TODO: Remove
        Introduce();
    }

    private void InitClient() {
        NetworkManager.Singleton.StartClient();
    }

    private void InitGame() {
        string player_01 = Environment.GetEnvironmentVariable("player_01");
        string player_02 = Environment.GetEnvironmentVariable("player_02");
        string match_id = Environment.GetEnvironmentVariable("id");

        if (player_01 != null && player_02 != null) {
            InitServer(player_01, player_02);
            return;
        }

        InitClient();
    }


    [ServerRpc(RequireOwnership = false)]
    private void InitializeServerRpc(string ID, ServerRpcParams rpcParams = default) {
        PlayerManager player = (PlayerManager)Helper.Get(s_Players, ID);
        if (player != null) {
            NetworkObject network = player.gameObject.GetComponent<NetworkObject>();
            ulong clientID = rpcParams.Receive.SenderClientId;
            network.ChangeOwnership(clientID);
            PlayerManager manager = player.gameObject.GetComponent<PlayerManager>();
            manager.s_Ready = true;
            manager.m_Player.Value.clientID = (long)clientID;

            if (s_Egg) {
                EggManager eggManager = s_Egg.GetComponent<EggManager>();
                if (eggManager) {
                    if (eggManager.m_Egg.Value.creator == ID) {
                        NetworkObject eggNetwork = s_Egg.GetComponent<NetworkObject>();
                        eggNetwork.ChangeOwnership(clientID);
                    }
                }
            }

            bool allReady = true;
            foreach (PlayerManager p in s_Players.Values) {
                if (!p.s_Ready) {
                    allReady = false;
                }
            }
            if (allReady) {
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
        float duration = 1.3f;
        while (elapsedTime < duration) {
            float alpha = Mathf.Lerp(originalA, 0f, elapsedTime / duration);
            c_Hover.color = new Color(color.r, color.g, color.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        c_Hover.color = new Color(color.r, color.g, color.b, 0f);
        c_Hover.gameObject.SetActive(false);
        c_MatchScore.SetActive(true);
    }

    private async Task ShowNotice(string key, int duration = 3000, int fontSize = 20) {
        c_TextNotice.StringReference.SetReference(LocalizationManager.Table.Game.ToString(), key);
        Text text = c_TextNotice.gameObject.GetComponent<Text>();
        text.fontSize = fontSize;
        c_TextNotice.gameObject.SetActive(true);
        await Task.Delay(duration);
        c_TextNotice.gameObject.SetActive(false);
    }

    private bool PlusPointAndCheckEnded(PlayerManager Shot, PlayerManager Target) {
        int newPoint = Shot.m_Player.Value.point + 1;
        bool equalTurn = s_DataEggs.Count % 2 == 0;
        if (newPoint == 5) {
            if (equalTurn) {
                if (Target.m_Player.Value.point < 5) {
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

    private async void SwitchTurn(bool isHit) {
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
                    bool ended = PlusPointAndCheckEnded(Shot, Target);
                    if (ended) {
                        EndGame(Shot.m_Player.Value.name);
                        return;
                    }
                }
                InitEgg(Target, Target.m_Player.Value.init_pos);
                return;
            }
        }
    }

    // [ServerRpc(RequireOwnership = false)]
    private void SendResult__(bool isHit) {
        EggManager manager = s_Egg.GetComponent<EggManager>();
        manager.m_Egg.Value.status = isHit ? "hit" : "missed";
        if (manager != null) {
            s_DataEggs.Add(manager.m_Egg.Value);
        }
        SwitchTurn(isHit);
    }

    public void SendResult(bool isHit) {
        SendResult__(isHit);
    }

    private void EndGame(string playerName) {
        Debug.Log($"Winner winner: {playerName}");
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
        InitServer(player_01, player_02);
    }


    private string my_player;
    public void ChangeMyPlayerID(string value) {
        my_player = value;
    }
    public void SubmitStartClient() {
        TestCreate.SetActive(false);
        GameManager.Instance.UpdateAppState(state => {
            state.profile.id = my_player;
            return state;
        });
        InitClient();
    }
}
