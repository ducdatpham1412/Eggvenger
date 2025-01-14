using Unity.Netcode;
using UnityEngine;

public class EggManager : EntityManager {
    public Sprite ExplodedSprite;
    private Rigidbody2D Rigid;

    private ThrowEggLogic s_throwEggLogic;
    private bool s_shouldCheckMissed = false;

    private Vector3 c_delta;

    public NetworkVariable<RoomThrowEgg.Egg> m_Egg = new NetworkVariable<RoomThrowEgg.Egg>();

    void Awake() {
        Rigid = GetComponent<Rigidbody2D>();
    }

    void Update() {
        HandlePanning();
        HandleCheckMissed();
    }

    public void Initialize(string creatorID, float move_speed, float shot_speed, ThrowEggLogic _logic, ulong? ownerClientID = null) {
        NetworkObject network = GetComponent<NetworkObject>();
        network.Spawn();
        s_throwEggLogic = _logic;
        m_Egg.Value = new RoomThrowEgg.Egg {
            id = Helper.GetID(),
            move_speed = move_speed,
            shot_speed = shot_speed,
            created = Helper.TsNow(),
            creator = creatorID,
            status = "active",
        };
        if (ownerClientID != null) {
            network.ChangeOwnership((ulong)ownerClientID);
        }
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (IsServer) {
            PlayerManager manager = collider.gameObject.GetComponent<PlayerManager>();
            if (manager != null && manager.m_Player.Value.id == s_throwEggLogic.m_TargetID.Value.ToString()) {
                Explode();
            }
        }
    }


    private void Explode() {
        s_shouldCheckMissed = false;
        Rigid.linearVelocity = Vector2.zero;
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = ExplodedSprite;
        transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
        s_throwEggLogic.SendResult(true);
    }

    [ServerRpc]
    private void ShotServerRpc() {
        s_shouldCheckMissed = true;
        Rigid.linearVelocity = new Vector2(0f, m_Egg.Value.shot_speed);
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
            }
            c_delta = transform.position - GameHelper.ToWorldPoint(mousePos);
        }

        if (GameHelper.TouchReleased()) {
            if (isPanning) {
                isPanning = false;
                // ShotServerRpc();

                // TODO: Remove this when released
                s_shouldCheckMissed = true;
                Rigid.linearVelocity = new Vector2(0f, m_Egg.Value.shot_speed);
            }
            return;
        }


        if (isPanning) {
            Vector2 mousePos = GameHelper.ToWorldPoint(Input.mousePosition) + c_delta;

            if (c_delta != Vector3.zero) {
                c_delta = Vector3.zero;
            }

            float distance = Vector2.Distance(mousePos, transform.position);
            if (distance <= 0.1f) {
                Rigid.linearVelocity = Vector2.zero;
                return;
            }

            float directionX = (mousePos - (Vector2)transform.position).normalized.x;
            Rigid.linearVelocity = new Vector2(directionX * m_Egg.Value.move_speed, 0f);
        }
    }

    private void HandleCheckMissed() {
        if (!s_shouldCheckMissed || !IsServer) {
            return;
        }
        float posY = Rigid.position.y;
        if (posY > GameHelper.world.maxY || posY < -GameHelper.world.maxY) {
            s_shouldCheckMissed = false;
            s_throwEggLogic.SendResult(false);
        }
    }
}
