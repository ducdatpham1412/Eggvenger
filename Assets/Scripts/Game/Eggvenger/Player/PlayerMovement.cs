using System.Linq;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    Rigidbody2D Rb;
    PlayerManager manager;
    [SerializeField] Animator animator;

    Vector2 moveDirection;

    void Start() {
        Rb = GetComponent<Rigidbody2D>();
        manager = GetComponent<PlayerManager>();
    }

    void Update() {
        if (!manager.IsOwner) return;
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        moveDirection = new Vector2(moveX, moveY).normalized;
    }

    void FixedUpdate() {
        if (moveDirection != null) {
            Rb.linearVelocity = moveDirection * manager.currentSpeed;

            if (moveDirection == Vector2.zero) {
                return;
            }

            float degree = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;

            if (degree > 0 && degree < 60) {
                animator.Play("TopRight", 1);
            }
            else if (degree <= 0 && degree > -60) {
                animator.Play("BottomRight", 1);
            }
            else if (degree >= 60 && degree < 120) {
                animator.Play("Top", 1);
            }
            else if (degree <= -60 && degree > -120) {
                animator.Play("Bottom", 1);
            }
            else if (degree >= 120 && degree < 180) {
                animator.Play("TopLeft", 1);
            }
            else {
                animator.Play("BottomLeft", 1);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        AdjustVelocityOnCollision(collision);
    }

    void OnCollisionStay2D(Collision2D collision) {
        AdjustVelocityOnCollision(collision);
    }

    void AdjustVelocityOnCollision(Collision2D collision) {
        Vector2 normal = collision.GetContact(0).normal;
        Vector2 reflect = Vector2.Reflect(Rb.linearVelocity, normal);
        Rb.linearVelocity = reflect;
    }

    void VFXMovement(float ratio) {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (ratio == 1) {
            renderer.color = new Color(1f, 1f, 1f, 1f);
            return;
        }
        if (ratio >= 0 && ratio < 1) {
            float c = Mathf.Lerp(0.55f, 1f, ratio / 1f);
            renderer.color = new Color(c, c, c, 1f);
        }
        if (ratio > 1) {
            // TODO: Add effect when ratio > 1
        }
    }

    void SetCurrentSpeed() {
        float ratio = manager.speedRatios.Count == 0 ? 1 : manager.speedRatios.Aggregate(1f, (acc, num) => acc * num);
        manager.currentSpeed = (manager.originalSpeed + manager.buffSpeed) * ratio;
        VFXMovement(ratio);
    }

    public void BuffSpeed(float value) {
        manager.buffSpeed += value;
        SetCurrentSpeed();
    }

    public void AddSpeedRatio(float ratio, float? seconds) {
        manager.speedRatios.Add(ratio);
        SetCurrentSpeed();
        if (seconds != null) {
            void Revert() {
                manager.speedRatios.Remove(ratio);
                SetCurrentSpeed();
            }
            Invoke(nameof(Revert), (float)seconds);
        }
    }

    public void RemoveSpeedRatio(float ratio) {
        if (manager.speedRatios.Contains(ratio)) {
            manager.speedRatios.Remove(ratio);
            SetCurrentSpeed();
        }
    }
}
