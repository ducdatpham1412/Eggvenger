using UnityEngine;

public class BulletTrajectory : MonoBehaviour {
    LineRenderer lineRenderer;
    int maxPointCount = 200;
    float timeStep = 0.1f;
    float gravity = -1;
    int layerMask;

    public PlayerManager Owner;

    void Awake() {
        lineRenderer = GetComponent<LineRenderer>();
        layerMask = LayerMask.GetMask(Helper.Layer.Player.ToString(), Helper.Layer.Obstacle.ToString());
    }

    public void DrawParabol(Vector3 startPos, Vector3 direction, float speed) {
        if (lineRenderer == null) return;

        lineRenderer.positionCount = maxPointCount;
        Vector3[] points = new Vector3[maxPointCount];
        Vector3 velocity = direction * speed;

        for (int i = 0; i < maxPointCount; i++) {
            float t = i * timeStep;
            Vector3 point = startPos + velocity * t + 0.5f * new Vector3(0, gravity, 0) * t * t;
            points[i] = point;

            if (i > 0 && point.y <= 0) {
                lineRenderer.positionCount = i + 1;
                break;
            }
        }

        lineRenderer.SetPositions(points);
    }

    public void DrawParabolToDestination(Vector3 startPos, Vector3 des, float speed) {
        Debug.Log("TO DO");
    }

    public void DrawStraight(Vector3 startPos, Vector3 direction, float radius = 5f) {
        if (lineRenderer == null) return;

        lineRenderer.positionCount = maxPointCount;
        Vector3[] points = new Vector3[maxPointCount];

        for (int i = 0; i < maxPointCount; i++) {
            Vector3 point = startPos + i * direction;
            points[i] = point;

            RaycastHit2D[] hits = Physics2D.RaycastAll(startPos, direction, i * direction.magnitude, layerMask);
            foreach (RaycastHit2D hit in hits) {
                if ((layerMask & (1 << hit.collider.gameObject.layer)) != 0) {
                    PlayerManager player = hit.collider.gameObject.GetComponent<PlayerManager>();
                    if (player.team != Owner.team) {
                        lineRenderer.positionCount = i + 1;
                        points[i] = hit.point;
                        break;
                    }
                }
                else {
                    lineRenderer.positionCount = i + 1;
                    points[i] = hit.point;
                    break;
                }
            }

            // Check next point is overflow
            // We should check in this instead of next loop to avoid line overflow radius
            if (Vector3.Distance(startPos, point + direction) - radius > 0.1f) {
                lineRenderer.positionCount = i + 1;
                break;
            }
        }

        lineRenderer.SetPositions(points);
    }

    public void RemoveLine() {
        lineRenderer.positionCount = 0;
    }
}