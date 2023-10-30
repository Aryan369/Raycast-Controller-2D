using UnityEngine;
using UnityEngine.InputSystem;

public class Boomerang : RaycastController
{
    #region Variables & Constants
    public LayerMask teleportCollisionMask;

    [Header("MOVEMENT")]
    public float speed = 13f;
    public float distance = 13f;
    public float waitTime = .15f;
    [Range(0f, 15f)] public float easeAmount = 1.5f;

    [HideInInspector] public bool isBoomeranging;
    [HideInInspector] public bool isReturning;
    [HideInInspector] public bool onReturn;

    private float distanceX;
    private float distanceY;

    private float percentBetweenPoints;
    [HideInInspector] public float nextMoveTime;

    private Player player;
    private Transform _player;
    private Vector3 startPoint;
    private Vector3 endPoint;
    [SerializeField] Vector3 offset;

    private SpriteRenderer sr;
    private TrailRenderer tr;

    private float rayLengthX;
    private float rayLengthY;
    public Vector4 teleportOffset; // L,R,U,D

    #endregion

    protected override void Start()
    {
        base.Start();
        player = Player.Instance;
        _player = player.transform;

        BoxCollider2D _playerCollider = player.GetComponent<BoxCollider2D>();

        rayLengthX = ((_playerCollider.bounds.size.x - collider.bounds.size.x) / 2);
        rayLengthY = ((_playerCollider.bounds.size.y - collider.bounds.size.y) / 2);

        sr = GetComponent<SpriteRenderer>();
        tr = GetComponent<TrailRenderer>();

        DeactivateBoomerang();
    }

    void Update()
    {
        UpdateRaycastOrigins();

        Vector3 velocity = CalculateBoomerangMovement();

        if (isBoomeranging)
        {
            CollisionCheck(velocity);
            TeleportedCollisionCheck(velocity);
        }

        transform.Translate(velocity);
    }

    #region Methods
    public void ActivateBoomerang(Vector2 _direction)
    {
        Vector2 directionalInput = _direction;
        //float _offset = offset.x * (player.controller.collisionData.faceDir);
        //transform.position = _player.position + new Vector3(_offset, offset.y);
        transform.position = _player.position;
        startPoint = transform.position;
        distanceX = distance * directionalInput.x;
        distanceY = distance * directionalInput.y;
        endPoint = new Vector3(transform.position.x + distanceX, transform.position.y + distanceY);
        sr.enabled = true;
        tr.enabled = true;
        collider.enabled = true;
        isBoomeranging = true;
    }

    public void DeactivateBoomerang()
    {
        percentBetweenPoints = 0f;
        isBoomeranging = false;
        isReturning = false;
        onReturn = false;
        nextMoveTime = Time.time;
        player.isBoomeranging = isBoomeranging;
        sr.enabled = false;
        tr.Clear();
        tr.enabled = false;
        collider.enabled = false;
    }

    float Ease(float x)
    {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    Vector3 CalculateBoomerangMovement()
    {
        if (isBoomeranging)
        {
            if (Time.time < nextMoveTime)
            {
                return Vector3.zero;
            }

            float distanceBetweenPoints = Vector3.Distance(startPoint, endPoint);
            percentBetweenPoints += Time.deltaTime * speed / distanceBetweenPoints;
            percentBetweenPoints = Mathf.Clamp01(percentBetweenPoints);

            float easedPercentBetweenPoints = Ease(percentBetweenPoints);

            Vector3 newPos = Vector3.Lerp(startPoint, endPoint, easedPercentBetweenPoints);

            if (percentBetweenPoints >= 1)
            {
                if (!isReturning)
                {
                    percentBetweenPoints = 0;
                    isReturning = true;
                    onReturn = true;
                    startPoint = endPoint;
                    endPoint = _player.position;
                    nextMoveTime = Time.time + waitTime;
                }
                else
                {
                    DeactivateBoomerang();
                }
            }

            if (onReturn)
            {
                endPoint = _player.position;
            }

            return newPos - transform.position;
        }
        else
        {
            return Vector3.zero;
        }
    }

    void CollisionCheck(Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        float directionY = Mathf.Sign(velocity.y);

        //Vertically Moving
        if (velocity.y != 0f)
        {
            float rayLength = Mathf.Abs(velocity.y) + skinWidth;

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i);
                Physics2D.queriesStartInColliders = false;
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

                if (hit)
                {
                    if (!isReturning && !hit.collider.CompareTag("Player"))
                    {
                        percentBetweenPoints = 0.2f;
                        isReturning = true;
                        startPoint = transform.position;
                        endPoint = _player.position;
                        onReturn = true;
                        nextMoveTime = Time.time;
                    }
                }
            }
        }

        //Horizontally Moving 
        if (velocity.x != 0f)
        {
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;

            for (int i = 0; i < horizontalRayCount; i++)
            {
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                Physics2D.queriesStartInColliders = false;
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

                if (hit)
                {
                    if (!isReturning && !hit.collider.CompareTag("Player"))
                    {
                        percentBetweenPoints = 0.2f;
                        isReturning = true;
                        startPoint = transform.position;
                        endPoint = _player.position;
                        onReturn = true;
                        nextMoveTime = Time.time;
                    }
                }
            }
        }
    }
    #endregion

    #region Check for Teleported Player will collide
    void TeleportedCollisionCheck(Vector3 velocity)
    {
        //Horizontal Collisions
        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = raycastOrigins.bottomLeft;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.left, rayLengthX, teleportCollisionMask);
            Debug.DrawRay(rayOrigin, Vector2.left * rayLengthX, Color.cyan);

            if (hit)
            {
                teleportOffset = new Vector4((rayLengthX + skinWidth), teleportOffset.y, teleportOffset.z, teleportOffset.w);
            }
            else
            {
                teleportOffset.x = 0f;
            }


            Vector2 rayOrigin2 = raycastOrigins.bottomRight;
            rayOrigin2 += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit2 = Physics2D.Raycast(rayOrigin2, Vector2.right, rayLengthX, teleportCollisionMask);
            Debug.DrawRay(rayOrigin2, Vector2.right * rayLengthX, Color.cyan);

            if (hit2)
            {
                teleportOffset = new Vector4(teleportOffset.x, (rayLengthX + skinWidth), teleportOffset.z, teleportOffset.w);
            }
            else
            {
                teleportOffset.y = 0f;
            }
        }


        //Vertical Collisions
        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLengthY, teleportCollisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * rayLengthY, Color.cyan);

            if (hit)
            {
                teleportOffset = new Vector4(teleportOffset.x, teleportOffset.y, (rayLengthY + skinWidth), teleportOffset.w);
            }
            else
            {
                teleportOffset.z = 0f;
            }


            Vector2 rayOrigin2 = raycastOrigins.bottomLeft;
            rayOrigin2 += Vector2.right * (verticalRaySpacing * i);
            RaycastHit2D hit2 = Physics2D.Raycast(rayOrigin2, Vector2.down, rayLengthY, teleportCollisionMask);
            Debug.DrawRay(rayOrigin2, Vector2.down * rayLengthY, Color.cyan);

            if (hit2)
            {
                teleportOffset = new Vector4(teleportOffset.x, teleportOffset.y, teleportOffset.z, (rayLengthY + skinWidth));
            }
            else
            {
                teleportOffset.w = 0f;
            }
        }
    }
    #endregion
}
