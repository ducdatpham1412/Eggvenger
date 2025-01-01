using UnityEngine;

public abstract class EntityManager : MonoBehaviour {
    protected ThrowEggLogic throwEggLogic;
    protected bool panable = false;
    protected bool isPanning = false;
    protected abstract void OnUpdateState(ThrowEggState state);

    public void SetThrowEggLogic(ThrowEggLogic logic) {
        throwEggLogic = logic;
        throwEggLogic.OnUpdateState += OnUpdateState;
    }


    void OnDestroy() {
        if (throwEggLogic != null) {
            throwEggLogic.OnUpdateState -= OnUpdateState;
        }
    }
}
