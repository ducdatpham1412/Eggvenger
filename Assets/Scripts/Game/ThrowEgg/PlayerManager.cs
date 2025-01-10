using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : EntityManager {
    private Rigidbody2D Rigid;
    private Vector3 delta;
    private GamePoints gamePoints;

    public bool s_Ready = false;

    public NetworkVariable<PlayerNetwork> m_Player = new NetworkVariable<PlayerNetwork>();
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

    public void Initialize(string _ID, Vector2 initPos, ThrowEggLogic _eggLogic) {
        GetComponent<NetworkObject>().Spawn();
        m_Player.Value = new PlayerNetwork {
            id = _ID,
            move_speed = 2f,
            shot_speed = 40f,
            point = 0,
            init_pos = initPos,
            clientID = -1,
            name = $"Name_{_ID}",
            avatar = $"Name_{_ID}",
        };
        _eggLogic.s_Players.Add(_ID, this);
        UpdateEnable(_eggLogic.m_TargetID.Value);
        _eggLogic.m_TargetID.OnValueChanged += OnTargetChanged;
    }

    public void UpdatePoint(int newPoint) {
        m_Player.Value = new PlayerNetwork {
            id = m_Player.Value.id,
            move_speed = m_Player.Value.move_speed,
            shot_speed = m_Player.Value.shot_speed,
            point = newPoint,
            init_pos = m_Player.Value.init_pos,
            clientID = m_Player.Value.clientID,
            name = m_Player.Value.name,
            avatar = m_Player.Value.avatar,
        };
    }

    private void OnTargetChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue) {
        UpdateEnable(newValue);
    }

    private void OnPlayerChanged(PlayerNetwork _, PlayerNetwork newValue) {
        if (gamePoints == null) {
            GameObject Canvas = GameObject.Find("Canvas");
            Transform MatchScore = Helper.FindChildRecursive(Canvas.transform, "MatchScore");
            bool isUnder = m_Player.Value.init_pos.y < 0f;
            gamePoints = Helper.FindChildRecursive(MatchScore, isUnder ? "PointDown" : "PointUp").GetComponent<GamePoints>();
        }
        gamePoints.TextValue.text = newValue.name;
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
