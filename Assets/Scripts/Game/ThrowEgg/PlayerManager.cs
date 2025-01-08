using UnityEngine;
using PlayerType = ThrowEggState.Player;

public class PlayerManager : EntityManager {
    private PlayerType player;
    private SpriteRenderer Renderer;
    private Rigidbody2D Rigid;
    private Vector3 delta;
    public string ID;

    void Awake() {
        Renderer = GetComponent<SpriteRenderer>();
        Rigid = GetComponent<Rigidbody2D>();
        panable = true;
    }

    void Update() {
        HandlePanning();
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
    }

    private void UpdateCG(ThrowEggState state) {
        if (Renderer == null) {
            Renderer = GetComponent<SpriteRenderer>();
            Renderer.enabled = false;
        }

        if (player.id == state.data.turn && Renderer.enabled) {
            Renderer.enabled = false;
        }
        else if (player.id != state.data.turn && !Renderer.enabled) {
            Renderer.enabled = true; // TODO: Set interactable if appState.myId = player_id
        }
    }


    public void Initialize(PlayerType _player, ThrowEggState state) {
        player = _player;
        ID = player.id;
        UpdateCG(state);
        panable = true; // TODO: Update this with condition
    }


    private void HandlePanning() {
        if (!panable) {
            return;
        }

        if (GameHelper.TouchBegin()) {
            Vector3 mousePos = Input.mousePosition;
            bool check = GameHelper.TouchHitGameObject(mousePos, gameObject);
            if (check) {
                isPanning = true;
            }
            delta = transform.position - GameHelper.ToWorldPoint(mousePos);
        }

        if (GameHelper.TouchReleased()) {
            isPanning = false;
            Rigid.linearVelocity = Vector2.zero;
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
            Rigid.linearVelocity = new Vector2(directionX * 2f, 0);
        }
    }
}
