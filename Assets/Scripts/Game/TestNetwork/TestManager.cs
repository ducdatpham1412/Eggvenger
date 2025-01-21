using System;
using Unity.Netcode;
using UnityEngine;


public class TestManager : NetworkBehaviour {
    // [Serializable]
    // public class GameState : INetworkSerializable, IEquatable<GameState> {
    //     public class Egg : INetworkSerializable, IEquatable<Egg> {
    //         public Vector3 direction;
    //         public Vector3 position;
    //         public string status; // active, hit

    //         public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
    //             if (serializer.IsWriter) {
    //                 var writer = serializer.GetFastBufferWriter();
    //                 writer.WriteValueSafe(direction);
    //                 writer.WriteValueSafe(position);
    //                 writer.WriteValueSafe(status);
    //             }
    //             else {
    //                 var reader = serializer.GetFastBufferReader();
    //                 reader.ReadValueSafe(out direction);
    //                 reader.ReadValueSafe(out position);
    //                 reader.ReadValueSafe(out status);
    //             }
    //         }


    //         public void Copy(Egg other) {
    //             direction = other.direction;
    //             position = other.position;
    //             status = other.status;
    //         }

    //         public bool Equals(Egg other) {
    //             return direction.Equals(other.direction) && position.Equals(other.position) && status == other.status;
    //         }
    //     }

    //     public Egg egg;



    //     public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
    //         if (serializer.IsWriter) {
    //             serializer.SerializeValue(ref egg);
    //         }
    //         else {
    //             if (egg == null) {
    //                 egg = new Egg();
    //             }
    //             serializer.SerializeValue(ref egg);
    //         }
    //     }

    //     public bool Equals(GameState other) {
    //         return true;
    //     }
    // }

    // public GameState gameState = new GameState {
    //     egg = new GameState.Egg {
    //         direction = Vector3.zero,
    //         status = "active",
    //     },
    // };

    // [ClientRpc]
    // public void PassStateClientRpc(GameState _newState) {
    //     // eggManager.Reconciliate(_newState.egg);
    // }

    public class GameStateSnapShot {
        public class Value {
            public Vector3 position;
            public Vector3 velocity;
        }
        public Value Egg;
        public Value Player;
        public long Timestamp;
    }


    public class GameStateBuffer {
        private GameStateSnapShot[] snapshots;
        private int currentIndex;
        private int capacity;

        public GameStateBuffer(int capacity) {
            this.capacity = capacity;
            snapshots = new GameStateSnapShot[capacity];
            currentIndex = 0;
        }

        public void AddSnapshot(GameStateSnapShot snapshot) {
            snapshots[currentIndex] = snapshot;
            currentIndex = (currentIndex + 1) % capacity;
        }

        public GameStateSnapShot GetSnapshot(long timestamp) {
            for (int i = 0; i < capacity; i++) {
                if (snapshots[i] != null && snapshots[i].Timestamp <= timestamp) {
                    return snapshots[i];
                }
            }
            return null;
        }
    }


    public TestEggManager eggManager;
    public PlayerManager playerManager;

    GameStateBuffer gameStateBuffer = new GameStateBuffer(1024);


    [ServerRpc(RequireOwnership = false)]
    public void ShotServerRpc(long timestamp) {
        GameStateSnapShot snapShot = gameStateBuffer.GetSnapshot(timestamp);
        if (snapShot == null) return;


    }


    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        if (IsServer) {
            NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientID) => {
                eggManager.gameObject.GetComponent<NetworkObject>().ChangeOwnership(clientID);
            };
        }
    }
}
