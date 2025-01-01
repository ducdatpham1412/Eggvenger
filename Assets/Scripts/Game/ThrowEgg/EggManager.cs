using UnityEngine;

using EggType = ThrowEggState.Egg;

public class EggManager : EntityManager {
    public Sprite ExplodedSprite;
    private EggType egg;
    private Vector3 delta;
    private Rigidbody2D rb;
    private Profile profile;
    private bool shouldCheckMissed = true;


    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        profile = GameManager.Instance.appState.profile;
    }

    void Update() {
        HandlePanning();
        HandleCheckMissed();
    }


    public void Initialize(EggType _egg, bool _panable) {
        egg = _egg;
        panable = _panable;
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.gameObject == throwEggLogic.Target.gameObject) {
            rb.linearVelocity = Vector2.zero;
            shouldCheckMissed = false;
            throwEggLogic.SendEventTurnResult(egg.id, true);
        }
    }

    protected override void OnUpdateState(ThrowEggState state) {
        string eggID = egg.id;
        egg = (EggType)Helper.Get(state.data.eggs, eggID);

        if (!panable) {
            ThrowEggState.Player me = (ThrowEggState.Player)Helper.Get(state.data.players, profile.id);
            if (me != null) {
                if (me.status == "ready" && egg.creator == profile.id) {
                    panable = true;
                }
            }
        }

        if (rb == null) {
            rb = GetComponent<Rigidbody2D>();
            profile = GameManager.Instance.appState.profile;
        }

        // Case 1: Not found in state.eggs => Destroy egg
        if (egg == null) {
            throwEggLogic.RemoveEgg(eggID);
            Destroy(gameObject);
            return;
        }

        // Case 2: Found egg, egg status = hit => Explode it
        if (egg.status == "hit") {
            Explode();
        }

        // Case 3: Found egg, but egg status is still active => Do nothing
        rb.linearVelocity = new Vector2(egg.velocity.x, egg.velocity.y);
    }

    private void Explode() {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = ExplodedSprite;
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
            throwEggLogic.SendEventShot(egg.id);
        }


        if (isPanning) {
            Vector2 mousePos = GameHelper.ToWorldPoint(Input.mousePosition) + delta;

            if (delta != Vector3.zero) {
                delta = Vector3.zero;
            }

            float distance = Vector2.Distance(mousePos, transform.position);
            if (distance <= 0.1f) {
                throwEggLogic.SendEventEggMove(egg.id, new Coordinate { x = 0, y = 0 });
                return;
            }

            float directionX = (mousePos - (Vector2)transform.position).normalized.x;
            throwEggLogic.SendEventEggMove(egg.id, new Coordinate { x = directionX * 2f, y = 0 });
        }
    }

    private void HandleCheckMissed() {
        if (!shouldCheckMissed) {
            return;
        }
        float posY = rb.position.y;
        if (posY > GameHelper.world.maxY) {
            throwEggLogic.SendEventTurnResult(egg.id, false);
            shouldCheckMissed = false;
        }
    }
}
