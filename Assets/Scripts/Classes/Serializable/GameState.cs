using System;


[Serializable]
public class FindingMatchState {
    public int secondsElapsed;
}



[Serializable]
public class InGameState {
    public int secondsElapsed;
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
    public object data; // typeof data = FindingMatchState | InGameState
}
