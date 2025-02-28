using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {
    public float originalSpeed = 6.5f;
    public float buffSpeed = 0f;
    public List<float> speedRatios = new List<float>();
    public float currentSpeed = 6.5f;

    public string id;
    public int team;

    public int heal = 1000;
    public int shield = 0; // percentage: 0 - 100%

    public bool IsOwner; // TODO: Only for test, remove it
}
