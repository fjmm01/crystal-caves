using System;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerCombatManager : MonoBehaviour,ICombatState,IDamageable
{
    [Header("Referencias")]
    [SerializeField] private CharacterData data;
    [SerializeField] private AnimatorManager animatorManager;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private PlayerMovement playerMovement;
    private PlayerStatsManager statsManager;
    private IAttackValidator attackValidator;
    private bool isAttacking;
    private float attackCooldownTimer;
    private Vector2 lastMoveDirection;
    private PlayerCombatState combatState = new PlayerCombatState();
    private bool isKnockedBack;
    private float knockbackTimer;
    private ParticleSystem damageParticles;

    // Implementación de ICombatState
    public bool IsAttacking => isAttacking;
    public string CurrentAttackType => GetCurrentAttackType();

    private void Awake()
    {
        InitializeComponents();
        attackValidator = new DefaultAttackValidator();
    }
    private void InitializeComponents()
    {
        playerMovement = GetComponent<PlayerMovement>();
        statsManager = GetComponent<PlayerStatsManager>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }
    private void Start()
    {
        //Subscribirse a eventos del sistema de salud
        statsManager.Health.OnHealthChanged += HandleHealthChanged;
        statsManager.Health.OnDeath += HandlePlayerDeath;
    }
    private void OnDestroy()
    {
        // Limpieza de eventos
        if (statsManager != null)
        {
            statsManager.Health.OnHealthChanged -= HandleHealthChanged;
            statsManager.Health.OnDeath -= HandlePlayerDeath;
        }
    }
    private void Update()
    {
        UpdateAttackState();
        UpdateLastMoveDirection();
        UpdateAnimationState();
        UpdateKnockbackState();
    }
    private void UpdateKnockbackState()
    {
        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0)
            {
                isKnockedBack = false;
            }
        }
    }
    private void UpdateAttackState()
    {
        if(attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
        }
    }

    public void HandleAttackInput(Vector2 moveDirection, bool attackPressed)
    {
        if (!attackPressed || attackCooldownTimer > 0 || isAttacking)
        {
            return;
        }

        // Determinar la dirección del ataque
        Vector2 attackDirection;
        if (moveDirection.sqrMagnitude > 0.1f)
        {
            attackDirection = moveDirection.normalized;
        }
        else
        {
            // Usar la dirección a la que mira el personaje para ataques horizontales
            attackDirection = new Vector2(spriteRenderer.flipX ? -1 : 1, 0);
        }

        // Crear los datos de validación
        var validationData = new AttackValidationData(
            attackDirection,
            playerMovement.IsGrounded,
            playerMovement.IsDashing
        );

        // Validar si podemos realizar el ataque
        if (attackValidator.CanPerformAttack(validationData.GetAttackDirection(), playerMovement))
        {
            lastMoveDirection = attackDirection;
            PerformAttack();
        }
    }

    private void UpdateLastMoveDirection()
    {
        Vector2 currentMovement = playerMovement.MovementDirection;
        if (currentMovement.sqrMagnitude > 0.1f)
        {
            lastMoveDirection = currentMovement.normalized;
        }
    }

    private void PerformAttack()
    {
        isAttacking = true;
        attackCooldownTimer = data.attackCooldown;

        bool isVerticalAttack = Mathf.Abs(lastMoveDirection.y) > Mathf.Abs(lastMoveDirection.x);
        Vector2 hitboxSize = isVerticalAttack ? data.verticalHitboxSize : data.horizontalHitboxSize;
        Vector2 hitboxOffset = CalculatehitboxOffset(isVerticalAttack);

        // Detectar y procesar hits
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            (Vector2)transform.position + hitboxOffset,
            hitboxSize,
            0f,
            LayerMask.GetMask("Enemy")
        );

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                ProcessHit(damageable, hit.transform.position, isVerticalAttack);
            }
        }

        Invoke(nameof(EndAttack), data.attackDuration);
    }

    private void ProcessHit(IDamageable damageable, Vector2 targetPosition, bool isVerticalAttack)
    {
        Vector2 knockbackDirection = CalculateKnockbackDirection(targetPosition, isVerticalAttack);
        damageable.TakeDamage(new DamageData
        {
            damage = data.attackDamage,
            knockbackForce = knockbackDirection * data.attackKnockbackForce,
            knockbackDuration = data.knockbackDuration
        });

        ApplyHitEffects();
    }
    private Vector2 CalculatehitboxOffset(bool isVerticalAttack)
    {
        if(isVerticalAttack)
        {
            return new Vector2(0,lastMoveDirection.y * data.hitboxOffset);
        }

        // Para ataques horizontales, usamos la dirección del sprite si no hay input de movimiento
        float horizontalDirection = lastMoveDirection.x != 0 ?
            lastMoveDirection.x :
            (spriteRenderer.flipX ? -1 : 1);

        return new Vector2(lastMoveDirection.x * data.hitboxOffset, 0);
    }

    private Vector2 CalculateKnockbackDirection(Vector2 targetPosition, bool isVerticalAttack)
    {
        if(isVerticalAttack)
        {
            return new Vector2(0, lastMoveDirection.y).normalized;
        }
        // Para knockback horizontal, usamos la dirección del sprite si no hay input de movimiento
        if (lastMoveDirection.x == 0)
        {
            return new Vector2(spriteRenderer.flipX ? -1 : 1, 0);
        }
        return (targetPosition - (Vector2)transform.position).normalized;
    }

    private void ApplyHitEffects()
    {
        //TODO: Implementar hit stop
        //TODO: Implementar screen Shake
        //TODO: reproducir efectos de particulas
    }

    private void EndAttack()
    {
        isAttacking = false;
    }

    private void UpdateAnimationState()
    {
        combatState.IsAttacking = isAttacking;
        combatState.CurrentAttackType = GetCurrentAttackType();
        animatorManager.UpdateCombatState(combatState);
    }

    private string GetCurrentAttackType()
    {
        if (!isAttacking) return "None";

        var direction = new AttackValidationData(lastMoveDirection, playerMovement.IsGrounded, playerMovement.IsDashing)
            .GetAttackDirection();

        return direction.ToString();
    }

    private void OnDrawGizmos()
    {
        if (!isAttacking) return;

        bool isVerticalAttack = Mathf.Abs(lastMoveDirection.y) > Mathf.Abs(lastMoveDirection.x);
        Vector2 hitboxSize = isVerticalAttack ? data.verticalHitboxSize : data.horizontalHitboxSize;
        Vector2 hitboxOffset = CalculatehitboxOffset(isVerticalAttack);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((Vector2)transform.position + hitboxOffset, hitboxSize);
    }

    public void TakeDamage(DamageData damageData)
    {
        statsManager.Health.TakeDamage(damageData);

        if (!isKnockedBack && damageData.knockbackForce != Vector2.zero)
        {
            ApplyKnockback(damageData.knockbackForce, damageData.knockbackDuration);
        }

        PlayDamageEffects();
    }

    private void PlayDamageEffects()
    {
        
    }

    private void ApplyKnockback(Vector2 knockbackForce, float duration)
    {
        if (GetComponent<Rigidbody2D>() is Rigidbody2D rb)
        {
            rb.linearVelocity = knockbackForce;
            isKnockedBack = true;
            knockbackTimer = duration;
        }
    }

    private void HandleHealthChanged(float newHealth)
    {
        // Aquí puedes agregar efectos visuales o sonoros cuando el jugador recibe daño
        if (damageParticles != null)
        {
            damageParticles.Play();
        }
    }

    private void HandlePlayerDeath()
    {
        // Deshabilitar el input y movimiento
        enabled = false;
        playerMovement.enabled = false;

        // Activar animación de muerte
        animatorManager.PlayAnimation("Death");

        // Puedes agregar aquí la lógica de game over o respawn
    }
}
public struct DamageData
{
    public int damage;
    public Vector2 knockbackForce;
    public float knockbackDuration;
}
// Enum que define las posibles direcciones de ataque
public enum AttackDirection
{
    None,       // No hay ataque activo
    Horizontal, // Ataque lateral (izquierda o derecha)
    Up,         // Ataque hacia arriba
    Down        // Ataque hacia abajo
}

public readonly struct AttackValidationData
{
    // Propiedades de solo lectura para garantizar inmutabilidad
    public readonly Vector2 InputDirection { get; }
    public readonly bool IsGrounded { get; }
    public readonly bool IsDashing { get; }

    public AttackValidationData(Vector2 inputDirection, bool isGrounded, bool isDashing)
    {
        InputDirection = inputDirection;
        IsGrounded = isGrounded;
        IsDashing = isDashing;
    }

    // Método helper para determinar la dirección del ataque basado en el input
    public AttackDirection GetAttackDirection()
    {
        // Si no hay input significativo, asumimos ataque horizontal
        if (InputDirection.sqrMagnitude < 0.1f)
        {
            return AttackDirection.Horizontal;
        }

        // Determinar si el ataque es vertical u horizontal basado en el componente mayor
        if (Mathf.Abs(InputDirection.y) > Mathf.Abs(InputDirection.x))
        {
            return InputDirection.y > 0 ? AttackDirection.Up : AttackDirection.Down;
        }

        return AttackDirection.Horizontal;
    }
}
