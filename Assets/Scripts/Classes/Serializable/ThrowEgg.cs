
using System;
using System.Collections.Generic;


[Serializable]
public class Coordinate {
    public float x;
    public float y;
}


[Serializable]
public class ThrowEggState {
    [Serializable]
    public class Player {
        public string id;
        public Coordinate init_pos;
        public Coordinate last_pos;

        public Coordinate velocity;
        public float speed;

        public Coordinate egg_velocity;
        public float egg_speed;

        public int point;
        public string reaction;
        public string status; // active, ready, hit
    }

    [Serializable]
    public class Egg {
        public string id;
        public Coordinate last_pos;
        public Coordinate velocity;
        public float speed;
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
