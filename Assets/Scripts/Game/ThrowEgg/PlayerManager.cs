using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : EntityManager {
    private Rigidbody2D Rigid;
    private Vector3 delta;
    private GamePoints gamePoints;

    private bool c_setAvatar = false;

    public NetworkVariable<RoomThrowEgg.Player> m_Player = new NetworkVariable<RoomThrowEgg.Player>();
    private NetworkVariable<bool> m_enable = new NetworkVariable<bool>(false);

    void Awake() {
        GetComponent<SpriteRenderer>().enabled = m_enable.Value;
        GetComponent<BoxCollider2D>().enabled = m_enable.Value;
        Rigid = GetComponent<Rigidbody2D>();
    }

    void Update() {
        HandlePanning();
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        m_enable.OnValueChanged += OnEnableChanged;
        m_Player.OnValueChanged += OnPlayerChanged;
    }

    public override void OnNetworkDespawn() {
        base.OnNetworkDespawn();
        m_enable.OnValueChanged -= OnEnableChanged;
        m_Player.OnValueChanged -= OnPlayerChanged;
    }

    private void OnEnableChanged(bool _, bool newValue) {
        GetComponent<SpriteRenderer>().enabled = newValue;
        GetComponent<BoxCollider2D>().enabled = newValue;
    }

    private void UpdateEnable(FixedString32Bytes targetID) {
        m_enable.Value = targetID.ToString() == m_Player.Value.id;
    }

    public void Initialize(RoomThrowEgg.Player player, ThrowEggLogic _eggLogic) {
        GetComponent<NetworkObject>().Spawn();
        m_Player.Value = player;
        UpdateEnable(_eggLogic.m_TargetID.Value);
        _eggLogic.m_TargetID.OnValueChanged += OnTargetChanged;
    }

    public void UpdatePoint(int newPoint) {
        m_Player.Value = new RoomThrowEgg.Player {
            id = m_Player.Value.id,
            move_speed = m_Player.Value.move_speed,
            shot_speed = m_Player.Value.shot_speed,
            point = newPoint,
            init_pos = m_Player.Value.init_pos,
        };
    }

    private void OnTargetChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue) {
        UpdateEnable(newValue);
    }

    private async void SetAvatar(string url) {
        c_setAvatar = true;
        try {
            Sprite sprite = await Helper.ImgUrlToSprite(url);
            GetComponent<SpriteRenderer>().sprite = sprite;
            // Helper.FitSpriteToGameObject(gameObject);
        }
        catch (System.Exception) {
            c_setAvatar = false;
            Debug.LogWarning("Error set avatar");
        }
    }

    private void OnPlayerChanged(RoomThrowEgg.Player _, RoomThrowEgg.Player newValue) {
        if (gamePoints == null) {
            GameObject Canvas = GameObject.Find("Canvas");
            Transform MatchScore = Helper.FindChildRecursive(Canvas.transform, "MatchScore");
            if (MatchScore != null) {
                bool isUnder = m_Player.Value.init_pos.y < 0f;
                Transform PointObject = Helper.FindChildRecursive(MatchScore, isUnder ? "PointDown" : "PointUp");
                gamePoints = PointObject.GetComponent<GamePoints>();
                MatchState.Player fPlayer = GameManager.Instance.gameState.matchState.players.Find(p => p.id == newValue.id);
                gamePoints.TextValue.text = fPlayer.name;
            }
        }

        if (!c_setAvatar) {
            MatchState.Player player = GameManager.Instance.gameState.matchState.players.Find(p => p.id == newValue.id);
            SetAvatar(player.avatar);
        }

        gamePoints.UpdatePoint(newValue.point);
    }

    private void HandlePanning() {
        if (!IsOwner) {
            return;
        }

        if (GameHelper.TouchBegin()) {
            Vector3 mousePos = Input.mousePosition;
            bool check = GameHelper.TouchHitGameObject(mousePos, gameObject);
            if (check) {
                isPanning = true;
                delta = transform.position - GameHelper.ToWorldPoint(mousePos);
            }
        }

        if (GameHelper.TouchReleased()) {
            if (isPanning) {
                isPanning = false;
                Rigid.linearVelocity = Vector2.zero;
            }
            return;
        }

        if (isPanning) {
            Vector2 mousePos = GameHelper.ToWorldPoint(Input.mousePosition) + delta;

            if (delta != Vector3.zero) {
                delta = Vector3.zero;
            }

            float distance = Vector2.Distance(mousePos, transform.position);
            if (distance <= 0.1f) {
                Rigid.linearVelocity = Vector2.zero;
                return;
            }

            float directionX = (mousePos - (Vector2)transform.position).normalized.x;
            Rigid.linearVelocity = new Vector2(directionX * m_Player.Value.move_speed, 0);
        }
    }
}
