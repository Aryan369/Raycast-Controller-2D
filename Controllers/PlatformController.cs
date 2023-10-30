using System.Collections.Generic;
using UnityEngine;

public class PlatformController : RaycastController
{
    #region Variables & Constants
    public LayerMask passengerMask;

    [Header("MOVEMENT")]
    public float speed = 5f;
    public bool cyclic;
    public float waitTime = 1f;
    [Range(0f, 15f)] public float easeAmount = 1f;

    public int fromWaypointIndex;
    float percentBetweenWaypoints;
    float nextMoveTime;

    public Vector3[] localWaypoints;
    Vector3[] globalWaypoints;

    List<PassengerMovement> passengerMovement;
    Dictionary<Transform, Controller> passengerDictionary = new Dictionary<Transform, Controller>();

    #endregion

    protected override void Start()
    {
        base.Start();

        globalWaypoints = new Vector3[localWaypoints.Length];
        for(int i=0; i<localWaypoints.Length; i++)
        {
            globalWaypoints[i] = localWaypoints[i] + transform.position;
        }
    }

    void FixedUpdate()
    {
        UpdateRaycastOrigins();

        Vector3 velocity = CalculatePlatformMovement();

        CalculatePassengerMovement(velocity);
        MovePassengers(true);
        transform.Translate(velocity);
        MovePassengers(false);
    }

    #region Methods
    float Ease(float x)
    {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1-x, a));
    }

    Vector3 CalculatePlatformMovement()
    {
        if(Time.time < nextMoveTime)
        {
            return Vector3.zero;
        }

        fromWaypointIndex %= globalWaypoints.Length;
        int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
        float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
        percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints;
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);

        float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);

        Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

        if(percentBetweenWaypoints >= 1)
        {
            percentBetweenWaypoints = 0;
            fromWaypointIndex++;

            if (!cyclic)
            {
                if (fromWaypointIndex >= globalWaypoints.Length - 1 || toWaypointIndex <= 0)
                {
                    fromWaypointIndex = 0;
                    System.Array.Reverse(globalWaypoints);
                }
            }
            nextMoveTime = Time.time + waitTime;
        }

        return newPos - transform.position;
    }

    void MovePassengers(bool beforeMovePlatform)
    {
        foreach(PassengerMovement passenger in passengerMovement)
        {
            if (!passengerDictionary.ContainsKey(passenger.transform))
            {
                passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<Controller>());
            }

            if(passenger.moveBeforePlatform == beforeMovePlatform)
            {
                passengerDictionary[passenger.transform].Move(passenger.velocity, passenger.standingOnPlatform);
            }
        }
    }

    void CalculatePassengerMovement(Vector3 velocity)
    {
        HashSet<Transform> movedPassenger = new HashSet<Transform>();
        passengerMovement = new List<PassengerMovement>();

        float directionX = Mathf.Sign(velocity.x);
        float directionY = Mathf.Sign(velocity.y);

        //Vertically Moving Platforms
        if (velocity.y != 0f)
        {
            float rayLength = Mathf.Abs(velocity.y) + skinWidth;

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

                if (hit && hit.distance != 0f)
                {
                    if (!movedPassenger.Contains(hit.transform))
                    {
                        movedPassenger.Add(hit.transform);
                        float pushX = (directionY == 1) ? velocity.x : 0;
                        float pushY = velocity.y - (hit.distance - skinWidth) * directionY;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
                    }
                }
            }
        }

        //Horizontally Moving Platforms
        if (velocity.x != 0f)
        {
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;

            for (int i = 0; i < horizontalRayCount; i++)
            {
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

                if (hit && hit.distance != 0f)
                {
                    if (!movedPassenger.Contains(hit.transform))
                    {
                        movedPassenger.Add(hit.transform);
                        float pushX = velocity.x - (hit.distance - skinWidth) * directionX;
                        float pushY = -skinWidth;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
                    }
                }
            }
        }

        //Player on Top of Horizontal Or Downward Platform
        if(directionY == -1f || velocity.y == 0f && velocity.x != 0f)
        {
            float rayLength = skinWidth * 2;

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

                if (hit && hit.distance != 0f)
                {
                    if (!movedPassenger.Contains(hit.transform))
                    {
                        movedPassenger.Add(hit.transform);
                        float pushX = velocity.x;
                        float pushY = velocity.y;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
                    }
                }
            }
        }
    }

    #endregion

    #region Structs
    struct PassengerMovement
    {
        public Transform transform;
        public Vector3 velocity;
        public bool standingOnPlatform;
        public bool moveBeforePlatform;

        public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform)
        {
            transform = _transform;
            velocity = _velocity;
            standingOnPlatform = _standingOnPlatform;
            moveBeforePlatform = _moveBeforePlatform;
        }
    }

    #endregion

    #region Gizmos
    private void OnDrawGizmos()
    {
        if(localWaypoints != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, .8f);
            float size = .3f;

            for(int i=0; i<localWaypoints.Length; i++)
            {
                Vector3 globalWaypointPos = (Application.isPlaying)?globalWaypoints[i]:localWaypoints[i] + transform.position;
                Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
                Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
            }
        }
    }

    #endregion
}
