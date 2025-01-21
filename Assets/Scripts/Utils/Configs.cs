using System;


public class Configs {
    public static _Color Color = new _Color {
        brown = "#4E3200",
        green = "#007427",
        green01 = "#52E032",
        golden = "#FFEEA3",
        silver = "#D1D1D1",
        bronze = "#FFC393",
        yellow = "#FFCE45",
    };

    public static _LocaleID LocaleID = new _LocaleID {
        En = "en",
        Vi = "vi",
    };

    public static _Env Env = new _Env {
        match_id = GetEnv("match_id"),
        match_port = GetEnv("match_port"),
    };


    static string GetEnv(string key) {
        try {
            return Environment.GetEnvironmentVariable(key);
        }
        catch (Exception) {
            UnityEngine.Debug.LogWarning($"No env found: {key}");
            return null;
        }
    }


    [Serializable]
    public class _Color {
        public string brown;
        public string green;
        public string green01;
        public string golden;
        public string silver;
        public string bronze;
        public string yellow;
    }

    [Serializable]
    public class _LocaleID {
        public string En;
        public string Vi;
    }

    [Serializable]
    public class _Env {
        public string match_id;
        public string match_port;
    }
}
