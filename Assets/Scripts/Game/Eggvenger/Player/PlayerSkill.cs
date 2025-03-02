using System;
using System.Collections;
using UnityEngine;

public class PlayerSkill : MonoBehaviour {
    [Header("GameObjects")]
    public BulletTrajectory BulletTrajectory;
    [SerializeField] Transform GunTransform;
    [SerializeField] Transform GunHead;
    [SerializeField] BulletPool Pool;
    public GunStats[] Guns = new GunStats[2];

    [Header("Skills")]
    public GameObject FirstSkill;
    public GameObject SecondSkill;
    public GameObject Ulti;

    [Header("Stats")]
    [SerializeField] Vector2 direction;
    [SerializeField] GunStats CurrentGun;

    BaseSkill CurrentSkill;
    PlayerManager manager;
    SpriteRenderer GunRenderer;
    AudioSource ShotAudio;
    Vector2 baseVector = new Vector2(1, 0);
    Vector2 lastDirection;
    bool isHolding = false;
    bool isBursting = false;


    void Start() {
        manager = GetComponent<PlayerManager>();
        GunRenderer = GunTransform.GetComponent<SpriteRenderer>();
        ShotAudio = GetComponent<AudioSource>();
        EquipGun(Guns[0]);
    }

    void Update() {
        if (manager.IsOwner) {
            direction = GetDirection();
        }
        HandleGunDirection();
        HandleShot();
        HandleSkills();
    }

    void HandleShot() {
        if (!manager.IsOwner || CurrentSkill != null) return;

        if (Input.GetMouseButtonDown(0)) {
            isHolding = true;
            StartCoroutine(Burst());
        }

        if (Input.GetMouseButtonUp(0)) {
            isHolding = false;
        }
    }

    IEnumerator Burst() {
        if (isBursting) yield break;

        isBursting = true;

        if (CurrentGun.holdToBurst) {
            while (isHolding) {
                yield return StartCoroutine(Fire());
            }
        }
        else {
            yield return StartCoroutine(Fire());
        }
        isBursting = false;
        yield return null;
    }

    IEnumerator Fire() {
        if (CurrentGun.burstAmount == 1) {
            ShotAudio.Play();
            Bullet bullet = Pool.GetBullet();
            bullet.transform.position = GunHead.position;
            bullet.transform.rotation = GunTransform.rotation;
        }
        else if (CurrentGun.burstAmount > 1) {
            ShotAudio.Play();
            Bullet[] bullets = Pool.GetBullets(CurrentGun.burstAmount);
            foreach (var b in bullets) {
                b.gameObject.SetActive(true);
                b.transform.position = GunHead.position;
                b.transform.rotation = GunTransform.rotation;
                yield return new WaitForSeconds(CurrentGun.fireDelay);
            }
        }
        yield return new WaitForSeconds(CurrentGun.burstDelay);
    }

    void EquipGun(GunStats gun) {
        CurrentGun = gun;

        if (CurrentGun.EquipSound != null) {
            ShotAudio.PlayOneShot(CurrentGun.EquipSound);
        }

        ShotAudio.clip = CurrentGun.ShotSound;
        GunRenderer.sprite = CurrentGun.Gun;
        Pool.FillPool(CurrentGun);

        Vector3 size = GunRenderer.sprite.bounds.size;
        float pivotX = GunRenderer.sprite.pivot.x / GunRenderer.sprite.rect.width;
        float maxX = size.x * (1 - pivotX);
        GunHead.localPosition = new Vector2(maxX, 0f);
    }

    void BackToGunFromSkill(bool playSound) {
        BulletTrajectory.RemoveLine();
        CurrentSkill = null;
        GunTransform.gameObject.SetActive(true);
        if (playSound) {
            ShotAudio.PlayOneShot(CurrentGun.EquipSound);
        }
    }

    void HandleGunDirection() {
        if (Equals(direction, lastDirection)) return;
        lastDirection = direction;

        // Check rotation
        float degree = Vector2.SignedAngle(baseVector, direction);
        GunTransform.rotation = Quaternion.Euler(0, 0, degree);

        // Check flipY
        degree = Math.Abs(degree);
        GunRenderer.flipY = degree > 90;

        // Check scale
        if (degree > 90) {
            degree = degree - (degree - 90) * 2;
        }
        float scaleX = Mathf.Lerp(1, 0.75f, degree / 90);
        float scaleY = Mathf.Lerp(1, 0.6f, degree / 90);
        GunTransform.localScale = new Vector3(scaleX, scaleY, 1f);
    }

    void HandleSkills() {
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
            BulletTrajectory.DrawStraight(direction);
            if (Input.GetMouseButtonDown(0)) {
                PlaySkill(direction);
            }
        }
    }

    public Vector3 GetSkillSpawnPos(bool isLocal = true) {
        Vector3 size = gameObject.GetComponent<SpriteRenderer>().bounds.size;
        if (isLocal) return new Vector3(0f, size.y * 1 / 4.5f, 0f);
        return new Vector3(transform.position.x, transform.position.y + size.y * 1 / 4.5f, 0f);
    }

    bool CheckSkill(GameObject Skill) {
        if (Skill == null) {
            return true;
        }

        if (isHolding) isHolding = false;

        if (CurrentSkill == null || CurrentSkill.OriginalPrefab != Skill) {
            GunTransform.gameObject.SetActive(false);
            if (CurrentSkill != null) {
                Destroy(CurrentSkill.gameObject);
            }
            CurrentSkill = Instantiate(Skill, GetSkillSpawnPos(isLocal: false), Quaternion.identity).GetComponent<BaseSkill>();
            CurrentSkill.Creator = manager;
            CurrentSkill.OriginalPrefab = Skill;
            if (!CurrentSkill.canReady) {
                PlaySkill(direction);
                return true;
            }
            CurrentSkill.Ready(direction);
            return false;
        }

        // If CurrentSkill == Skill => Turn off skill and switch back to Shotting mode
        Destroy(CurrentSkill.gameObject);
        BackToGunFromSkill(true);
        return true;
    }

    void PlaySkill(Vector3 direction) {
        CurrentSkill.Play(direction);
        BackToGunFromSkill(false);
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

