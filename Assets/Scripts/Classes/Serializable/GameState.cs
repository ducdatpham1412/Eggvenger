using System;


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
