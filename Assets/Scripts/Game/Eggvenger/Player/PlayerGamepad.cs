using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerGamepad : MonoBehaviour {
    [Header("GameObjects")]
    public BulletTrajectory BulletTrajectory;

    [Header("UI GameObjects")]
    public EventTrigger MoveControl;
    public RectTransform MoveIndicator;
    public EventTrigger ShotOutside;
    public EventTrigger ShotInside;
    public Image CurrentGunUI;
    public RectTransform BulletUI;
    public RectTransform FirstSkill;
    public RectTransform SecondSkill;
    public Sprite BulletSprite;

    [Header("Magazine Status")]
    public Text TextCurrentBullets;
    public Text TextRemainingBullets;

    [Header("Countdown")]
    public Image ReloadCountdown;
    public Image ShotCountdown;

    [Header("Player Components")]
    [SerializeField] PlayerSkill Skill;
    [SerializeField] PlayerMovement Movement;

    public bool isHolding = false;
    bool isAiming = false;
    Coroutine turnBackGunDirection;
    Coroutine moveIndicatorComeback;
    Vector2 moveIndicatorCenter;
    float maxRadius = 0f;

    void Start() {
        ShotInside.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.39f;
        ShotOutside.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.2f;
        RectTransform MoveControlRect = MoveControl.GetComponent<RectTransform>();
        Vector3 worldSize = Vector3.Scale(MoveControlRect.rect.size, MoveControlRect.lossyScale);
        moveIndicatorCenter = MoveIndicator.position;
        float radius = worldSize.x / 2;
        maxRadius = radius * 0.85f;
    }

    void OnDestroy() {
        ShotInside.triggers.Clear();
        ShotOutside.triggers.Clear();
        MoveControl.triggers.Clear();
    }

    IEnumerator BackToMoveDirection() {
        yield return new WaitForSeconds(0.3f);
        float duration = 0.2f;
        float elapsedTime = 0f;
        Vector2 curDir = Skill.direction;
        while (elapsedTime < duration) {
            Vector2 targetDir = Skill.PlayerManager.Movement.moveDirection == Vector2.zero ? Skill.PlayerManager.Movement.lastDirection : Skill.PlayerManager.Movement.moveDirection;

            Skill.direction = Vector2.Lerp(curDir, targetDir, elapsedTime / duration).normalized;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Skill.direction = Vector2.zero;
    }

    IEnumerator DrawTrajectoryUntilRelease() {
        if (Skill.CurrentSkill) {
            while (isHolding) {
                BulletTrajectory.DrawStraight(Skill.CurrentSkill.transform.position, Skill.direction, radius: 8f);
                yield return null;
            }
        }
        else {
            // If user're holding gun, wait for 0.5s to display trajectory pointer
            // TODO: Refactor this, should having a button a draw instead of waiting
            yield return new WaitForSeconds(0.8f);
            while (isHolding) {
                BulletTrajectory.DrawStraight(Skill.GunHead.transform.position, Skill.direction, radius: 8f);
                yield return null;
            }
        }


    }

    IEnumerator GetDirectionUntilRelease() {
        while (isHolding) {
            Skill.direction = (Input.mousePosition - BulletUI.position).normalized;
            yield return null;
        }
    }

    IEnumerator ScaleAndBack(RectTransform rect) {
        yield return StartCoroutine(ChangeScaleBulletUI(rect, 1.5f));
        yield return StartCoroutine(ChangeScaleBulletUI(rect, 1f));
    }

    IEnumerator ChangeScaleBulletUI(RectTransform rect, float targetScale) {
        float elapsedTime = 0f;
        float duration = 0.1f;
        float currentScale = rect.localScale.x;
        while (elapsedTime < duration) {
            float s = Mathf.Lerp(currentScale, targetScale, elapsedTime / duration);
            rect.localScale = new Vector3(s, s, s);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        rect.localScale = new Vector3(targetScale, targetScale, targetScale);
    }

    public void SetPlayerSkill(PlayerSkill skill) {
        Skill = skill;
        AddBulletUIEvent(ShotInside, EventTriggerType.PointerDown, ShotInsideDown);
        AddBulletUIEvent(ShotInside, EventTriggerType.PointerUp, ShotInsideUp);
        AddBulletUIEvent(ShotOutside, EventTriggerType.PointerDown, ShotOutsideDown);
        AddBulletUIEvent(ShotOutside, EventTriggerType.PointerUp, ShotOutsideUp);

        if (Skill.FirstSkill) {
            BaseSkill First = Skill.FirstSkill.GetComponent<BaseSkill>();
            FirstSkill.GetComponent<Image>().sprite = First.SkillSprite;
        }
        else {
            FirstSkill.gameObject.SetActive(false);
        }

        if (Skill.SecondSkill) {
            BaseSkill Second = Skill.SecondSkill.GetComponent<BaseSkill>();
            SecondSkill.GetComponent<Image>().sprite = Second.SkillSprite;
        }
        else {
            SecondSkill.gameObject.SetActive(false);
        }
    }

    public void SetPlayerMovement(PlayerMovement movement) {
        Movement = movement;
        AddBulletUIEvent(MoveControl, EventTriggerType.PointerDown, MoveIndicatorDown);
        AddBulletUIEvent(MoveControl, EventTriggerType.Drag, MoveIndicatorDrag);
        AddBulletUIEvent(MoveControl, EventTriggerType.PointerUp, MoveIndicatorUp);
    }

    void AddBulletUIEvent(EventTrigger trigger, EventTriggerType type, Action<BaseEventData> action) {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener((data) => action.Invoke(data));
        trigger.triggers.Add(entry);
    }

    public void ChangeGun() {
        Skill.ChangeGun();
    }

    public void PressSkill(int index) {
        Skill.PressSkill(index);
        RectTransform target = index == 0 ? FirstSkill : SecondSkill;
        StartCoroutine(ScaleAndBack(target));
    }

    public void PressAiming() {
        isAiming = !isAiming;
        Skill.OpenCloseAiming(isAiming);
    }

    public void ResetAiming() {
        if (isAiming) {
            isAiming = false;
            Skill.OpenCloseAiming(isAiming);
        }
    }

    public void ReloadGun() {
        Skill.ReloadGun();
    }

    public IEnumerator Countdown(Image img, float seconds, Action callback = null) {
        img.fillAmount = 1;
        float elapsedTime = 0f;
        while (elapsedTime < seconds) {
            img.fillAmount = Mathf.Lerp(1, 0, elapsedTime / seconds);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        img.fillAmount = 0;
        callback?.Invoke();
    }

    /*
    Shot outside
    */
    void ShotOutsideDown(BaseEventData eventData) {
        ActionDown();
        PointerEventData pointerData = eventData as PointerEventData;
        Skill.direction = (pointerData.position - (Vector2)BulletUI.position).normalized;
        StartCoroutine(GetDirectionUntilRelease());
        StartCoroutine(DrawTrajectoryUntilRelease());
    }

    void ShotOutsideUp(BaseEventData eventData) {
        BulletTrajectory.RemoveLine();
        isHolding = false;
        Skill.PlayHit(forceOneShot: true);
        turnBackGunDirection = StartCoroutine(BackToMoveDirection());
        StartCoroutine(ScaleAndBack(BulletUI));
    }

    /*
    Shot inside
    */
    void ShotInsideDown(BaseEventData eventData) {
        ActionDown();
        Skill.PlayHit(forceOneShot: false);
        StartCoroutine(ChangeScaleBulletUI(BulletUI, 1.5f));
    }

    void ShotInsideUp(BaseEventData eventData) {
        BulletTrajectory.RemoveLine();
        isHolding = false;
        turnBackGunDirection = StartCoroutine(BackToMoveDirection());
        StartCoroutine(ChangeScaleBulletUI(BulletUI, 1f));
    }

    /*
    Move control
    */
    void MoveIndicatorDown(BaseEventData eventData) {
        if (moveIndicatorComeback != null) {
            StopCoroutine(moveIndicatorComeback);
            moveIndicatorComeback = null;
        }
        PointerEventData pointerData = eventData as PointerEventData;
        Vector2 dragPos = pointerData.position;
        MoveIndicator.position = dragPos;
        Vector2 direction = (dragPos - moveIndicatorCenter).normalized;
        Movement.moveDirection = direction;
    }

    void MoveIndicatorDrag(BaseEventData eventData) {
        PointerEventData pointerData = eventData as PointerEventData;
        Vector2 dragPos = pointerData.position;
        Vector2 direction = (dragPos - moveIndicatorCenter).normalized;
        if (Vector2.Distance(dragPos, moveIndicatorCenter) > maxRadius) {
            dragPos = moveIndicatorCenter + direction * maxRadius;
        }
        MoveIndicator.position = dragPos;
        Movement.moveDirection = direction;
    }

    void MoveIndicatorUp(BaseEventData eventData) {
        Movement.moveDirection = Vector2.zero;
        moveIndicatorComeback = StartCoroutine(Comeback(MoveIndicator.position));
    }

    IEnumerator Comeback(Vector2 currentPos) {
        float duration = 0.1f;
        float elapsedTime = 0f;
        while (elapsedTime < duration) {
            MoveIndicator.position = Vector2.Lerp(currentPos, moveIndicatorCenter, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        MoveIndicator.position = moveIndicatorCenter;
    }

    /*
    Helper functions
    */
    void ActionDown() {
        isHolding = true;
        if (turnBackGunDirection != null) {
            StopCoroutine(turnBackGunDirection);
            turnBackGunDirection = null;
        }
    }
}
