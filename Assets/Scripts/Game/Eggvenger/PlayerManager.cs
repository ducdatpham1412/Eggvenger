using UnityEngine;

public class PlayerManager : MonoBehaviour {

    [Header("GameObjects")]
    public BulletTrajectory BulletTrajectory;

    [Header("Skills")]
    public BaseSkill FirstSkill;
    public BaseSkill SecondSkill;
    public BaseSkill Ulti;

    [Header("Properties")]
    public int team;

    BaseSkill CurrentSkill;

    void Update() {
        if (!BulletTrajectory) {
            return;
        }

        if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0)) {
            Vector3 direction = GetDirection();
            BulletTrajectory.DrawStraight(transform.position, direction, radius: 5f);
            CurrentSkill = FirstSkill;
            return;
        }

        if (Input.GetMouseButtonUp(0)) {
            BulletTrajectory.RemoveLine();
            if (CurrentSkill) {
                Vector3 direction = GetDirection();
                CurrentSkill.Play(transform.position, direction);
            }
            return;
        }
    }

    Vector3 GetDirection() {
        Vector3 startPos = transform.position;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y));
        startPos.z = 0f;
        mouseWorldPos.z = 0f;
        Vector3 direction = (mouseWorldPos - startPos).normalized;
        return direction;
    }

    void Shot() {

    }
}
