using System;

[Serializable]
public class Event {
    [Serializable]
    public class Ready {
        public string type = "ready";
    }

    [Serializable]
    public class LoadRoom {
        public string type = "load_room";
        public string match_id;
        public string room_id;
    }

    [Serializable]
    public class Connect {
        public string type = "connect";
        public string player_id;
    }

    [Serializable]
    public class TurnResult {
        public string type = "turn_res";
        public string match_id;
        public string room_id;
        public string egg_id;
        public string status;
    }

    [Serializable]
    public class EggMove {
        public string type = "egg_move";
        public string match_id;
        public string room_id;
        public string id;
        public Coordinate velocity;
    }

    [Serializable]
    public class Shot {
        public string type = "shot";
        public string match_id;
        public string room_id;
        public string egg_id;
    }
}
