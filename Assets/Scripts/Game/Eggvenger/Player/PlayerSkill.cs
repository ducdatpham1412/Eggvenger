using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkill : MonoBehaviour {
    [Header("GameObjects")]
    [SerializeField] Transform GunTransform;
    public Transform GunHead;
    [SerializeField] BulletPool Pool;
    public GunStats[] Guns = new GunStats[2];
    [SerializeField] GunStats[] RoundGuns = new GunStats[2];

    [Header("Skills")]
    public GameObject FirstSkill;
    public GameObject SecondSkill;
    public GameObject Ulti;

    [Header("Stats")]
    public Vector2 direction = Vector2.right;
    [SerializeField] GunStats CurrentGun;

    public BaseSkill CurrentSkill;
    public PlayerMovement PlayerMovement;
    public EggvengerManager Manager;

    bool isBursting = false;
    SpriteRenderer GunRenderer;
    AudioSource GunAudio;
    AudioSource PlayerAudio;
    PlayerGamepad Gamepad;
    PlayerManager PlayerManager;
    Vector2 baseVector = Vector2.right;
    Vector2 lastDirection;
    Coroutine reloadCoroutine;

    void Start() {
        PlayerManager = GetComponent<PlayerManager>();
        PlayerMovement = GetComponent<PlayerMovement>();
        GunRenderer = GunTransform.GetComponent<SpriteRenderer>();
        GunAudio = GunHead.GetComponent<AudioSource>();
        PlayerAudio = GetComponent<AudioSource>();
        if (PlayerManager.IsOwner) {
            Gamepad = Manager.GetComponent<PlayerGamepad>();
            Gamepad.SetPlayerSkill(this);
        }
        ResetGuns();
    }

    // TODO: Assign PlayerGamepad in OnNetworkSpawn

    void Update() {
        HandleGunDirection();
    }

    public void ChangeGun() {
        int index = Array.IndexOf(RoundGuns, CurrentGun);
        index = (index + 1) % RoundGuns.Length;
        if (RoundGuns[index] != null) {
            CheckToStopReload();
            EquipGun(RoundGuns[index]);
        }
    }

    public void PlayHit(bool forceOneShot) {
        if (CurrentSkill == null) {
            StartCoroutine(Burst(forceOneShot));
        }
        else {
            PlaySkill();
        }
    }

    public void PressSkill(int index) {
        bool played = false;

        if (index == 0) {
            played = CheckSkill(FirstSkill);
        }

        if (index == 1) {
            played = CheckSkill(SecondSkill);
        }

        if (index == 2) {
            played = CheckSkill(Ulti);
        }

        if (!played) {
            CheckToStopReload();
        }
    }

    public Vector3 GetSkillSpawnPos(bool isLocal = true) {
        Vector3 size = gameObject.GetComponent<SpriteRenderer>().bounds.size;
        if (isLocal) return new Vector3(0f, size.y * 1 / 4.5f, 0f);
        return new Vector3(transform.position.x, transform.position.y + size.y * 1 / 4.5f, 0f);
    }

    public void ResetGuns() {
        for (int i = 0; i < Guns.Length; i++) {
            if (Guns[i] != null) {
                RoundGuns[i] = Instantiate(Guns[i]);
            }
            else {
                RoundGuns[i] = null;
            }
        }
        if (CurrentGun != null) {
            GunStats FindGun = Array.Find(RoundGuns, g => g.id == CurrentGun.id);
            if (FindGun != null) {
                EquipGun(FindGun);
            }
            else {
                EquipGun(RoundGuns[0]);
            }
        }
        else {
            EquipGun(RoundGuns[0]);
        }
    }

    public void BuyGun() {

    }

    public void ReloadGun() {
        if (reloadCoroutine != null) return;

        void Reload() {
            if (CurrentGun.isLimitBullets) {
                int minusBullets = CurrentGun.magazineSize - CurrentGun.currentBullets;
                CurrentGun.currentBullets = Math.Min(CurrentGun.magazineSize, CurrentGun.numberBullets);
                CurrentGun.numberBullets -= minusBullets;
                Gamepad.TextRemainingBullets.text = CurrentGun.numberBullets.ToString();
            }
            else {
                CurrentGun.currentBullets = CurrentGun.magazineSize;
            }
            Gamepad.TextCurrentBullets.text = CurrentGun.currentBullets.ToString();
            reloadCoroutine = null;
        }

        if (CurrentGun.ReloadSound != null) {
            GunAudio.PlayOneShot(CurrentGun.ReloadSound);
        }
        reloadCoroutine = StartCoroutine(Gamepad.Countdown(Gamepad.ReloadCountdown, CurrentGun.reloadDelay, Reload));
    }

    IEnumerator DrawSkillTrajectoryUntilHolding() {
        while (!Gamepad.isHolding && CurrentSkill != null) {
            Gamepad.BulletTrajectory.DrawStraight(CurrentSkill.transform.position, GetDirection(), radius: 8f);
            yield return null;
        }
    }

    IEnumerator Burst(bool forceOneShot = false) {
        if (isBursting) yield break;

        if (CurrentGun.currentBullets <= 0) {
            if (!GunAudio.isPlaying) {
                GunAudio.PlayOneShot(CurrentGun.ReloadSound);
            }
            yield break;
        }

        isBursting = true;

        CheckToStopReload();

        if (forceOneShot || !CurrentGun.holdToBurst) {
            yield return StartCoroutine(Fire());
        }
        else {
            while (Gamepad.isHolding) {
                yield return StartCoroutine(Fire());
            }
        }
        isBursting = false;
        yield return null;
    }

    IEnumerator Fire() {
        int burstAmount = 1;
        if (CurrentGun.burstAmount == 1) {
            GunAudio.Play();
            Bullet bullet = Pool.GetBullet();
            bullet.transform.position = GunHead.position;
            bullet.transform.rotation = GunTransform.rotation;
        }
        else if (CurrentGun.burstAmount > 1) {
            burstAmount = Math.Min(CurrentGun.burstAmount, CurrentGun.currentBullets);
            GunAudio.Play();
            Bullet[] bullets = Pool.GetBullets(burstAmount);
            foreach (var b in bullets) {
                b.gameObject.SetActive(true);
                b.transform.position = GunHead.position;
                b.transform.rotation = GunTransform.rotation;
                yield return new WaitForSeconds(CurrentGun.fireDelay);
            }
        }
        CurrentGun.currentBullets -= burstAmount;
        Gamepad.TextCurrentBullets.text = CurrentGun.currentBullets.ToString();
        if (CurrentGun.currentBullets <= 0) {
            ReloadGun();
        }
        if (CurrentGun.burstDelay >= 0.5f) {
            StartCoroutine(Gamepad.Countdown(Gamepad.ShotCountdown, CurrentGun.burstDelay));
        }
        yield return new WaitForSeconds(CurrentGun.burstDelay);
    }

    void EquipGun(GunStats gun) {
        CurrentGun = gun;

        if (CurrentGun.EquipSound != null) {
            GunAudio.PlayOneShot(CurrentGun.EquipSound);
        }

        GunAudio.clip = CurrentGun.ShotSound;
        GunRenderer.sprite = CurrentGun.Gun;
        Pool.FillPool(CurrentGun);

        Vector3 size = GunRenderer.sprite.bounds.size;
        float pivotX = GunRenderer.sprite.pivot.x / GunRenderer.sprite.rect.width;
        float maxX = size.x * (1 - pivotX);
        GunHead.localPosition = new Vector2(maxX, 0f);

        if (Gamepad != null) {
            Gamepad.CurrentGunUI.sprite = CurrentGun.GunUI != null ? CurrentGun.GunUI : CurrentGun.Gun;
            Gamepad.BulletUI.GetComponent<Image>().sprite = Gamepad.BulletSprite;
            Gamepad.TextCurrentBullets.text = CurrentGun.currentBullets.ToString();
            Gamepad.TextRemainingBullets.text = CurrentGun.isLimitBullets ? CurrentGun.numberBullets.ToString() : "";
        }
    }

    void BackToGunFromSkill(bool playSound) {
        if (Gamepad != null) {
            Gamepad.BulletTrajectory.RemoveLine();
            Gamepad.BulletUI.GetComponent<Image>().sprite = Gamepad.BulletSprite;
        }
        CurrentSkill = null;
        GunTransform.gameObject.SetActive(true);
        if (playSound) {
            GunAudio.PlayOneShot(CurrentGun.EquipSound);
        }
    }

    void HandleGunDirection() {
        if (!PlayerManager.IsOwner) return;

        Vector2 temp = GetDirection();
        if (Equals(temp, lastDirection)) return;
        lastDirection = temp;

        // Check rotation
        float degree = Vector2.SignedAngle(baseVector, temp);
        Quaternion newRotation = Quaternion.Euler(0, 0, degree);
        GunTransform.rotation = newRotation;
        if (Gamepad != null) {
            Gamepad.BulletUI.rotation = newRotation;
        }
        if (CurrentSkill != null) {
            CurrentSkill.transform.rotation = newRotation;
        }

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

    bool CheckSkill(GameObject Skill) {
        if (Skill == null) {
            return true;
        }

        if (Gamepad.isHolding) Gamepad.isHolding = false;

        if (CurrentSkill == null || CurrentSkill.OriginalPrefab != Skill) {
            GunTransform.gameObject.SetActive(false);
            if (CurrentSkill != null) {
                Destroy(CurrentSkill.gameObject);
            }
            CurrentSkill = Instantiate(Skill, GetSkillSpawnPos(isLocal: false), Quaternion.identity).GetComponent<BaseSkill>();
            CurrentSkill.Creator = PlayerManager;
            CurrentSkill.OriginalPrefab = Skill;
            if (!CurrentSkill.canReady) {
                PlaySkill();
                return true;
            }
            if (CurrentSkill.SkillSprite != null && Gamepad != null) {
                Gamepad.BulletUI.GetComponent<Image>().sprite = CurrentSkill.SkillSprite;
            }
            CurrentSkill.Ready(direction);
            StartCoroutine(DrawSkillTrajectoryUntilHolding());
            return false;
        }

        // If CurrentSkill == Skill => Turn off skill and switch back to Shotting mode
        Destroy(CurrentSkill.gameObject);
        BackToGunFromSkill(true);
        return true;
    }

    void PlaySkill() {
        CurrentSkill.Play(GetDirection());
        BackToGunFromSkill(false);
    }

    Vector2 GetDirection() {
        return direction == Vector2.zero ? (PlayerMovement.moveDirection == Vector2.zero ? PlayerMovement.lastDirection : PlayerMovement.moveDirection) : direction;
    }

    void CheckToStopReload() {
        if (reloadCoroutine != null) {
            StopCoroutine(reloadCoroutine);
            Gamepad.ReloadCountdown.fillAmount = 0;
            reloadCoroutine = null;
        }
    }
}

