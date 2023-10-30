using UnityEngine;

public class Aminotejikarable : RaycastController
{
    [HideInInspector] public bool isActive;
    [HideInInspector] public bool isHovered;
    private bool isSelected;
    private bool isSelectedOld;

    private SpriteRenderer _sr;
    private Color _color;

    private float rayLengthX;
    private float rayLengthY;
    [HideInInspector] public Vector4 teleportOffset; // L,R,U,D

    protected override void Start()
    {
        base.Start();
        _sr = GetComponent<SpriteRenderer>();
        _color = _sr.color;

        BoxCollider2D _playerCollider = Player.Instance.GetComponent<BoxCollider2D>();

        rayLengthX = ((_playerCollider.bounds.size.x - collider.bounds.size.x) / 2) * 1.2f;
        rayLengthY = ((_playerCollider.bounds.size.y - collider.bounds.size.y) / 2) * 1.2f;
    }

    private void Update()
    {
        UpdateRaycastOrigins();
        TeleportedCollisionCheck();

        ColorSelect();

        if (GameManager.Instance._gameState == GameState.Rinnegan)
        {
            if (Vector2.Distance(transform.position, Player.Instance.transform.position) > Chakra.Instance.range)
            {
                isActive = false;
            }
            
            if (isSelected && Chakra.Instance._replacedObj != this.gameObject)
            {
                isSelected = false;
            }
        }
        else
        {
            isActive = false;
        }
        
        Aim();
        
        isHovered = false;
    }

    #region Methods
    void Aim()
    {
        if (!Chakra.Instance.aimToSelect)
        {
            if (InputManager.Instance.aminotejikaraAction.triggered)
            {
                if (isHovered)
                {
                    if (isActive)
                    {
                        if (!isSelected)
                        {
                            isSelected = true;
                            Chakra.Instance._replacedObj = gameObject;
                        }
                        else
                        {
                            isSelected = false;
                            Chakra.Instance._replacedObj = null;
                        }
                    }
                }
            }
        }
        else
        {
            isSelectedOld = isSelected;
            if (isHovered && isActive)
            {
                isSelected = true;
            }
            else
            {
                isSelected = false;
            }

            if (isSelected != isSelectedOld)
            {
                if (isSelected)
                {
                    Chakra.Instance._replacedObj = gameObject;
                }
                else
                {
                    Chakra.Instance._replacedObj = null;
                }
            }
        }
    }

    #endregion

    #region Graphic
    void ColorSelect()
    {
        if (GameManager.Instance._gameState == GameState.Rinnegan)
        {
            if (isSelected)
            {
                _sr.color = new Color(0f, 1f, 1f);
            }
            else if (isHovered)
            {
                _sr.color = new Color(1f, 0.8443396f, 0.9693651f);
            }
            else if (isActive)
            {
                _sr.color = new Color(0.25f, 0.8971107f, 0.4841958f);
            }
            else
            {
                _sr.color = new Color(1f, 0f, 0.6135602f);
            }
        }
        else
        {
            _sr.color = _color;
        }
    }

    #endregion

    #region Check for Teleported Player will collide
    void TeleportedCollisionCheck()
    {
        //Horizontal Collisions
        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = raycastOrigins.bottomLeft;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.left, rayLengthX, collisionMask);
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
            RaycastHit2D hit2 = Physics2D.Raycast(rayOrigin2, Vector2.right, rayLengthX, collisionMask);
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
            rayOrigin += Vector2.right * (verticalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLengthY, collisionMask);

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
            RaycastHit2D hit2 = Physics2D.Raycast(rayOrigin2, Vector2.down, rayLengthY, collisionMask);
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
