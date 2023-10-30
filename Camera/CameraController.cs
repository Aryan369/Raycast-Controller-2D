using UnityEngine;

public class CameraController : MonoBehaviour
{
    Transform target;
    Vector3 velocity = Vector3.zero;
    public Vector3 posOffset;
    public Vector2 xLimit;
    public Vector2 yLimit;

    [Range(0,1)]
    public float smoothTime;

    void Start()
    {
        target = Player.Instance.transform;
    }

    void LateUpdate()
    {
        if (Player.Instance == null)
        {
            return;
        }

        Vector3 targetPosition = target.position + posOffset;
        targetPosition = new Vector3(Mathf.Clamp(target.position.x, xLimit.x, xLimit.y), Mathf.Clamp(target.position.y, yLimit.y, yLimit.y), -10);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
