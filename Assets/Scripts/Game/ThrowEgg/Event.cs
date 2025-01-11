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
        public class LoadMatch {
            public string type = "load_match";
            public string match_id;
        }
    }
}
