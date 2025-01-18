using System;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerCombatManager : MonoBehaviour,ICombatState,IDamageable
{
    [Header("References")]
    [SerializeField] private CharacterData data;
    [SerializeField] private AnimatorManager animatorManager;

    private PlayerMovement playerMovement;
    private PlayerStatsManager statsManager;
    private bool isAttacking;
    private float attackCooldownTimer;
    private Vector2 lastMoveDirection;
    private PlayerCombatState combatState = new PlayerCombatState();
    private bool isKnockedBack;
    private float knockbackTimer;
    private ParticleSystem damageParticles;

    //Implementacion de ICombatState
    public bool IsAttacking => isAttacking;
    public string CurrentAttackType => GetCurrentAttackType();

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        statsManager = GetComponent<PlayerStatsManager>();
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
        if(attackPressed && attackCooldownTimer <= 0 && !isAttacking)
        {
            lastMoveDirection = moveDirection.normalized;
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

        //Determinar la direccion del ataque
        bool isVerticalAttack = Mathf.Abs(lastMoveDirection.y) > Mathf.Abs(lastMoveDirection.x);

        //Crear hitbox segun la direccion
        Vector2 hitboxSize = isVerticalAttack ? data.verticalHitboxSize : data.horizontalHitboxSize;
        Vector2 hitboxOffset = CalculatehitboxOffset(isVerticalAttack);

        //Detectar hits
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            (Vector2)transform.position + hitboxOffset,
            hitboxSize,
            0f,
            LayerMask.GetMask("Enemy"));

        //Procesar hits
        foreach(Collider2D hit in hits)
        {
            if(hit.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                Vector2 knockbackDirection = CalculateKnockbackDirection(hit.transform.position, isVerticalAttack);
                damageable.TakeDamage(new DamageData
                {
                    damage = data.attackDamage,
                    knockbackForce = knockbackDirection * data.attackKnockbackForce,
                    knockbackDuration = data.knockbackDuration
                });

                ApplyHitEffects();
            }
        }
        //Reset del estado de ataque
        Invoke(nameof(EndAttack), data.attackDuration);
    }

    private Vector2 CalculatehitboxOffset(bool isVerticalAttack)
    {
        if(isVerticalAttack)
        {
            return new Vector2(0,lastMoveDirection.y * data.hitboxOffset);
        }
        return new Vector2(lastMoveDirection.x * data.hitboxOffset, 0);
    }

    private Vector2 CalculateKnockbackDirection(Vector2 targetPosition, bool isVerticalAttack)
    {
        if(isVerticalAttack)
        {
            return new Vector2(0, lastMoveDirection.y).normalized;
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
        
        bool isVerticalAttack = Mathf.Abs(lastMoveDirection.y) > Mathf.Abs(lastMoveDirection.x);
        if (isVerticalAttack)
        {
            return lastMoveDirection.y > 0 ? "Up" : "Down";
        }
        return "Horizontal";
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
        //Aplicar el daño al sistema de salud
        statsManager.Health.TakeDamage(damageData);

        //Aplicar knockback
        if(!isKnockedBack && damageData.knockbackForce != Vector2.zero)
        {
            ApplyKnockback(damageData.knockbackForce, damageData.knockbackDuration);
        }

        //Efectos visuales de daño
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
