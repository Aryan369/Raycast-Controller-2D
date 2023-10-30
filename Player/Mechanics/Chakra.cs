using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.VersionControl.Asset;
//ReSharper disable All

public class Chakra : MonoBehaviour
{
    public static Chakra Instance;
    
    private Controller _controller;

    #region CHAKRA

    [Header("CHAKRA")] 
    public float maxChakra = 8f;
    private float chakraRefillRate = 2.5f;
    private float chakra;

    [Header("SHARINGAN (BULLET TIME)")] 
    public float sharinganTimeScale = .4f;
    [HideInInspector] public bool isUsingSharingan;
    
    [Header("RINNEGAN")] 
    public LayerMask collisionMask;
    
    public float range = 30f;
    
    public float rinneTimeScale = 0.075f;
    private float rinneTimeScaleBufferFactor = 2.67f;
    
    [HideInInspector] public bool isUsingRinnegan;
    private float rinneganChakraDepleteFactor = 1.25f;
    
    private bool canTeleport;
    private bool isTeleporting;
    
    public float rinneBufferTime = .125f; //Time timescale remains slowed down after teleporting (Old Value : .15f)
    private float rinneBufferTimeCounter;
    
    public bool aimToSelect = true;
    public bool _360Vision;

    [Header("CANVAS COMPONENTS")]
    public Slider bulletTimeSlider;
    public Object bulletTimeBar;
    public Gradient bulletTimeBarGradient;
    public Image bulletTimeFill;
    public Transform bulletTimeCanvas;

    #endregion

    [HideInInspector] public GameObject _replacedObj = null;

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

    private void Start()
    {
        _controller = Player.Instance.controller;
        chakra = maxChakra;
        SetBulletTimeBarMax();
    }

    private void Update()
    {
        HandleChakra();
        HandleGameState();
        HandleRinnegan();
        SetBulletTimeBar();

        if (GameManager.Instance._gameState == GameState.Rinnegan)
        {
            CollisionCheck();
        }

        //if(GameManager.Instance._gameState == GameState.BulletTime)
        //{
        //    bulletTimeCanvas.gameObject.SetActive(true);
        //    SetBulletTimeBar();
        //}
        //else
        //{
        //    bulletTimeCanvas.gameObject.SetActive(false);
        //}
    }

    #region UI

    private void SetBulletTimeBarMax()
    {
        bulletTimeSlider.maxValue = maxChakra * 2;
        bulletTimeSlider.value = maxChakra * 2;

        bulletTimeFill.color = bulletTimeBarGradient.Evaluate(1f);
    }

    private void SetBulletTimeBar()
    {
        bulletTimeSlider.value = chakra * 2;

        bulletTimeFill.color = bulletTimeBarGradient.Evaluate(bulletTimeSlider.normalizedValue);
    }

    #endregion

    #region Methods
    void HandleChakra()
    {
        if (chakra > 0f)
        {
            if (isUsingRinnegan)
            {
                chakra -= (Time.deltaTime * rinneganChakraDepleteFactor) / rinneTimeScale;
            }
            else if (isUsingSharingan)
            {
                chakra -= Time.deltaTime / sharinganTimeScale;
            }
        }
        else
        {
            canTeleport = false;
            isUsingRinnegan = false;
            isUsingSharingan = false;
        }

        if (chakra < maxChakra && !isUsingRinnegan && !isUsingSharingan)
        {
            chakra += Time.deltaTime * chakraRefillRate;
        }
    }

    void HandleGameState()
    {
        if(GameManager.Instance._gameState != GameState.Paused)
            if (isUsingRinnegan)
            {
                GameManager.Instance._gameState = GameState.Rinnegan;
                isUsingSharingan = false;

                canTeleport = true;
                Time.timeScale = rinneTimeScale;
            }
            else if (isUsingSharingan)
            {
                GameManager.Instance._gameState = GameState.BulletTime;

                Time.timeScale = sharinganTimeScale;
            }
            else if (rinneBufferTimeCounter <= 0)
            {
                GameManager.Instance._gameState = GameState.Play;

                Time.timeScale = 1f;
            }
    }
    
    void HandleRinnegan()
    {
        if (canTeleport)
        {
            if (!aimToSelect)
            {
                RinneganAminotejikara();
            }
            else
            {
                if (!isUsingRinnegan)
                {
                    RinneganAminotejikara();
                    
                    canTeleport = false;
                }
            }
        }

        rinneBufferTimeCounter -= Time.deltaTime;
    }

    void RinneganAminotejikara()
    {
        if (_replacedObj != null)
        {
            isTeleporting = true;
            Vector3 _to = _replacedObj.transform.position;
            _replacedObj.transform.position = Player.Instance.transform.position;

            Aminotejikarable _replacedObject = _replacedObj.GetComponent<Aminotejikarable>();
            float offsetX = _replacedObject.teleportOffset.x == 0 ? -_replacedObject.teleportOffset.y : _replacedObject.teleportOffset.x;
            float offsetY = _replacedObject.teleportOffset.z == 0 ? _replacedObject.teleportOffset.w : -_replacedObject.teleportOffset.z;
            Vector3 offset = new Vector2(offsetX, offsetY);

            Player.Instance.transform.position = _to + offset;
            if (_replacedObj.CompareTag("Throwable"))
            {
                _replacedObj.GetComponent<Throwable>().SwitchPos();
            }
            _replacedObj = null;
            isTeleporting = false;
            isUsingRinnegan = false;
            Time.timeScale = rinneTimeScale * rinneTimeScaleBufferFactor;
            rinneBufferTimeCounter = rinneBufferTime;
        }
    }
    

    #endregion

    #region Collision
    void CollisionCheck()
    {
        Vector2 _origin = new Vector2(transform.position.x, transform.position.y);
        var hitColliders = Physics2D.OverlapCircleAll(_origin, range, collisionMask);

        for (int i = 0; i < hitColliders.Length; i++)
        {
            Vector2 dir = hitColliders[i].transform.position -  transform.position;
            Physics2D.queriesStartInColliders = false;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir);
            Debug.DrawRay(transform.position, dir, Color.black);

            if (hit)
            {
                if (hit.collider == hitColliders[i])
                {
                    if (_360Vision)
                    {
                        hitColliders[i].GetComponent<Aminotejikarable>().isActive = true;
                    }
                    else
                    {
                        if (Mathf.Sign(hitColliders[i].transform.position.x - transform.position.x) == _controller.collisionData.faceDir)
                        {
                            hitColliders[i].GetComponent<Aminotejikarable>().isActive = true;
                        }
                        else
                        {
                            hitColliders[i].GetComponent<Aminotejikarable>().isActive = false;
                        }
                    }
                }
            }
        }

        
        //Aiming
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(InputManager.Instance.mousePosAction.ReadValue<Vector2>());
        Vector2 aimdir = (mousePos - transform.position);

        RaycastHit2D _hit = Physics2D.Raycast(transform.position, aimdir, range);
        Debug.DrawRay(transform.position, aimdir, Color.green);
        
        if (_hit)
        {
            if (_hit.collider.CompareTag("Aminotejikarable") || _hit.collider.CompareTag("Throwable"))
            {
                if (_hit.collider.GetComponent<Aminotejikarable>().isActive)
                {
                    _hit.collider.GetComponent<Aminotejikarable>().isHovered = true;
                }
            }
        }
    }
    
    #endregion

    #region Gizmos
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(.5f, .5f, .9f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, range);
    }
    
    #endregion
}
