using System;
using Unity.VisualScripting;
using UnityEngine;


[Serializable]
public class _Color {
    public string brown;
    public string green;
    public string golden;
    public string silver;
    public string bronze;
    public string yellow;
}
[Serializable]
public class _Configs {
    public _Color Color;
}

[Serializable]
public class _LocaleID {
    public int En;
    public int Vi;
}

public static class Configs {
    public static _Color Color = new _Color {
        brown = "#4E3200",
        green = "#007427",
        golden = "#FFEEA3",
        silver = "#D1D1D1",
        bronze = "#FFC393",
        yellow = "#FFCE45",
    };

    public static _LocaleID LocaleID = new _LocaleID {
        En = 0,
        Vi = 1,
    };
}
