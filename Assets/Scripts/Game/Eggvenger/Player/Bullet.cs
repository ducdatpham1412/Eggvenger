using UnityEngine;

public class Bullet : MonoBehaviour {
    public GunStats.BulletStats stats;
    public BulletPool Pool;
    public PlayerManager Owner;

    private void Update() {
        transform.Translate(Vector3.right * stats.speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collider) {
        string layer = LayerMask.LayerToName(collider.gameObject.layer);
        if (layer == Helper.Layer.Environment.ToString()) {
            string tag = collider.gameObject.tag;
            if (tag == Helper.Tag.Obstacle.ToString()) {
                Pool.ReturnBullet(this);
            }
            else if (tag == Helper.Tag.Item.ToString()) {
                Pool.ReturnBullet(this);
                // TODO: Destroy Item
            }
        }
        else if (layer == Helper.Layer.Player.ToString()) {
            TakeDamage take = collider.GetComponent<TakeDamage>();
            if (take != null && take.player.team != Owner.team) {
                Pool.ReturnBullet(this);
                // TODO: Attack player
            }
        }
    }
}
