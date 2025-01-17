using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    PlayerInputs playerInputs;

    [Header("Inputs")]
    [SerializeField] private Vector2 moveInput;
    [SerializeField] private bool jumpInput;
    [SerializeField] private bool dashInput;
    [SerializeField] private bool attackInput;
    [SerializeField] private bool cloneInput;
    [SerializeField] private bool teleportInput;
    private bool needNewCloneInput;
    private bool needNewTeleportInput;
    private bool needNewAttackInput;
    private bool needNewJumpInput;
    private bool needNewDashInput;

    #region Getters and Setters
    public Vector2 MoveInput { get { return moveInput; } }
    public bool JumpInput { get {return jumpInput; } }
    public bool DashInput { get { return dashInput; } }
    public bool AttackInput { get { return attackInput; } }
    public bool CloneInput { get { return cloneInput; } }
    public bool TeleportInput { get { return teleportInput; } }
    public bool NeedNewDashInput { get {return needNewDashInput;} set { needNewDashInput = value; } }
    public bool NeedNewJumpInput { get { return needNewJumpInput; } set { needNewJumpInput = value; } }
    public bool NeedNewAttackInput { get { return needNewAttackInput; } set { needNewAttackInput = value; } }
    public bool NeedNewCloneInput { get { return needNewCloneInput; } set { needNewCloneInput = value; } }
    public bool NeedNewTeleportInput { get { return needNewTeleportInput; } set { needNewTeleportInput = value; } }
    #endregion
    private void OnEnable()
    {
        playerInputs.Enable();
    }
    private void OnDisable()
    {
        playerInputs.Disable();
    }
    private void Awake()
    {
        playerInputs = new PlayerInputs();


        //Move inputs
        playerInputs.PlayerControls.Move.started += OnMoveInput;
        playerInputs.PlayerControls.Move.performed += OnMoveInput;
        playerInputs.PlayerControls.Move.canceled += OnMoveInput;

        //Jump input
        playerInputs.PlayerControls.Jump.started += OnJumpInput;
        playerInputs.PlayerControls.Jump.performed += OnJumpInput;
        playerInputs.PlayerControls.Jump.canceled += OnJumpInput;

        //Dash inputs
        playerInputs.PlayerControls.Dash.started += OnDashInput;
        playerInputs.PlayerControls.Dash.performed += OnDashInput;
        playerInputs.PlayerControls.Dash.canceled += OnDashInput;

        //Dash inputs
        playerInputs.PlayerControls.Attack.started += OnAttackInput;
        playerInputs.PlayerControls.Attack.performed += OnAttackInput;
        playerInputs.PlayerControls.Attack.canceled += OnAttackInput;

        //Clone Input
        playerInputs.PlayerControls.Clone.started += OnCloneInput;
        playerInputs.PlayerControls.Clone.canceled += OnCloneInput;

        //Teleport Input
        playerInputs.PlayerControls.Teleport.started += OnTeleportInput;
        playerInputs.PlayerControls.Teleport.canceled += OnTeleportInput;

    }

    private void OnMoveInput(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnJumpInput(InputAction.CallbackContext context)
    {
        jumpInput = context.ReadValueAsButton();
        if(jumpInput)
        {
            needNewJumpInput=false;
        }
    }

    private void OnDashInput(InputAction.CallbackContext context)
    {
        dashInput = context.ReadValueAsButton();
        if(dashInput)
        {
            needNewDashInput=false;
        }
    }

    private void OnAttackInput(InputAction.CallbackContext context)
    {
        attackInput = context.ReadValueAsButton();
        if(attackInput)
        {
            needNewAttackInput=false;
        }
    }
    private void OnCloneInput (InputAction.CallbackContext context)
    {
        cloneInput = context.ReadValueAsButton();
        if (cloneInput)
        {
            needNewCloneInput = false;
        }
    }
    private void OnTeleportInput(InputAction.CallbackContext context)
    {
        teleportInput = context.ReadValueAsButton();
        if (teleportInput)
        {
            needNewTeleportInput = false;
        }
    }
}
