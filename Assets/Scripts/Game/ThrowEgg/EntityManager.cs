using UnityEngine;
using Unity.Netcode;

public abstract class EntityManager : NetworkBehaviour {
    protected ThrowEggLogic throwEggLogic;
    protected bool panable = false;
    protected bool isPanning = false;
}
