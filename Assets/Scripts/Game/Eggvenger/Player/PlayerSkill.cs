using UnityEngine;

public class PlayerSkill : MonoBehaviour {
    [Header("GameObjects")]
    public BulletTrajectory BulletTrajectory;

    [Header("Skills")]
    public GameObject FirstSkill;
    public GameObject SecondSkill;
    public GameObject Ulti;

    BaseSkill CurrentSkill;
    PlayerManager manager;

    void Start() {
        manager = GetComponent<PlayerManager>();
    }

    void Update() {
        if (!manager.IsOwner) return;

        if (Input.GetKeyDown(KeyCode.C)) {
            bool played = CheckSkill(FirstSkill);
            if (played) {
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.Q)) {
            bool played = CheckSkill(SecondSkill);
            if (played) {
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.E)) {
            bool played = CheckSkill(Ulti);
            if (played) {
                return;
            }
        }

        if (CurrentSkill) {
            Vector3 direction = GetDirection();
            BulletTrajectory.DrawStraight(direction, radius: 5f);
            if (Input.GetMouseButtonDown(0)) {
                PlaySkill(direction);
            }
        }
    }

    public Vector3 GetSkillSpawnPos(bool isLocal = true) {
        Vector3 size = gameObject.GetComponent<SpriteRenderer>().bounds.size;
        Debug.Log("Size: " + size);
        if (isLocal) return new Vector3(0f, size.y * 1 / 4.5f, 0f);
        return new Vector3(transform.position.x, transform.position.y + size.y * 1 / 4.5f, 0f);
    }

    bool CheckSkill(GameObject Skill) {
        if (Skill == null) {
            return true;
        }

        if (CurrentSkill != Skill && CurrentSkill != null) {
            Destroy(CurrentSkill.gameObject);
        }
        CurrentSkill = Instantiate(Skill, GetSkillSpawnPos(isLocal: false), Quaternion.identity).GetComponent<BaseSkill>();
        CurrentSkill.Creator = manager;
        Vector3 direction = GetDirection();
        if (!CurrentSkill.canReady) {
            PlaySkill(direction);
            return true;
        }
        CurrentSkill.Ready(direction);
        return false;
    }

    void PlaySkill(Vector3 direction) {
        CurrentSkill.Play(direction);
        BulletTrajectory.RemoveLine();
        CurrentSkill = null;
    }

    Vector3 GetDirection() {
        Vector3 startPos = transform.position;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y));
        startPos.z = 0f;
        mouseWorldPos.z = 0f;
        Vector3 direction = (mouseWorldPos - startPos).normalized;
        return direction;
    }
}

