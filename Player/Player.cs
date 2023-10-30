using System.Collections;
using UnityEngine;
// ReSharper disable All

[RequireComponent(typeof(Controller), typeof(InputManager))]
public class Player : MonoBehaviour
{
    #region Variables & Constants

    #region Ref
    public static Player Instance;

    enum State
    {
        alive,
        dead
    }
    State state = State.alive;

    [HideInInspector] public Controller controller;
    [HideInInspector] public CameraEffects cameraEffects;

    #endregion

    #region PARTICLE SYSTEM
    [Header("PARTICLE SYSTEM")]
    public ParticleSystem dustParticle;
    public ParticleSystem deathParticle;
    #endregion

    #region BASIC MOVEMENT
    #region MOVEMENT
    [Header("MOVEMENT")]
    public float walkSpeed = 2.5f;
    public float runSpeed = 11f;
    public float accelerationGrounded = .05f;
    public float accelerationAirborne = .1f;
    public float clampedFallSpeed = 30f;

    public bool canMove = true;
    private bool isCrouching;
    private bool isWalking;
    private bool isGrounded;

    #endregion

    #region JUMP
    [Header("JUMP")]
    public float maxJumpHeight = 3f;
    public float minJumpHeight = .5f;
    public float timeToJumpApex = .4f;

    //public bool useFallMultiplier;
    [SerializeField] private float fallMultiplier = 1.5f;

    private float coyoteTime = .2f;
    private float coyoteTimeCounter;

    [HideInInspector] public float jumpBufferTime = .2f;
    [HideInInspector] public float jumpBufferTimeCounter;

    [HideInInspector] public bool isJumping;

    private float maxJumpVelocity;
    private float minJumpVelocity;
    private float gravity;
    
    #endregion
    
    #region ROLL
    [Header("ROLL")]
    public float rollDistance = 7f;
    public float rollTime = 0.3f; // .175f;
    private bool canRoll;
    private bool isRolling;

    #endregion
    
    #region GLIDE
    [Header("GLIDE")]
    public float glideGravityMultiplier = 0.05f;
    private float glideGravity;
    [HideInInspector] public bool isGliding;
    
    #endregion
    
    #region WALLSLIDE
    [Header("WALL SLIDE")] 
    public float wallSlideSpeedMax = 4.5f;
    public float wallStickTime = 0.1f;
    [SerializeField] private Vector2 wallJump = new Vector2(33f, 24f);

    [SerializeField] private bool canSlideOnObjects;
    private bool isWallSliding;

    private float timeToWallUnstick;
    private int wallDirX;

    #endregion

    #region SLASH
    [Header("SLASH")]
    public bool hasSlashedMidAir; //Returns true if the player is in mid-air after slashing

    #endregion

    #endregion

    #region BOOMERANG
    [Header("BOOMERANG")]
    public bool mouseAim;
    [HideInInspector] public bool isBoomeranging;
    //private Boomerang boomerang;
    private BoomerangV2 boomerang;

    #endregion

    #region INTERACTION
    [Header("INTERACTION")]
    private bool canInteract;
    private bool isInteracting;

    private bool canPushObject;
    private bool isPushingObject;

    [HideInInspector] public bool _interactInp;

    #endregion

    #region Other
    [HideInInspector] public Vector3 velocity;
    private float velocityXSmoothing;

    private Vector4 directionalInput; // z = isCrouching, w = isInteracting
    
    #endregion
    
    #endregion
    
    void Awake()
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

    void Start()
    {
        controller = GetComponent<Controller>();
        //boomerang = GameObject.FindGameObjectWithTag("Boomerang").GetComponent<Boomerang>();
        boomerang = GameObject.FindGameObjectWithTag("Boomerang").GetComponent<BoomerangV2>();

        maxJumpHeight *= transform.localScale.y;
        minJumpHeight *= transform.localScale.y;

        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
        glideGravity = gravity * glideGravityMultiplier;
    }

    void Update()
    {
        if(state == State.alive)
            HandleInteractions();
            if (canMove)
            {
                CalculateVelocity();
                Flip();
                HandleJump();
                HandleGlide();
                HandleCrouch();
                StartCoroutine(HandleRoll());
                HandlePushObject();
                HandleWallSliding();
                HandleClampedFallSpeed();
                controller.Move(velocity * Time.deltaTime, directionalInput);
            }


            if (controller.collisionData.below)
            {
                isGrounded = true;
                if (!isRolling) canRoll = true;
                isJumping = false;
                hasSlashedMidAir = false;
                isGliding = false;
                coyoteTimeCounter = coyoteTime;
            }
            else
            {
                isGrounded = false;
                coyoteTimeCounter -= Time.deltaTime;
            }

            if (isGrounded || controller.collisionData.above)
            {
                if (controller.collisionData.isSlidingDownSlope)
                {
                    velocity.y += controller.collisionData.slopeNormal.y * -gravity * Time.deltaTime;
                }
                else
                {
                    velocity.y = 0f;
                }
            }            
    }

    #region Methods
    #region Other
    void CalculateVelocity()
    {
        if (canMove)
        {
            if (Mathf.Abs(directionalInput.x) > 0f && isGrounded)
            {
                CreateDust();
            }

            float targetVelocityX;

            if (isWalking || isPushingObject)
            {
                targetVelocityX = directionalInput.x * walkSpeed;
            }
            else
            {
                targetVelocityX = directionalInput.x * runSpeed;
            }

            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (isGrounded) ? accelerationGrounded : accelerationAirborne);
        }
        else
        {
            velocity.x = 0f;
            directionalInput = Vector2.zero;
        }

        HandleFall();
    }

    void HandleFall()
    {
        if ((isJumping || hasSlashedMidAir) && velocity.y < 0.1f)
        {
            velocity.y += gravity * Time.deltaTime * fallMultiplier;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        if (directionalInput.y < 0f && isJumping)
        {
            velocity.y += gravity * Time.deltaTime * 500f;
        }
    }

    void HandleClampedFallSpeed()
    {
        if (!isWallSliding && !isGrounded)
        {
            if (velocity.y < -clampedFallSpeed)
            {
                velocity.y = -clampedFallSpeed;
            }
        }
    }

    void Flip()
    {
        if (!controller.collisionData.isPushingObject)
        {
            transform.localScale = new Vector2(controller.collisionData.faceDir, transform.localScale.y);
        }
    }

    public void KillPlayer()
    {
        if (isRolling)
            return;

        state = State.dead;
        //TODO Animate Death
        //cameraEffects.Shake(500f, 1f);
        //PlayerAnim.Instance.DeathAnim(.5f);

        print("Kill player");
        //Destroy(gameObject);
    }

    //Particle System
    private void CreateDust()
    {
        //Instantiate(dust, transform.position + new Vector3(0f, -0.498f), Quaternion.identity);
        dustParticle.Play();
    }
    #endregion

    #region Mechanics
    void HandleJump()
    {
        if (coyoteTimeCounter > 0f && jumpBufferTimeCounter > 0f)
        {
            isJumping = true;

            if (controller.collisionData.isSlidingDownSlope)
            {
                if (directionalInput.x != -Mathf.Sign(controller.collisionData.slopeNormal.x))
                {
                    velocity.y = maxJumpVelocity * controller.collisionData.slopeNormal.y;
                    velocity.x = maxJumpVelocity * controller.collisionData.slopeNormal.x;
                }
            }
            else
            {
                velocity.y = maxJumpVelocity;
            }

            coyoteTimeCounter = 0f;
            jumpBufferTimeCounter = 0f;
            //cameraEffects.Shake(50f, 0.35f);
            CreateDust();
        }

        jumpBufferTimeCounter -= Time.deltaTime;
    }


    IEnumerator HandleRoll()
    {
        if (canMove && isGrounded && isRolling)
        {
            canRoll = false;
            isCrouching = true;
            float rollVelocity = rollDistance / rollTime;

            if (InputManager.Instance.moveAction.ReadValue<Vector2>().x == 0) { 
                velocity.x = rollVelocity * controller.collisionData.faceDir;
            }
            else
            {
                velocity.x = rollVelocity * (InputManager.Instance.moveAction.ReadValue<Vector2>().x > 0 ? 1 : -1);
            }

            velocity.y = 0f;

            CreateDust();

            yield return new WaitForSeconds(rollTime);

            velocity.x = Mathf.Epsilon * controller.collisionData.faceDir;
            isRolling = false;
            isCrouching = false;
        }
    }


    void HandleWallSliding()
    {
        if ((controller.collisionData.left || controller.collisionData.right) && !controller.collisionData.below && velocity.y < 0)
        {
            if (canPushObject)
            {
                if (canSlideOnObjects)
                {
                    isWallSliding = true;
                }
                else
                {
                    isWallSliding = false;
                }
            }
            else if (controller.collisionData.wallAhead)
            {
                isWallSliding = true;
            }
        }
        else
        {
            isWallSliding = false;
        }

        if (isWallSliding)
        {
            wallDirX = (controller.collisionData.left) ? -1 : 1;

            if ((controller.collisionData.left || controller.collisionData.right) && !isGrounded && velocity.y < 0)
            {
                if (velocity.y < -wallSlideSpeedMax)
                {
                    velocity.y = -wallSlideSpeedMax;
                }

                if (timeToWallUnstick > 0)
                {
                    velocityXSmoothing = 0;
                    velocity.x = 0;

                    if (directionalInput.x != wallDirX && directionalInput.x != 0)
                    {
                        timeToWallUnstick -= Time.deltaTime;
                    }
                    else
                    {
                        timeToWallUnstick = wallStickTime;
                    }
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                }
            }
        }
    }


    void HandleGlide()
    {
        if (isGliding)
        {
            if(velocity.y < glideGravity)
            {
                velocity.y = glideGravity;
            }
        }
    }


    void HandleCrouch()
    {
        if (isCrouching)
        {
            directionalInput = new Vector4(directionalInput.x, directionalInput.y, 1f, directionalInput.w);
        }
        else
        {
            directionalInput = new Vector4(directionalInput.x, directionalInput.y, 0f, directionalInput.w);
        }
    }


    void HandlePushObject()
    {
        if (canInteract)
        {
            canPushObject = controller.collisionData.canPushObject;

            if (canPushObject)
            {
                isPushingObject = controller.collisionData.isPushingObject;
            }
            else
            {
                isPushingObject = false;
            }
        }
        else
        {
            canPushObject = false;
            isPushingObject = false;
        }
    }

    
    void HandleInteractions()
    {
        canInteract = controller.collisionData.canInteract;

        if (_interactInp)
        {
            directionalInput = new Vector4(directionalInput.x, directionalInput.y, directionalInput.z, 1f);
            if (!canPushObject)
            {
                _interactInp = false;
            }
        }
        else
        {
            directionalInput = new Vector4(directionalInput.x, directionalInput.y, directionalInput.z, 0f);
        }
        
        if (canInteract)
        {
            isInteracting = controller.collisionData.isInteracting;
        }
        else
        {
            isInteracting = false;
            _interactInp = false;
        }

        if (isInteracting && !canPushObject)
        {
            canMove = false;
        }
        else
        {
            canMove = true;
        }
    }


    void HandleBoomerang()
    {
        if (!isBoomeranging)
        {
            isBoomeranging = true;
            if (mouseAim)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(InputManager.Instance.mousePosAction.ReadValue<Vector2>());
                Vector2 angle = (mousePos - transform.position).normalized;
                boomerang.ActivateBoomerang(angle);
            }
            else
            {
                if(directionalInput.x == 0 && directionalInput.y == 0)
                {
                    boomerang.ActivateBoomerang(new Vector2(controller.collisionData.faceDir, directionalInput.y));
                }
                else
                {
                    boomerang.ActivateBoomerang(directionalInput);
                }
            }
        }
        else
        {
            float offsetX = boomerang.teleportOffset.x == 0 ? -boomerang.teleportOffset.y : boomerang.teleportOffset.x;
            float offsetY = boomerang.teleportOffset.z == 0 ? boomerang.teleportOffset.w : -boomerang.teleportOffset.z;
            Vector3 offset = new Vector2(offsetX, offsetY);

            transform.position = boomerang.transform.position + offset;
            boomerang.DeactivateBoomerang();
        }
    }
    #endregion

    #endregion

    #region Input

    #region Directional Input
    public void SetDirectionalInput(Vector2 input)
    {
        directionalInput = input;
    }
    #endregion

    #region Jump
    public void OnJumpInputPressed()
    {
        if (isWallSliding)
        {
            velocity.x = -wallDirX * wallJump.x;
            velocity.y = wallJump.y;

            isWallSliding = false;
        }

        jumpBufferTimeCounter = jumpBufferTime;
    }

    public void OnJumpInputReleased()
    {
        if (velocity.y > minJumpVelocity)
        {
            velocity.y = minJumpVelocity;
        }
    }
    #endregion

    #region Glide
    public void OnGlideInputPressed()
    {
        if (isJumping)
        {
            isGliding = true;
        }
    }

    public void OnGlideInputReleased()
    {
        isGliding = false;
    }
    #endregion

    #region Crouch
    public void OnCrouchInputDown()
    {
        if (isGrounded)
        {
            isCrouching = true;
            //canMove = false;
        }
        else
        {
            isCrouching = false;
            //canMove = true;
        }
    }

    public void OnCrouchInputUp()
    {
        if (!controller.collisionData.above)
        {
            isCrouching = false;
            //canMove = true;
        }
    }
    #endregion

    #region Walk
    public void OnWalkInputPressed()
    {
        if (isGrounded)
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }
    }

    public void OnWalkInputReleased()
    {
        isWalking = false;
    }
    #endregion

    #region Roll
    public void OnRollInput()
    {
        if (canRoll)
        {
            isRolling = true;
            //cameraEffects.Shake(8f, 0.01f);
            //impulseSource.GenerateImpulse(new Vector3(10, 10));
        }
    }
    #endregion

    #region Interact
    public void OnInteractInput()
    {
        if (canInteract)
        {
            _interactInp = !_interactInp;
        }
    }
    #endregion

    #region Boomerang
    public void OnBoomerangInput()
    {
        HandleBoomerang();
    }
    #endregion

    #region Sharingan
    public void OnSharinganInputPressed()
    {
        // isUsingSharingan = true;
        Chakra.Instance.isUsingSharingan = true;
    }

    public void OnSharinganInputReleased()
    {
        // isUsingSharingan = false;
        Chakra.Instance.isUsingSharingan = false;
    }
    #endregion
    
    #region Aminotejikara
    public void OnRinneganInputPressed()
    {
        Chakra.Instance.isUsingRinnegan = true;
    }

    public void OnRinneganInputReleased()
    {
        Chakra.Instance.isUsingRinnegan = false;
    }
    #endregion

    #region Throwable
    public void OnThrowableInput()
    {
        Attack.Instance.HandleThrowable();
    }

    #endregion

    #endregion
}
