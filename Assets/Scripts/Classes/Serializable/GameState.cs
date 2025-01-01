using System;


[Serializable]
public class FindingMatchState {
    public int secondsElapsed;
}



[Serializable]
public class MatchState {
    public string id;

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
