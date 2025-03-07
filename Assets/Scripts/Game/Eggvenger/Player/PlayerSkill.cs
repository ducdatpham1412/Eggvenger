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
    public BaseSkill FirstSkill;
    public BaseSkill SecondSkill;
    public BaseSkill Ulti; // TODO: Develop later

    [Header("Stats")]
    public Vector2 direction = Vector2.right;
    public int firstSkillNumber = 0;
    public int secondSkillNumber = 0;
    [SerializeField] GunStats CurrentGun;

    [Header("Runtime Value")]
    public BaseSkill CurrentSkill;
    public PlayerMovement PlayerMovement;
    public PlayerManager PlayerManager;
    public EggvengerManager Manager;

    bool isBursting = false;
    SpriteRenderer GunRenderer;
    AudioSource GunAudio;
    AudioSource PlayerAudio;
    PlayerGamepad Gamepad;
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
            Manager.GetComponent<PlayerShopping>().SetPlayerSkill(this);
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

    /*
    Guns
    */
    void ResetGuns(bool playSound = true) {
        for (int i = 0; i < Guns.Length; i++) {
            if (Guns[i] != null) {
                RoundGuns[i] = Instantiate(Guns[i]);
            }
            else {
                RoundGuns[i] = null;
            }
        }
        if (CurrentGun != null) {
            GunStats FindGun = Array.Find(RoundGuns, g => g != null && g.id == CurrentGun.id);
            EquipGun(FindGun ?? RoundGuns[0], playSound);
        }
        else {
            EquipGun(RoundGuns[0], playSound);
        }
    }

    public void BuySellGun(GunStats gun) {
        GunStats FindGun = Array.Find(Guns, g => g != null && g.id == gun.id);

        // Sell gun
        if (FindGun != null) {
            SoundManager.Instance.PlaySF(SoundManager.SF.Sell);
            int index = Array.IndexOf(Guns, FindGun);
            Guns[index] = null;
            PlayerManager.eggs += FindGun.sellPrice;
            CurrentGun = null;
            ResetGuns(playSound: false);
            return;
        }

        // Buy gun
        int eggsRemaining = PlayerManager.eggs - gun.price;
        if (eggsRemaining >= 0) {
            if (Guns[1] != null) {
                // TODO: Throw this gun for other player collect
            }

            Guns[1] = gun;
            CurrentGun = gun;
            PlayerManager.eggs -= gun.price;
            ResetGuns();
        }
    }

    public void ReloadGun() {
        if (reloadCoroutine != null || (CurrentGun.numberBullets <= 0 && CurrentGun.isLimitBullets)) return;

        void Reload() {
            if (CurrentGun.isLimitBullets) {
                int bulletsNeedMore = CurrentGun.magazineSize - CurrentGun.currentBullets;
                int bulletsActuallyAvailable = Math.Min(bulletsNeedMore, CurrentGun.numberBullets);
                CurrentGun.currentBullets += bulletsActuallyAvailable;
                CurrentGun.numberBullets -= bulletsActuallyAvailable;
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

    /*
    Skills
    */
    public void BuySkill(BaseSkill skill) {
        if (PlayerManager.eggs < skill.price || CheckSkillFull(skill)) return;
        SoundManager.Instance.PlaySF(SoundManager.SF.Buy);
        PlayerManager.eggs -= skill.price;
        if (skill == FirstSkill) {
            firstSkillNumber += 1;
        }
        else if (skill == SecondSkill) {
            secondSkillNumber += 1;
        }
    }

    public bool CheckSkillFull(BaseSkill skill) {
        if (skill == FirstSkill) {
            return firstSkillNumber >= skill.maxNumber;
        }
        if (skill == SecondSkill) {
            return secondSkillNumber >= skill.maxNumber;
        }
        throw new Exception("Can not check skill is full or not");
    }

    /*
    Coroutines
    */
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
                GunAudio.PlayOneShot(Manager.OutOfAmmo);
            }
            yield break;
        }

        isBursting = true;

        CheckToStopReload();

        if (forceOneShot || !CurrentGun.holdToBurst) {
            yield return StartCoroutine(Fire());
        }
        else {
            while (Gamepad.isHolding && CurrentGun.currentBullets > 0) {
                yield return StartCoroutine(Fire());
            }
        }
        isBursting = false;
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
            yield break;
        }
        if (CurrentGun.burstDelay >= 0.5f) {
            StartCoroutine(Gamepad.Countdown(Gamepad.ShotCountdown, CurrentGun.burstDelay));
        }
        yield return new WaitForSeconds(CurrentGun.burstDelay);
    }

    void EquipGun(GunStats gun, bool playSound = true) {
        CurrentGun = gun;

        if (CurrentGun.EquipSound && playSound) {
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

    bool CheckSkill(BaseSkill Skill) {
        if (Skill == null) {
            return true;
        }

        if (Skill == FirstSkill && firstSkillNumber <= 0) return true;
        if (Skill == SecondSkill && secondSkillNumber <= 0) return true;

        if (Gamepad.isHolding) Gamepad.isHolding = false;

        if (CurrentSkill == null || CurrentSkill.id != Skill.id) {
            GunTransform.gameObject.SetActive(false);
            if (CurrentSkill != null) {
                Destroy(CurrentSkill.gameObject);
            }
            CurrentSkill = Instantiate(Skill.gameObject, GetSkillSpawnPos(isLocal: false), Quaternion.identity).GetComponent<BaseSkill>();
            CurrentSkill.SetCreator(PlayerManager);
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
        if (CurrentSkill.id == FirstSkill.id) {
            firstSkillNumber -= 1;
        }
        else if (CurrentSkill.id == SecondSkill.id) {
            secondSkillNumber -= 1;
        }
        CurrentSkill.Play(GetDirection());
        BackToGunFromSkill(false);
        PlayerShopping shopping = Manager.GetComponent<PlayerShopping>();
        foreach (var dot in shopping.FirstSkillDots) {
            dot.UpdateActiveDots(firstSkillNumber);
        }
        foreach (var dot in shopping.SecondSkillDots) {
            dot.UpdateActiveDots(secondSkillNumber);
        }
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

