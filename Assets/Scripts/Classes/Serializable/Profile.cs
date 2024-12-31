using System;



[Serializable]
public class Authorized {
    public string username;
    public string password;
    public string token;
    public string refresh_token;
}

[Serializable]
public class Profile {
    public string name;
    public string avatar;
    public int eggs;
    public int ranking;
    public int localeID;
}

