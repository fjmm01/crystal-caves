using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] InputManager inputManager;
    [SerializeField] CharacterData data;
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] PlayerCombatManager playerCombatManager;
    [SerializeField] PlayerCloneController cloneController;
    [SerializeField] SpriteRenderer spriteRenderer;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        inputManager = GetComponent<InputManager>();
        playerMovement = GetComponent<PlayerMovement>();
        playerCombatManager = GetComponent<PlayerCombatManager>();
        cloneController = GetComponent<PlayerCloneController>();
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // Handle movement
        playerMovement.HandleMovement(inputManager.MoveInput);
        if(inputManager.MoveInput.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if(inputManager.MoveInput.x > 0)
        {
            spriteRenderer.flipX= false;
        }

        // Handle jump
        if (inputManager.JumpInput)
        {
            playerMovement.HandleJumpInput(true);
            inputManager.NeedNewJumpInput = true;
        }
        else
        {
            playerMovement.HandleJumpInput(inputManager.JumpInput);
        }

        // Handle dash
        if (inputManager.DashInput)
        {
            playerMovement.HandleDashInput(true);
            inputManager.NeedNewDashInput = true;
        }

        //Handle Attack
        if(inputManager.AttackInput)
        {
            playerCombatManager.HandleAttackInput(inputManager.MoveInput, true);
            inputManager.NeedNewAttackInput = true;
        }
        else
        {
            playerCombatManager.HandleAttackInput(inputManager.MoveInput, false);
        }
        
    }
}
