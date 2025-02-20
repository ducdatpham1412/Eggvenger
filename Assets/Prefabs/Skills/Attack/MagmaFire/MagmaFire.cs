using UnityEngine;

public class MagmaFire : BaseSkill {
    // TODO: Do this
    [SerializeField] GameObject MagmaBall;
    [SerializeField] GameObject FireWall;

    Rigidbody2D rigidbody;

    float speed = 30f;

    void Awake() {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    void Start() {

    }

    void Update() {

    }

    public override void Play(Vector3 pos, Vector3 direction) {
        gameObject.SetActive(true);
        rigidbody.AddForce(direction.normalized * speed, ForceMode2D.Force);
    }
}
