using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour
{
    #region Variables & Constants
    protected BoxCollider2D collider;
    protected Vector2 colliderSize;
    protected RaycastOrigins raycastOrigins;

    [Header("RAYCAST")]
    public LayerMask collisionMask;

    const float distBetweenRays = .2f;

    protected int horizontalRayCount;
    protected int verticalRayCount;

    protected float horizontalRaySpacing;
    protected float verticalRaySpacing;

    protected const float skinWidth = .015f;
    #endregion

    protected virtual void Awake()
    {
        collider = GetComponent<BoxCollider2D>();
    }

    protected virtual void Start()
    {
        colliderSize = new Vector2(collider.size.x, collider.size.y);
        CalculateRaySpacing();
    }

    #region Raycast Origin & Spacing Methods
    protected void UpdateRaycastOrigins()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    protected void CalculateRaySpacing()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        float boundsWidth = bounds.size.x;
        float boundsHeight = bounds.size.y;

        horizontalRayCount = Mathf.RoundToInt(boundsHeight / distBetweenRays);
        verticalRayCount = Mathf.RoundToInt(boundsWidth / distBetweenRays);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }
    #endregion

    #region Structs
    protected struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    #endregion
}
