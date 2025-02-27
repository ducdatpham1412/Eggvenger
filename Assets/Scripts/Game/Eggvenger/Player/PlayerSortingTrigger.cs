using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerSortingTrigger : MonoBehaviour {
    SpriteRenderer sprite;
    CapsuleCollider2D collider2D;
    Dictionary<GameObject, int> sortingOrder = new Dictionary<GameObject, int>();

    void Awake() {
        sprite = GetComponent<SpriteRenderer>();
        collider2D = GetComponent<CapsuleCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D collider) {
        Tilemap tilemap = collider.GetComponent<Tilemap>();
        Collider2D[] colliders = new Collider2D[10];
        ContactFilter2D filter = new ContactFilter2D();
        int numCollisions = collider.Overlap(filter, colliders);

        Debug.Log("Num collision: " + numCollisions);

        for (int i = 0; i < numCollisions; i++) {
            Vector3 hitPos = colliders[i].transform.position;
            Vector3Int tilePosition = tilemap.WorldToCell(hitPos);
            TileBase triggeredTile = tilemap.GetTile(tilePosition);

            if (triggeredTile != null) {
                Debug.Log("Triggered Tile at: " + tilePosition);
            }
        }
    }

    void OnTriggerStay2D(Collider2D collider) {
        if (sortingOrder.TryGetValue(collider.gameObject, out int order)) {
            if (collider.gameObject.transform.position.y >= transform.position.y) {
                sprite.sortingOrder = order + 1;
            }
            else {
                sprite.sortingOrder = order - 1;
            }
        }
    }

    void OnTriggerExit2D(Collider2D collider) {
        if (sortingOrder.ContainsKey(collider.gameObject)) {
            sortingOrder.Remove(collider.gameObject);
        }
    }
}
