using UnityEngine;

public class Attack : MonoBehaviour
{
    #region Variables
    
    public static Attack Instance;
    
    private PolygonCollider2D hitbox;

    #region ATTACKS

    #region SLASH

    [Header("SLASH")]
    public LayerMask attackMask;
    [SerializeField] private float slashDistance = 3f;
    [SerializeField] private float slashAngleRange = 120f;
    [HideInInspector] public float slashCooldown = .25f;
    [HideInInspector] public float slashCooldownCounter;
    public float _slashPlayerVelocity = 26.25f; //Depends on maxJumpVelocity (KatanaZero ratio 3.5 : 3 {maxJumpVel})
    public float _midAirSlashPlayerVelocity =  9f;
    private float numberOfMidAirSlash;
    private Vector2 slashDir;

    #endregion

    #region THROWABLE

    [Header("THROWABLE")] 
    public LayerMask _throwableMask;
    public float throwVelocity = 100f; //80f
    private float _throwablePickableRange = 2.5f;
    private GameObject _throwable = null;
    private GameObject _pickable = null;
    private bool canPickThrowable;
    
    #endregion
    
    #endregion

    #endregion
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Update()
    {
        HandleSlash();
    }


    #region Methods
    
    #region Slash
    
    private void HandleSlash()
    {
        if (InputManager.Instance.attackAction.triggered)
        {
            if (slashCooldownCounter <= 0f)
            {
                if (GameManager.Instance._gameState != GameState.Paused && GameManager.Instance._gameState != GameState.Rinnegan)
                {
                    Vector3 mousePos = Camera.main.ScreenToWorldPoint(InputManager.Instance.mousePosAction.ReadValue<Vector2>());

                    if (Mathf.Sign(mousePos.x - transform.position.x) != Mathf.Sign(Player.Instance.controller.collisionData.faceDir))
                    {
                        Player.Instance.controller.collisionData.faceDir *= -1;
                    }

                    slashDir = (mousePos - transform.position).normalized;

                    //Pushing player in the dir of slash
                    float slashDirX = Mathf.Clamp(mousePos.x - transform.position.x, -1, 1);
                    float slashDirY = Mathf.Clamp(mousePos.y - transform.position.y, -1, 1);
                    Vector3 _slashPlayerVelDir = new Vector3(slashDirX, slashDirY);

                    if (!Player.Instance.hasSlashedMidAir && !Player.Instance.isJumping)
                    {
                        //Player.Instance.velocity += _slashPlayerVelocity * slashDir;
                        Player.Instance.velocity = _slashPlayerVelocity * _slashPlayerVelDir;
                        numberOfMidAirSlash = 0;
                    }
                    else
                    {
                        numberOfMidAirSlash++;
                        //Player.Instance.velocity.x += _slashPlayerVelocity * slashDir.x;
                        //Player.Instance.velocity.y += _midAirSlashPlayerVelocity * slashDir.y / Mathf.Sqrt(numberOfMidAirSlash);

                        Player.Instance.velocity.x = _slashPlayerVelocity * _slashPlayerVelDir.x;
                        Player.Instance.velocity.y = _midAirSlashPlayerVelocity * _slashPlayerVelDir.y / Mathf.Sqrt(numberOfMidAirSlash);
                    }

                    //Collision Detection
                    Collider2D [] hitTargets = Physics2D.OverlapCircleAll(transform.position, slashDistance, attackMask);
                    if(hitTargets != null)
                    {
                        foreach (Collider2D hitTarget in hitTargets)
                        {
                            Vector2 dirToHitTarget = (hitTarget.transform.position - transform.position).normalized;
                            float angleBetweenHitTargetAndKatana = Vector2.Angle(slashDir, dirToHitTarget);
                            if (angleBetweenHitTargetAndKatana < slashAngleRange / 2f)
                            {
                                if (hitTarget.CompareTag("Enemy"))
                                {
                                    hitTarget.GetComponent<EnemyAI>().KillEnemy();
                                    slashCooldownCounter = slashCooldown;
                                }

                                if (hitTarget.CompareTag("Door"))
                                {
                                    hitTarget.GetComponent<BoxCollider2D>().isTrigger = true;
                                    hitTarget.gameObject.layer = 0;
                                    //TODO Animate opening of door
                                    SpriteRenderer sr = hitTarget.gameObject.GetComponent<SpriteRenderer>();
                                    sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.2f);
                                    slashCooldownCounter = slashCooldown;
                                }

                                if (GameManager.Instance._gameState == GameState.BulletTime && hitTarget.CompareTag("Bullet"))
                                {
                                    hitTarget.GetComponent<Bullet>().DeflectBullet();
                                    slashCooldownCounter = slashCooldown;
                                }
                            }
                        }
                    }

                    Player.Instance.hasSlashedMidAir = true;
                    slashCooldownCounter = slashCooldown;
                    //Player.Instance.cameraEffects.Shake(50f, 0.35f);
                }  
            }
        }
        else
        {
            if (slashCooldownCounter > 0f)
            {
                slashCooldownCounter -= Time.deltaTime;
            }
        }
    }
    
    #endregion
    
    #region Throwable
    public void HandleThrowable()
    {
        HandleThrowableDetection();

        if (canPickThrowable)
        {
            if (_throwable != null)
            {
                _throwable.GetComponent<Throwable>().state = ThrowableStates.Discarded;
                _throwable = _pickable;
                _throwable.GetComponent<Throwable>().state = ThrowableStates.Picked;
            }
            else
            {
                _throwable = _pickable;
                _throwable.GetComponent<Throwable>().state = ThrowableStates.Picked;
            }
            
            canPickThrowable = false;
        }
        else
        {
            if (_throwable != null)
            {
                _throwable.GetComponent<Throwable>().Throw(throwVelocity);
                _throwable = null;
            }
        }
    }

    void HandleThrowableDetection()
    {
        Collider2D throwableHit = Physics2D.OverlapCircle(transform.position, _throwablePickableRange, _throwableMask);
        if (!throwableHit)
        {
            canPickThrowable = false;
            _pickable = null;
        }
        if (throwableHit)
        {
            if (throwableHit.GetComponent<Throwable>().state == ThrowableStates.Idle)
            {
                canPickThrowable = true;
                _pickable = throwableHit.gameObject;
            }
        }
    }
    
    #endregion

    #endregion
    
    #region Gizmos

    private void OnDrawGizmos()
    {
        //Slash Range
        Gizmos.color = new Color(1f, 0.8f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, slashDistance);
        
        //Throwable Range
        Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
        Gizmos.DrawWireSphere(transform.position, _throwablePickableRange);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, slashDir);
    }

    #endregion
}
