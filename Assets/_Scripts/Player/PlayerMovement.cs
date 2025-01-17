using System.Xml.Serialization;
using UnityEditor.Animations;
using UnityEngine;
[RequireComponent (typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour,IAnimationState
{
    [Header("Referencias")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] public CharacterData data;
    [SerializeField] private AnimatorManager animatorManager;
    private PlayerAnimationState animationState = new PlayerAnimationState();

    #region Estado del Personaje
    private bool isGrounded;
    private bool isJumping;
    private bool isDashing;
    private bool isWallSliding;
    private bool canDoubleJump = true;
    #endregion

    #region Variables de Movimiento
    private float currentSpeed;
    private float dashTimeLeft;
    private float dashCooldownLeft;
    private float coyoteTimeLeft;
    private float jumpBufferTimeLeft;
    private float wallJumpCoolDownLeft;
    private float wallDirectionX;
    private Vector2 movementInput;
    #endregion

    #region Variables de Comprobación Física
    private ContactFilter2D groundFilter;
    private ContactFilter2D wallFilter;
    private RaycastHit2D[] groundHits = new RaycastHit2D[4];
    private RaycastHit2D[] wallHits = new RaycastHit2D[2];
    #endregion

    private Transform playerTransform;

    #region Implementación de Interfaces
    // Implementación de IMovementState/IAnimationState
    bool IMovementState.IsGrounded => isGrounded;
    bool IMovementState.IsJumping => isJumping;
    bool IMovementState.IsFalling => rb.linearVelocity.y < -0.1f;
    bool IMovementState.IsWallSliding => isWallSliding;
    bool IMovementState.IsDashing => isDashing;
    float IMovementState.MovementSpeed => Mathf.Abs(rb.linearVelocity.x);
    Vector2 IMovementState.MovementDirection => movementInput;

    // Propiedades públicas para uso interno
    public bool IsGrounded => isGrounded;
    public bool IsJumping => isJumping;
    public bool IsFalling => rb.linearVelocity.y < -0.1f;
    public bool IsWallSliding => isWallSliding;
    public bool IsDashing => isDashing;
    public float MovementSpeed => Mathf.Abs(rb.linearVelocity.x);
    public Vector2 MovementDirection => movementInput;
    #endregion

    private void Awake()
    {
        InitializeComponents();
        InitializeCollisionFilters();
    }

    private void FixedUpdate()
    {
        UpdatePhysicsState();

        if (dashCooldownLeft > 0)
        {
            dashCooldownLeft -= Time.fixedDeltaTime;
        }
    }

    private void Update()
    {
        UpdateAnimationState();
    }

    #region Inicialización
    private void InitializeComponents()
    {
        playerTransform = transform;
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!animatorManager) animatorManager = GetComponent<AnimatorManager>();
    }

    private void InitializeCollisionFilters()
    {
        // Configurar filtro de suelo
        groundFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = data.groundLayer
        };

        // Configurar filtro de paredes
        wallFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = data.wallLayer
        };
    }
    #endregion

    #region Manejo de Input
    public void HandleMovement(Vector2 input)
    {
        movementInput = input;
        ApplyMovement();
    }

    public void HandleJumpInput(bool jumpPressed)
    {
        if (jumpPressed)
        {
            jumpBufferTimeLeft = data.jumpBufferTime;
            TryJump();
        }
        else if (rb.linearVelocity.y > 0 && isJumping)
        {
            // Soltar el salto temprano para altura variable
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * data.minJumpMultiplier);
            isJumping = false;
        }
    }

    public void HandleDashInput(bool dashPressed)
    {
        if (dashPressed && dashCooldownLeft <= 0 && !isDashing)
        {
            StartDash();
        }
    }
    #endregion

    #region Métodos de Movimiento
    private void ApplyMovement()
    {
        // Manejar casos especiales primero
        if (isDashing)
        {
            HandleDashMovement();
            return;
        }

        if (wallJumpCoolDownLeft > 0)
        {
            wallJumpCoolDownLeft -= Time.fixedDeltaTime;
            return;
        }

        float targetSpeed = movementInput.x * data.maxSpeed;
        float currentSpeed = rb.linearVelocity.x;

        // Manejo de la desaceleración cuando no hay input
        if (Mathf.Abs(movementInput.x) < 0.01f)
        {
            // Aplicar desaceleración suave usando interpolación
            float smoothDeceleration = Mathf.Lerp(
                currentSpeed,
                0f,
                Time.fixedDeltaTime / data.decelerationTime
            );

            rb.linearVelocity = new Vector2(smoothDeceleration, rb.linearVelocity.y);
            UpdateGravityScale();
            return;
        }

        // Cálculo de velocidad objetivo usando interpolación suave
        float newSpeed;
        if (Mathf.Sign(targetSpeed) != Mathf.Sign(currentSpeed))
        {
            // Cambio de dirección: usar turnSpeed para una transición más suave
            float turnRate = Time.fixedDeltaTime / data.turnSpeed;
            newSpeed = Mathf.Lerp(currentSpeed, targetSpeed, turnRate);
        }
        else
        {
            // Misma dirección: usar accelerationTime para el movimiento normal
            float accelerationRate = Time.fixedDeltaTime / data.accelerationTime;
            newSpeed = Mathf.Lerp(currentSpeed, targetSpeed, accelerationRate);
        }

        // Aplicar modificador de control aéreo
        if (!isGrounded)
        {
            // Reducir el cambio de velocidad en el aire
            newSpeed = Mathf.Lerp(currentSpeed, newSpeed, data.airControlMultiplier);
        }

        // Aplicar la velocidad final
        rb.linearVelocity = new Vector2(newSpeed, rb.linearVelocity.y);

        UpdateGravityScale();
    }

    private void UpdateGravityScale()
    {
        if (rb.linearVelocity.y < 0)
        {
            // Caída
            rb.gravityScale = data.fallGravityMultiplier;
        }
        else if (rb.linearVelocity.y > 0 && rb.linearVelocity.y < data.apexThreshold)
        {
            // Cerca del punto más alto
            rb.gravityScale = data.apexGravityMultiplier;
        }
        else if (rb.linearVelocity.y > 0)
        {
            // Subiendo
            rb.gravityScale = data.ascendingGravityMultiplier;
        }
        else
        {
            rb.gravityScale = 1f;
        }

        // Limitar velocidad de caída
        if (rb.linearVelocity.y < -data.maxFallSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -data.maxFallSpeed);
        }
    }

    private void TryJump()
    {
        if (isGrounded || coyoteTimeLeft > 0)
        {
            PerformJump(data.jumpForce);
            canDoubleJump = true;
        }
        else if (isWallSliding)
        {
            PerformWallJump();
        }
        else if (canDoubleJump)
        {
            PerformDoubleJump();
        }
    }

    private void PerformJump(float jumpForce)
    {
        isJumping = true;
        coyoteTimeLeft = 0;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        ApplyJumpSquash();
    }

    private void PerformWallJump()
    {
        isWallSliding = false;
        isJumping = true;
        wallJumpCoolDownLeft = data.wallJumpControlCooldown;

        Vector2 wallJumpForce = new Vector2(-wallDirectionX * data.wallJumpForce, data.wallJumpForce);
        rb.linearVelocity = wallJumpForce;
    }

    private void PerformDoubleJump()
    {
        canDoubleJump = false;
        isJumping = true;
        PerformJump(data.jumpForce * 0.8f);
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimeLeft = data.dashDuration;
        dashCooldownLeft = data.dashCooldown;

        float dashDirection = movementInput.x != 0 ?
            Mathf.Sign(movementInput.x) :
            Mathf.Sign(playerTransform.localScale.x);
        rb.linearVelocity = new Vector2(dashDirection * data.dashSpeed, 0);

        ApplyDashEffects();
    }

    private void HandleDashMovement()
    {
        dashTimeLeft -= Time.fixedDeltaTime;

        if (dashTimeLeft <= 0)
        {
            EndDash();
        }
    }

    private void EndDash()
    {
        isDashing = false;
        if (data.preserveDashMomentum)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                rb.linearVelocity.y * data.dashEndVerticalMomentum
            );
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
    #endregion

    #region Comprobaciones de Estado
    private void UpdatePhysicsState()
    {
        UpdateGroundedState();
        UpdateWallState();
        HandleCoyoteTime();
        HandleJumpBuffer();
    }

    private void UpdateGroundedState()
    {
        Vector2 boxCenter = (Vector2)transform.position + data.groundCheckOffset;
        Vector2 boxSize = new Vector2(data.groundCheckWidth, data.groundCheckDistance);

        int hitCount = Physics2D.BoxCast(
            boxCenter,
            boxSize,
            0f,
            Vector2.down,
            groundFilter,
            groundHits,
            data.groundCheckDistance
        );

        bool wasGrounded = isGrounded;
        isGrounded = hitCount > 0;

        if (wasGrounded && !isGrounded)
        {
            coyoteTimeLeft = data.coyoteTime;
        }

        if (isGrounded)
        {
            canDoubleJump = true;
            isJumping = false;
            ApplyLandingSquash();
        }

        if (Debug.isDebugBuild)
        {
            Color debugColor = isGrounded ? Color.green : Color.red;
            DrawDebugCube(boxCenter, boxSize, debugColor);
        }
    }

    private void UpdateWallState()
    {
        Vector2 boxCenter = (Vector2)transform.position + new Vector2(0, data.wallCheckVerticalOffset);
        Vector2 boxSize = new Vector2(data.wallCheckDistance, data.wallCheckHeight);

        int rightHitCount = Physics2D.BoxCast(
            boxCenter,
            boxSize,
            0f,
            Vector2.right,
            wallFilter,
            wallHits,
            data.wallCheckDistance
        );

        int leftHitCount = Physics2D.BoxCast(
            boxCenter,
            boxSize,
            0f,
            Vector2.left,
            wallFilter,
            wallHits,
            data.wallCheckDistance
        );

        bool wasWallSliding = isWallSliding;
        isWallSliding = false;
        wallDirectionX = 0;

        if (!isGrounded && rb.linearVelocity.y < 0)
        {
            if (rightHitCount > 0 && movementInput.x > 0)
            {
                isWallSliding = true;
                wallDirectionX = 1;
            }
            else if (leftHitCount > 0 && movementInput.x < 0)
            {
                isWallSliding = true;
                wallDirectionX = -1;
            }
        }

        if (isWallSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -data.wallSlideSpeed);
        }

        if (Debug.isDebugBuild)
        {
            DrawDebugCube(boxCenter + Vector2.right * data.wallCheckDistance, boxSize,
                rightHitCount > 0 ? Color.green : Color.red);
            DrawDebugCube(boxCenter + Vector2.left * data.wallCheckDistance, boxSize,
                leftHitCount > 0 ? Color.green : Color.red);
        }
    }

    private void HandleCoyoteTime()
    {
        if (!isGrounded && coyoteTimeLeft > 0)
        {
            coyoteTimeLeft -= Time.fixedDeltaTime;
        }
    }

    private void HandleJumpBuffer()
    {
        if (jumpBufferTimeLeft > 0)
        {
            jumpBufferTimeLeft -= Time.fixedDeltaTime;
            if ((isGrounded || isWallSliding) && jumpBufferTimeLeft > 0)
            {
                jumpBufferTimeLeft = 0;
                TryJump();
            }
        }
    }
    #endregion

    #region Efectos Visuales
    private void UpdateAnimationState()
    {
        animationState.IsGrounded = IsGrounded;
        animationState.IsJumping = IsJumping;
        animationState.IsFalling = IsFalling;
        animationState.IsWallSliding = IsWallSliding;
        animationState.IsDashing = IsDashing;
        animationState.MovementSpeed = MovementSpeed;
        animationState.MovementDirection = MovementDirection;

        animatorManager.UpdateAnimationState(animationState);
    }

    private void ApplyJumpSquash()
    {
        // Aplicar efecto de estiramiento al saltar
        Vector3 newScale = playerTransform.localScale;
        newScale.y *= (1 + data.jumpStretchAmount);
        newScale.x *= (1 - data.jumpStretchAmount * 0.5f);
        playerTransform.localScale = newScale;

        // Restaurar la escala después de la duración especificada
        Invoke(nameof(ResetScale), data.squashStretchDuration);
    }

    private void ApplyLandingSquash()
    {
        // Aplicar efecto de aplastamiento al aterrizar
        Vector3 newScale = playerTransform.localScale;
        newScale.y *= (1 - data.landSquashAmount);
        newScale.x *= (1 + data.landSquashAmount * 0.5f);
        playerTransform.localScale = newScale;

        // Restaurar la escala después de la duración especificada
        Invoke(nameof(ResetScale), data.squashStretchDuration);
    }

    private void ApplyDashEffects()
    {
        // Aquí se pueden implementar efectos visuales adicionales para el dash
        // Por ejemplo:
        // - Sistema de partículas
        // - Trail renderer
        // - Efectos de post-procesado temporales
        // - Efectos de sonido
    }

    private void ResetScale()
    {
        // Restaurar la escala original del personaje
        playerTransform.localScale = Vector3.one;
    }
    #endregion

    #region Utilidades de Debug
    private static void DrawDebugCube(Vector2 position, Vector2 size, Color color)
    {
        // Calcular las esquinas del cubo para visualización en el editor
        Vector2 halfSize = size * 0.5f;

        Vector2 topLeft = position + new Vector2(-halfSize.x, halfSize.y);
        Vector2 topRight = position + halfSize;
        Vector2 bottomRight = position + new Vector2(halfSize.x, -halfSize.y);
        Vector2 bottomLeft = position + new Vector2(-halfSize.x, -halfSize.y);

        // Dibujar las líneas que forman el cubo
        Debug.DrawLine(topLeft, topRight, color);
        Debug.DrawLine(topRight, bottomRight, color);
        Debug.DrawLine(bottomRight, bottomLeft, color);
        Debug.DrawLine(bottomLeft, topLeft, color);
    }
    #endregion
}
