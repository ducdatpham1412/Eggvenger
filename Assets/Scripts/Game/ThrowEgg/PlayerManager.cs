using UnityEngine;

public class PlayerManager : EntityManager {
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

    private void UpdateCG() {
        if (Renderer == null) {
            Renderer = GetComponent<SpriteRenderer>();
            Renderer.enabled = false;
        }
    }


    public void Initialize(string _ID) {
        ID = _ID;
        UpdateCG();
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
