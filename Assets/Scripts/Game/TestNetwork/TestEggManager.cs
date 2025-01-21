using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class TestEggManager : NetworkBehaviour {
    [System.Serializable]
    public class MovementData : INetworkSerializable {
        public int tick;
        public Vector2 direction;
        public Vector2 position;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref direction);
            serializer.SerializeValue(ref position);
        }
    }

    public Sprite ExplodedSprite;
    public TestManager manager;
    Rigidbody2D Rigid;
    Vector3 c_delta;
    Vector3 predictedPosition;
    bool isPanning = false;
    bool isShotting = false;
    float moveSpeed = 5f;

    [SerializeField] private int tickRate = 60;
    [SerializeField] int currentTick;
    private float time;
    private float tickTime;


    void Awake() {
        Rigid = GetComponent<Rigidbody2D>();
        tickTime = 1f / tickRate;
        time = 0;
    }

    void Update() {
        time += Time.deltaTime;
        HandlePanning();
    }

    private void FixedUpdate() {
        while (time > tickTime) {
            currentTick++;
            time -= tickTime;
        }
    }


    void OnTriggerEnter2D(Collider2D collider) {
        if (!IsServer) {
            return;
        }
        isShotting = false;
        Rigid.linearVelocity = Vector2.zero;
        Explode();
        ExplodeClientRpc();
        // state.direction = Vector3.zero;
        // state.status = "hit";
        // manager.PassStateClientRpc(manager.gameState);
    }

    void Explode() {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = ExplodedSprite;
        transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
    }


    [ClientRpc]
    void ExplodeClientRpc() {
        Explode();
    }

    // void InterpolateToServerPosition() {
    //     transform.position = Vector3.Lerp(transform.position, state.position, Time.deltaTime * 10f);
    // }


    // void PredictMovement() {
    //     if (isPanning) {
    //         updateLocal++;
    //         Debug.Log($"Update local: {updateLocal}");
    //         predictedPosition += state.direction * moveSpeed * Time.deltaTime;
    //         transform.position = predictedPosition;
    //     }
    // }

    // public void Reconciliate(TestManager.GameState.Egg serverEgg) {
    //     state.Copy(other: serverEgg);
    //     float positionError = Vector3.Distance(predictedPosition, state.position);

    //     if (positionError > 0.1f) {
    //         updateFromServer++;
    //         Debug.Log($"Update from server: {updateFromServer}");
    //         transform.position = state.position;
    //         predictedPosition = state.position;
    //     }

    //     if (state.status == "hit") {
    //         Explode();
    //     }
    // }


    // [ServerRpc(RequireOwnership = false)]
    // void SendMoveDirectionToServerRpc(Vector3 direction) {
    //     state.direction = direction;
    //     transform.position += direction * moveSpeed * Time.deltaTime;
    //     state.position = transform.position;
    // }

    void Simulate() {
        Vector3 originalPos = transform.position;

        Physics2D.simulationMode = SimulationMode2D.Script;
        for (int i = 0; i < 35; i++) {
            Rigid.linearVelocity = new Vector2(0f, 2f);
            Physics2D.Simulate(Time.fixedDeltaTime);
        }
        Vector3 correctedPos = transform.position;
        transform.position = originalPos;
        Rigid.linearVelocity = Vector2.zero;

        Physics2D.simulationMode = SimulationMode2D.FixedUpdate;

        transform.position = correctedPos;
    }

    [ServerRpc(RequireOwnership = false)]
    void ShotServerRpc() {
        NetworkObject network = GetComponent<NetworkObject>();
        network.ChangeOwnership(NetworkManager.ServerClientId);
        // Rigid.linearVelocity = new Vector2(0f, 40f);

        Debug.Log("Start simulate");

        Simulate();
    }

    // IEnumerator SendPositionAndVelocityToClient() {
    //     while (isShotting) {
    //         transform.position += state.direction * moveSpeed * Time.deltaTime;
    //         state.position = transform.position;
    //         yield return new WaitForSeconds(0.1f);
    //     }
    // }

    private void HandlePanning() {
        // if (!IsOwner) {
        //     return;
        // }

        if (GameHelper.TouchBegin()) {
            Vector3 mousePos = Input.mousePosition;
            if (GameHelper.TouchHitGameObject(mousePos, gameObject)) {
                isPanning = true;
            }
            c_delta = transform.position - GameHelper.ToWorldPoint(mousePos);
        }

        if (GameHelper.TouchReleased()) {
            if (isPanning) {
                isPanning = false;
                Rigid.linearVelocity = Vector2.zero;
                Debug.Log($"Before shot: {Time.time}");
                // ShotServerRpc();
                Simulate();
            }
            return;
        }

        if (isPanning) {
            Vector3 mousePos = GameHelper.ToWorldPoint(Input.mousePosition) + c_delta;

            if (c_delta != Vector3.zero) {
                c_delta = Vector3.zero;
            }

            float distance = Vector2.Distance(mousePos, transform.position);
            if (distance <= 0.1f) {
                // state.direction = Vector3.zero;
                // SendMoveDirectionToServerRpc(state.direction);
                Rigid.linearVelocity = Vector2.zero;
            }
            else {
                // state.direction = new Vector3((mousePos - transform.position).normalized.x, 0f, 0f);
                // SendMoveDirectionToServerRpc(state.direction);

                float directionX = (mousePos - transform.position).normalized.x;
                Rigid.linearVelocity = new Vector2(directionX * 5f, 0f);
            }

            // Predict
            // updateLocal++;
            // Debug.Log($"Update local: {updateLocal}");
            // predictedPosition += state.direction * moveSpeed * Time.deltaTime;
            // transform.position = predictedPosition;
        }
    }
}
