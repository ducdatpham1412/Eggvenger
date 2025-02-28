using System.Collections.Generic;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

public class BulletPool : MonoBehaviour {
    [SerializeField] GameObject BulletPrefab;
    [SerializeField] PlayerManager Owner;
    Queue<Bullet> bulletsPool = new Queue<Bullet>();
    Transform Parent;
    GunStats CurrentStats;
    int poolSize = 20;

    void Start() {
        GameObject newObject = new GameObject($"BulletsPool_{Owner.id}");
        newObject.transform.position = Vector3.zero;
        Parent = newObject.transform;
    }

    Bullet InitBulletInfo(GameObject newBullet) {
        SpriteRenderer renderer = newBullet.GetComponent<SpriteRenderer>();
        renderer.sprite = CurrentStats.Bullet;
        CapsuleCollider2D collider2D = newBullet.GetComponent<CapsuleCollider2D>();
        collider2D.size = renderer.bounds.size;
        collider2D.offset = new Vector2(renderer.bounds.size.x / 2, 0);
        Bullet bullet = newBullet.GetComponent<Bullet>();
        bullet.Pool = this;
        bullet.stats = CurrentStats.bulletStats;
        bullet.Owner = Owner;
        return bullet;
    }

    public void FillPool(GunStats stats) {
        bulletsPool.Clear();
        CurrentStats = stats;
        for (int i = 0; i < poolSize; i++) {
            GameObject newBullet = Instantiate(BulletPrefab, Parent);
            Bullet bullet = InitBulletInfo(newBullet);
            newBullet.SetActive(false);
            bulletsPool.Enqueue(bullet);
        }
    }

    public Bullet GetBullet(bool shouldActive = true) {
        if (bulletsPool.Count > 0) {
            Bullet bullet = bulletsPool.Dequeue();
            bullet.gameObject.SetActive(shouldActive);
            return bullet;
        }
        else {
            GameObject newBullet = Instantiate(BulletPrefab, Parent);
            Bullet bullet = InitBulletInfo(newBullet);
            bullet.gameObject.SetActive(shouldActive);
            bulletsPool.Enqueue(bullet);
            return bullet;
        }
    }

    public Bullet[] GetBullets(int count = 1) {
        // Get many bullets return inactive bullets
        Bullet[] res = new Bullet[count];
        int i = 0;
        while (i < count) {
            Bullet temp = GetBullet(shouldActive: false);
            res[i] = temp;
            i++;
        }
        return res;
    }

    public void ReturnBullet(Bullet bullet) {
        bullet.gameObject.SetActive(false);
        bulletsPool.Enqueue(bullet);
    }
}
