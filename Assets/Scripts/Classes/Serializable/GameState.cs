using System;
using Unity.Netcode;
using UnityEngine;


[Serializable]
public class FindingMatchState {
    public int secondsElapsed;
}


[Serializable]
public class MatchState {
    public class Configs {
        public float maxX;
        public float maxY;
        public string ip;
        public int port;
    }
    public string id;
    public Configs configs;
    public float created;
    public float? ended;
    public string status;
}


[Serializable]
public class GameState {
    public enum Status {
        none,
        findingMatch,
        loadingMatch,
        inGame,
    }
    public Status status;
    public object data; // typeof data = FindingMatchState | MatchState
}


[Serializable]
public class World {
    public float maxX;
    public float maxY;
}


[Serializable]
public class PlayerNetwork : INetworkSerializable, IEquatable<PlayerNetwork> {
    public string id;
    public string name;
    public string avatar;
    public float move_speed;
    public float shot_speed;
    public int point;
    public Vector2 init_pos;
    public long clientID; // init by -1 when it has not been Initialize

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        if (serializer.IsReader) {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out id);
            reader.ReadValueSafe(out name);
            reader.ReadValueSafe(out avatar);
            reader.ReadValueSafe(out move_speed);
            reader.ReadValueSafe(out shot_speed);
            reader.ReadValueSafe(out point);
            reader.ReadValueSafe(out init_pos);
            reader.ReadValueSafe(out clientID);
        }
        else {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(id);
            writer.WriteValueSafe(name);
            writer.WriteValueSafe(avatar);
            writer.WriteValueSafe(move_speed);
            writer.WriteValueSafe(shot_speed);
            writer.WriteValueSafe(point);
            writer.WriteValueSafe(init_pos);
            writer.WriteValueSafe(clientID);
        }
    }


    // public void SetName(string v) {
    //     name = v;
    //     OnUpdateName(name);
    // }

    // public void OnUpdateName(string v) {
    //     // trigger when update
    // }

    public bool Equals(PlayerNetwork other) {
        return move_speed == other.move_speed && shot_speed == other.shot_speed && point == other.point && clientID == other.clientID;
    }
}