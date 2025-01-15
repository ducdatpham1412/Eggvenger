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
    [Serializable]
    public class Setting {
        public string language;
    }

    public string id;
    public string name;
    public string avatar;
    public int eggs;
    public int ranking;
    public int localeID;
    public Setting setting;
}


[Serializable]
public class Passport {
    public Profile profile;
}
