using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Parameters")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    
    [Header("Combat Parameters")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackDamage = 25f;
    [SerializeField] private float attackCooldown = 0.5f;
    
    // Movement and combat state management
    private bool isGrounded;
    private bool isWallSliding;
    private bool canDoubleJump;
    private bool isDashing;
    private bool canAttack = true;
    
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    private void Update()
    {
        HandleMovement();
        HandleJump();
        HandleWallSlide();
        HandleDash();
        HandleCombat();
    }
    
    private void HandleMovement()
    {
        if (isDashing) return;
        
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        
        if (moveInput != 0)
        {
            spriteRenderer.flipX = moveInput < 0;
            animator.SetBool("IsRunning", true);
        }
        else
        {
            animator.SetBool("IsRunning", false);
        }
    }
    
    private void HandleCombat()
    {
        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            Attack();
        }
    }
    
    private void Attack()
    {
        canAttack = false;
        animator.SetTrigger("Attack");
        
        // Combat logic implementation
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange, LayerMask.GetMask("Enemy"));
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<EnemyHealth>()?.TakeDamage(attackDamage);
        }
        
        Invoke(nameof(ResetAttack), attackCooldown);
    }
    
    private void ResetAttack()
    {
        canAttack = true;
    }
}