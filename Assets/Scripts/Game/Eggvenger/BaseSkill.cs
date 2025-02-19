using UnityEngine;

public abstract class BaseSkill : MonoBehaviour {
    public abstract void Play(Vector3 pos, Vector3 direction);

    protected virtual void OnTriggerEnter2D(Collider2D collider) { }
}
