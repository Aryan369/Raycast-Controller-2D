using UnityEngine;
using UnityEngine.InputSystem;
//ReSharper disable All

[RequireComponent(typeof(Player), typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{
    #region Variables
    public static InputManager Instance;
    
    Player player;

    [HideInInspector] public PlayerInput playerInput;

    [HideInInspector] public InputAction moveAction;
    private InputAction jumpAction;
    private InputAction rollAction;
    private InputAction glideAction;
    private InputAction walkAction;
    private InputAction interactAction;
    private InputAction sharinganAction;
    private InputAction rinneganAction;
    private InputAction throwAction;
    private InputAction boomerangAction;
    [HideInInspector] public InputAction attackAction;
    [HideInInspector] public InputAction aminotejikaraAction;

    [HideInInspector] public InputAction mousePosAction;

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
        
        player = GetComponent<Player>();
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        rollAction = playerInput.actions["Roll"];
        glideAction = playerInput.actions["Glide"];
        walkAction = playerInput.actions["Walk"];
        interactAction = playerInput.actions["Interact"];
        sharinganAction = playerInput.actions["Sharingan"];
        rinneganAction = playerInput.actions["Rinnegan"];
        throwAction = playerInput.actions["Throw"];
        boomerangAction = playerInput.actions["Boomerang"];
        attackAction = playerInput.actions["Attack"];
        aminotejikaraAction = playerInput.actions["Aminotejikara"];

        mousePosAction = playerInput.actions["MousePos"];

        jumpAction.started += Jump;
        jumpAction.canceled += Jump;

        rollAction.started += Roll;
        rollAction.canceled += Roll;

        glideAction.started += Glide;
        glideAction.canceled += Glide;

        walkAction.started += Walk;
        walkAction.canceled += Walk;

        interactAction.performed += Interact;

        sharinganAction.started += Sharingan;
        sharinganAction.canceled += Sharingan;

        rinneganAction.started += Rinnegan;
        rinneganAction.canceled += Rinnegan;

        throwAction.performed += Throw;
        
        boomerangAction.performed += Boomerang;
    }

    void Update()
    {
        Vector2 directionalInput = moveAction.ReadValue<Vector2>();
        player.SetDirectionalInput(directionalInput);

        #region Crouch & Roll
        if (directionalInput.x == 0f)
        {
            if (directionalInput.y == -1f)
            {
                player.OnCrouchInputDown();
            }
            else if (directionalInput.y > -1f)
            {
                player.OnCrouchInputUp();
            }
        }

        if (rollAction.IsPressed())
        {
            if (directionalInput.x != 0f)
            {
                player.OnRollInput();
            }
        }
        #endregion
    }

    #region Methods
    #region Jump
    void Jump(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            player.OnJumpInputPressed();
        }

        if (ctx.canceled)
        {
            player.OnJumpInputReleased();
        }
    }
    #endregion

    #region Roll
    void Roll(InputAction.CallbackContext ctx)
    {
        Vector2 directionalInput = moveAction.ReadValue<Vector2>();

        if (directionalInput.x != 0f)
        {
            if (ctx.started)
            {
                player.OnRollInput();
            }
        }        
    }
    #endregion

    #region Glide
    void Glide(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            player.OnGlideInputPressed();
        }

        if (ctx.canceled)
        {
            player.OnGlideInputReleased();
        }
    }
    #endregion

    #region Walk
    void Walk(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            player.OnWalkInputPressed();
        }
    
        if (ctx.canceled)
        {
            player.OnWalkInputReleased();
        }
    }
    #endregion

    #region Interact
    void Interact(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            player.OnInteractInput();
        }
    }
    #endregion

    #region Sharingan
    void Sharingan(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            player.OnSharinganInputPressed();
        }
    
        if (ctx.canceled)
        {
            player.OnSharinganInputReleased();
        }
    }
    #endregion
    
    #region Rinnegan
    void Rinnegan(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            player.OnRinneganInputPressed();
        }
    
        if (ctx.canceled)
        {
            player.OnRinneganInputReleased();
        }
    }
    #endregion

    #region Throwable
    void Throw(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            player.OnThrowableInput();
        }
    }
    #endregion

    #region Boomerang
    void Boomerang(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            player.OnBoomerangInput();
        }
    }
    

    #endregion
    #endregion
}
