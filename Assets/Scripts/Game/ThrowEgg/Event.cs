using System;

[Serializable]
public class Event {
    public interface IEvent {
        public string type { get; set; }
    }

    public class Send {
        public class Connect {
            public string type = "connect";
            public string player_id;
        }
        public class FindMatch {
            public string type = "find_match";
        }
        public class StopFindMatch {
            public string type = "stop_find_match";
        }
    }

    public class Receive {
        [Serializable]
        public class MatchFound {
            public string type = "match_found";
            public MatchState data;
        }
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
