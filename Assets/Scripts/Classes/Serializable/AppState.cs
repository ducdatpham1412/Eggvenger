using System;

[Serializable]
public class Resource {
    public string[] backgrounds;
}


[Serializable]
public class Account {
    public string username;
    public string password;
    public string token;
    public string refresh_token;
}


[Serializable]
public class AppState {
    public Resource resource;
    public Profile profile;
    public Account account;
}
