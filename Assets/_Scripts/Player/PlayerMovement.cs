﻿using System.Collections;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;
[RequireComponent (typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour,IAnimationState
{

    [Header("References")]
    [SerializeField] public Rigidbody2D rb;
    [SerializeField] SpriteRenderer spriteRenderer;

    [Header("Referencias")]
    

    [SerializeField] public CharacterData data;
    [SerializeField] AnimatorManager animatorManager;
    private PlayerAnimationState animationState = new PlayerAnimationState();


    #region StateFlags
    private bool isGrounded;
    private bool isJumping;
    private bool isDashing;
    private bool isWallSliding;
    private bool canDoubleJump = true;
    private bool canMove;
    #endregion

    #region Movement Variables
    private float currentSpeed;
    private float dashTimeLeft;
    private float dashCooldownLeft;
    private float coyoteTimeLeft;
    private float jumpBufferTimeLeft;
    private float wallJumpCoolDownLeft;
    private float wallDirectionX;
    private Vector2 movementInput;
    private float originalGravityScale;
    private float postDashSpeed = 0f;
    private bool isInPostDashState = false;
    private float postDashTimer = 0f;
    private const float POST_DASH_DURATION = 0.2f;
    #endregion

    #region Physics Check Variables
    private ContactFilter2D groundFilter;
    private ContactFilter2D wallFilter;
    private RaycastHit2D[] groundHits = new RaycastHit2D[4];
    private RaycastHit2D[] wallHits = new RaycastHit2D[2];
    #endregion

    //Components cache
    private Transform playerTransform;

    // Implementaci�n de la interfaz IAnimationState
    public bool IsGrounded => isGrounded;
    public bool IsJumping => isJumping;
    public bool IsFalling => rb.linearVelocity.y < -0.1f;
    public bool IsWallSliding => isWallSliding;
    public bool IsDashing => isDashing;
    public float MovementSpeed => Mathf.Abs(rb.linearVelocity.x);
    public Vector2 MovementDirection => movementInput;

    private void Awake()
    {
        InitializeComponents();
        InitializeCollisionFilters();
        originalGravityScale = rb.gravityScale;
    }
    private void FixedUpdate()
    {
        UpdatePhysicsState();
        ApplyMovement();

        // Actualizar el dash si está activo
        if (isDashing)
        {
            HandleDashMovement();
        }
        if (dashCooldownLeft > 0)
        {
            dashCooldownLeft -= Time.fixedDeltaTime;
        }
    }
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

    private void Update()
    {
        UpdateAnimationState();
        Debug.Log(isDashing);
    }

    #region Initialization
    private void InitializeComponents()
    {
        playerTransform = transform;
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if(!animatorManager) animatorManager = GetComponent<AnimatorManager>();
        if(!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

    }

    private void InitializeCollisionFilters()
    {
        //Setup ground filter
        groundFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = data.groundLayer
        };

        //Set up Wall Filter
        wallFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = data.wallLayer
        };
    }
    #endregion

    #region Public Methods
    public void HandleMovement(Vector2 input)
    {
         movementInput = input;
         
    }

    // Variable para rastrear si ya procesamos este salto
    private bool jumpProcessed = false;

    public void HandleJumpInput(bool jumpPressed)
    {
        if (jumpPressed && !jumpProcessed)
        {
            jumpProcessed = true;
            jumpBufferTimeLeft = data.jumpBufferTime;
            TryJump();
        }
        else if (!jumpPressed)
        {
            jumpProcessed = false;
            if (rb.linearVelocity.y > 0 && isJumping)
            {
                //Early jump release for variable height
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * data.minJumpMultiplier);
                isJumping = false;
            }
        }
    }

    public void HandleDashInput(bool dashPressed)
    {
        if(dashPressed && dashCooldownLeft <= 0 && !isDashing)
        {
            Debug.Log("Entro en el if que llama a StartDash");
            StartDash();
        }
    }
    #endregion

    #region Movement Methods
    private void ApplyMovement()
    {
        if(isDashing)
        {
            HandleDashMovement();
            return;
        }
        // Manejo del estado post-dash
        if (isInPostDashState)
        {
            HandlePostDashMovement();
            return;
        }
        

        if (wallJumpCoolDownLeft > 0)
        {
            wallJumpCoolDownLeft -= Time.fixedDeltaTime;
            return;
        }
        // Movimiento horizontal
        float targetSpeed = movementInput.x * data.maxSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float acceleration;

        // Determinar qué aceleración usar
        if (Mathf.Abs(targetSpeed) < 0.01f)
        {
            // Desaceleración (frenado)
            acceleration = data.maxSpeed / data.decelerationTime;
        }
        else if (Mathf.Sign(targetSpeed) != Mathf.Sign(rb.linearVelocity.x))
        {
            // Cambio de dirección
            acceleration = data.maxSpeed / data.turnSpeed;
        }
        else
        {
            // Aceleración normal
            acceleration = data.maxSpeed / data.accelerationTime;
        }

        // Calcular la fuerza a aplicar
        float moveForce = Mathf.Abs(speedDiff) * acceleration * rb.mass;

        // Limitar la fuerza máxima
        moveForce = Mathf.Min(moveForce, data.maxSpeed * 10);

        // Aplicar la dirección
        moveForce *= Mathf.Sign(speedDiff);

        // Modificador de control aéreo
        if (!isGrounded)
        {
            moveForce *= data.airControlMultiplier;
        }

        // Aplicar la fuerza
        rb.AddForce(Vector2.right * moveForce);

        // Limitar velocidad horizontal
        float maxSpeedMultiplier = isDashing ? 1f : 0.95f;
        float clampedSpeedX = Mathf.Clamp(rb.linearVelocity.x, -data.maxSpeed * maxSpeedMultiplier, data.maxSpeed * maxSpeedMultiplier);
        rb.linearVelocity = new Vector2(clampedSpeedX, rb.linearVelocity.y);


        //Gestion de gravedad
        UpdateGravityScale();
    }

    private void UpdateGravityScale()
    {
        if (isDashing) return;
        // Si estamos cayendo
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = data.fallGravityMultiplier;
        }
        // Si estamos cerca del pico del salto
        else if (rb.linearVelocity.y > 0 && rb.linearVelocity.y < data.apexThreshold)
        {
            rb.gravityScale = data.apexGravityMultiplier;
        }
        // Si estamos subiendo
        else if (rb.linearVelocity.y > 0)
        {
            rb.gravityScale = data.ascendingGravityMultiplier;
        }
        // En otros casos
        else
        {
            rb.gravityScale = originalGravityScale;
        }

        // Limitar la velocidad de ca�da
        if (rb.linearVelocity.y < -data.maxFallSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -data.maxFallSpeed);
        }
    }
    private void TryJump()
    {
        if(isGrounded || coyoteTimeLeft > 0)
        {
            PerformJump(data.jumpForce);
            canDoubleJump = true;
        }
        else if(isWallSliding)
        {
            PerformWallJump();
        }
        else if(canDoubleJump)
        {
            PerformDoubleJump();
        }
    }

    private void PerformJump(float jumpForce)
    {
        isJumping = true;
        coyoteTimeLeft = 0;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private void PerformWallJump()
    {
        isWallSliding = false;
        isJumping = true;
        wallJumpCoolDownLeft = data.wallJumpControlCooldown;

        Vector2 wallJumpForce = new Vector2(-wallDirectionX* data.wallJumpForce, data.wallJumpForce);
        rb.linearVelocity = wallJumpForce;
    }

    private void PerformDoubleJump()
    {
        canDoubleJump = false;
        isJumping = true;
        PerformJump(data.jumpForce * 0.8f);
    }

    private void HandlePostDashMovement()
    {
        postDashTimer -= Time.fixedDeltaTime;

        if (postDashTimer <= 0)
        {
            isInPostDashState = false;
            return;
        }

        // Permitir cierto control durante el estado post-dash
        float targetSpeed = movementInput.x * data.maxSpeed;
        float currentSpeed = rb.linearVelocity.x;

        // Interpolar entre la velocidad post-dash y la velocidad objetivo
        float t = 1 - (postDashTimer / POST_DASH_DURATION);
        float finalSpeed = Mathf.Lerp(postDashSpeed, targetSpeed, t);

        rb.linearVelocity = new Vector2(finalSpeed, rb.linearVelocity.y);
    }
    private void StartDash()
    {
        if (isDashing) return;

        isDashing = true;
        dashTimeLeft = data.dashDuration;
        dashCooldownLeft = data.dashCooldown;

        // Determinar la dirección del dash
        float dashDirection;
        if (Mathf.Abs(movementInput.x) > 0.1f)
        {
            dashDirection = Mathf.Sign(movementInput.x);
        }
        else
        {
            dashDirection = spriteRenderer.flipX ? -1 : 1;
        }

        // Desactivar gravedad durante el dash
        rb.gravityScale = 0;

        // Aplicar velocidad del dash directamente
        rb.linearVelocity = new Vector2(dashDirection * data.dashSpeed, 0);

        // Limpiar fuerzas residuales
        rb.angularVelocity = 0;

        // Optional: Apply dash effects
        ApplyDashEffects();
    }

    private void HandleDashMovement()
    {
        // Si no estamos en dash, no deberíamos estar aquí
        if (!isDashing) return;

        // Actualizar el tiempo restante del dash
        dashTimeLeft -= Time.fixedDeltaTime;

        if (dashTimeLeft > 0)
        {
            // Mantener velocidad constante durante el dash
            float currentDashDirection = Mathf.Sign(rb.linearVelocity.x);
            rb.linearVelocity = new Vector2(currentDashDirection * data.dashSpeed, 0);
        }
        else
        {
            EndDash();
        }
    }
    private void EndDash()
    {
        if (!isDashing) return;

        isDashing = false;
        isInPostDashState = true;
        postDashTimer = POST_DASH_DURATION;

        // Restaurar gravedad
        rb.gravityScale = originalGravityScale;

        // Establecer una velocidad consistente al salir del dash
        float currentDirection = Mathf.Sign(rb.linearVelocity.x);
        postDashSpeed = currentDirection * data.maxSpeed * 0.7f;
        rb.linearVelocity = new Vector2(postDashSpeed, rb.linearVelocity.y);

        // Limpiar cualquier fuerza residual
        rb.angularVelocity = 0f;
    }

    private  IEnumerator EnableMovementAfterDelay(float delay)
    {
        // Deshabilitamos temporalmente el input de movimiento
        canMove = false;
        yield return new WaitForSeconds(delay);
        canMove = true;
    }
    #endregion

    #region Checks
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
        Vector2 boxSize = new Vector2(data.groundCheckWidth,data.groundCheckDistance);

        //Perform the ground Check
        int hitCount = Physics2D.BoxCast(
            boxCenter,
            boxSize,
            0f,
            Vector2.down,
            groundFilter,
            groundHits,
            data.groundCheckDistance
            );

        //Update Grounded State
        bool wasGrounded = isGrounded;
        isGrounded = hitCount > 0;

        //Start CoyoteTime when leaving ground
        if(wasGrounded && !isGrounded)
        {
            coyoteTimeLeft = data.coyoteTime;
        }

        //Reset doubleJump when touching ground
        if(isGrounded)
        {
            canDoubleJump = true;
            isJumping = false;
            ApplyLandingSquash();
        }

        //Debug visualization
        if(Debug.isDebugBuild)
        {
            Color debugColor = isGrounded ? Color.green : Color.red;
            DrawDebugCube(boxCenter, boxSize, debugColor);
        }
    }

    private void UpdateWallState()
    {
        //Check both left and right walls
        Vector2 boxCenter = (Vector2)transform.position +
            new Vector2(0, data.wallCheckVerticalOffset);
        Vector2 boxSize = new Vector2(data.wallCheckDistance,data.wallCheckHeight);

        //Check right wall
        int rightHitCount = Physics2D.BoxCast(
            boxCenter,
            boxSize,
            0f,
            Vector2.right,
            wallFilter,
            wallHits,
            data.wallCheckDistance
        );
        // Check left wall
        int leftHitCount = Physics2D.BoxCast(
            boxCenter,
            boxSize,
            0f,
            Vector2.left,
            wallFilter,
            wallHits,
            data.wallCheckDistance
        );

        //Update WallSliding State

        bool wasWallSliding = isWallSliding;
        isWallSliding = false;
        wallDirectionX = 0;

        //Only allow wall sliding if we are moving into the wall and falling
        if(!isGrounded && rb.linearVelocity.y < 0)
        {
            if(rightHitCount > 0 && movementInput.x> 0)
            {
                isWallSliding = true;
                wallDirectionX = 1;
            }
            else if(leftHitCount > 0 && movementInput.x < 0)
            {
                isWallSliding = true;
                wallDirectionX = -1;
            }
        }

        //Apply wall slide velocity
        if(wasWallSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -data.wallSlideSpeed);
        }

        // Debug visualization
        if (Debug.isDebugBuild)
        {
            Color rightDebugColor = rightHitCount > 0 ? Color.green : Color.red;
            Color leftDebugColor = leftHitCount > 0 ? Color.green : Color.red;

            DrawDebugCube(
                boxCenter + Vector2.right * data.wallCheckDistance,
                boxSize,
                rightDebugColor
            );
            DrawDebugCube(
                boxCenter + Vector2.left * data.wallCheckDistance,
                boxSize,
                leftDebugColor
            );
        }
    }
    private void HandleCoyoteTime()
    {
        if(!isGrounded && coyoteTimeLeft > 0)
        {
            coyoteTimeLeft -= Time.fixedDeltaTime;
        }
    }

    private void HandleJumpBuffer()
    {
        if(jumpBufferTimeLeft > 0)
        {
            jumpBufferTimeLeft -= Time.fixedDeltaTime;
            if((isGrounded || isWallSliding) && jumpBufferTimeLeft > 0)
            {
                jumpBufferTimeLeft = 0;
                TryJump();
            }
        }
    }

    #endregion

    #region Visual Effects
    private void ApplyJumpSquash()
    {
        // Apply jump stretch effect
        Vector3 newScale = playerTransform.localScale;
        newScale.y *= (1 + data.jumpStretchAmount);
        newScale.x *= (1 - data.jumpStretchAmount * 0.5f);
        playerTransform.localScale = newScale;

        // Reset scale after duration
        Invoke(nameof(ResetScale), data.squashStretchDuration);
    }

    private void ApplyLandingSquash()
    {
        // Apply landing squash effect
        Vector3 newScale = playerTransform.localScale;
        newScale.y *= (1 - data.landSquashAmount);
        newScale.x *= (1 + data.landSquashAmount * 0.5f);
        playerTransform.localScale = newScale;

        // Reset scale after duration
        Invoke(nameof(ResetScale), data.squashStretchDuration);
    }

    private void ApplyDashEffects()
    {
        // Implementation for dash effects (particles, trails, etc.)
    }

    private void ResetScale()
    {
        playerTransform.localScale = Vector3.one;
    }
    #endregion

    #region Debug Helpers
    private static void DrawDebugCube(Vector2 position, Vector2 size, Color color)
    {
        Vector2 halfSize = size * 0.5f;

        Vector2 topLeft = position + new Vector2(-halfSize.x, halfSize.y);
        Vector2 topRight = position + halfSize;
        Vector2 bottomRight = position + new Vector2(halfSize.x, -halfSize.y);
        Vector2 bottomLeft = position + new Vector2(-halfSize.x, -halfSize.y);

        Debug.DrawLine(topLeft, topRight, color);
        Debug.DrawLine(topRight, bottomRight, color);
        Debug.DrawLine(bottomRight, bottomLeft, color);
        Debug.DrawLine(bottomLeft, topLeft, color);
    }
    #endregion


}
