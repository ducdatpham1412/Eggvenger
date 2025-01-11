
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


[Serializable]
public class ThrowEggState {
    public class Player {
        public string id;
        public string name;
        public string avatar;
        public float move_speed;
        public float shot_speed;
        public int point;
        public Vector2 init_pos;
        public ulong? clientID;
    }

    [Serializable]
    public class Egg {
        public string id;
        public float move_speed;
        public float shot_speed;
        public float created;
        public string creator;
        public string status; // active, hit, missed
    }

    [Serializable]
    public class Data {
        public string turn;
        public Dictionary<string, Player> players;
        public Dictionary<string, Egg> eggs;

    }

    public string id;
    public string type;
    public Data data;
    public float created;
    public float? ended;
    public string status; // active, ended
}




[Serializable]
public class EggNetwork : INetworkSerializable, IEquatable<EggNetwork> {
    public string id;
    public float move_speed;
    public float shot_speed;
    public float created;
    public string creator;
    public string status; // active, hit, missed

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        if (serializer.IsReader) {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out id);
            reader.ReadValueSafe(out move_speed);
            reader.ReadValueSafe(out shot_speed);
            reader.ReadValueSafe(out created);
            reader.ReadValueSafe(out creator);
            reader.ReadValueSafe(out status);
        }
        else {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(id);
            writer.WriteValueSafe(move_speed);
            writer.WriteValueSafe(shot_speed);
            writer.WriteValueSafe(created);
            writer.WriteValueSafe(creator);
            writer.WriteValueSafe(status);
        }
    }

    public bool Equals(EggNetwork other) {
        return move_speed == other.move_speed && shot_speed == other.shot_speed && status == other.status;
    }
}