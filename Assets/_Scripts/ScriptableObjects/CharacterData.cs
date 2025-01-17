using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Crystal Caves/Player/Data")]
public class CharacterData : ScriptableObject
{
    [Header("Ground Movement")]
    [Tooltip("Maximum horizontal speed")]
    public float maxSpeed = 10f;                    // Aumentado para movimiento más rápido
    [Tooltip("Time to reach max speed")]
    public float accelerationTime = 0.1f;           // Reducido para aceleración más rápida
    [Tooltip("Time to stop when releasing input")]
    public float decelerationTime = 0.05f;          // Reducido para frenado más rápido
    [Tooltip("Additional acceleration when turning around")]
    public float turnSpeed = 2f;                    // Aumentado para giros más rápidos

    [Header("Air Movement")]
    [Tooltip("Horizontal air control multiplier")]
    public float airControlMultiplier = 0.75f;      // Reducido para menor control en el aire
    [Tooltip("Maximum fall speed")]
    public float maxFallSpeed = 25f;                // Aumentado para caídas más rápidas
    [Tooltip("Gravity multiplier when falling")]
    public float fallGravityMultiplier = 2.5f;      // Aumentado significativamente
    [Tooltip("Gravity multiplier when ascending")]
    public float ascendingGravityMultiplier = 0.9f; // Ajustado para mantener altura de salto

    [Header("Jump Settings")]
    [Tooltip("Initial jump force")]
    public float jumpForce = 16f;                   // Aumentado para saltos más altos
    [Tooltip("Minimum jump height multiplier when button is released early")]
    public float minJumpMultiplier = 0.4f;          // Reducido para saltos cortos más cortos
    [Tooltip("Maximum time to buffer a jump input")]
    public float jumpBufferTime = 0.2f;
    [Tooltip("Time window to jump after leaving a platform")]
    public float coyoteTime = 0.15f;
    [Tooltip("Additional gravity when reaching jump apex")]
    public float apexGravityMultiplier = 2f;        // Nueva variable
    [Tooltip("Velocity threshold for detecting jump apex")]
    public float apexThreshold = 2f;                // Nueva variable

    [Header("Wall Movement")]
    [Tooltip("Horizontal force when wall jumping")]
    public float wallJumpForce = 10f;
    [Tooltip("Vertical force when wall jumping")]
    public float wallJumpUpForce = 15f;
    [Tooltip("Time that horizontal input is limited after wall jump")]
    public float wallJumpControlCooldown = 0.2f;
    [Tooltip("Speed while sliding down walls")]
    public float wallSlideSpeed = 3f;

    [Header("Dash Settings")]
    [Tooltip("Dash speed")]
    public float dashSpeed = 20f;
    [Tooltip("Duration of the dash")]
    public float dashDuration = 0.15f;
    [Tooltip("Time before being able to dash again")]
    public float dashCooldown = 2f;
    [Tooltip("Whether the dash should preserve vertical momentum")]
    public bool preserveDashMomentum = true;
    [Tooltip("Multiplier for vertical momentum after dash")]
    public float dashEndVerticalMomentum = 0.5f;

    [Header("Corner Correction")]
    [Tooltip("Distance to check for corner correction")]
    public float cornerCorrectionDistance = 0.3f;
    [Tooltip("How far to push the player when corner correcting")]
    public float cornerCorrectionHeight = 0.3f;

    [Header("Environmental Interaction")]
    [Tooltip("Multiplier for movement on slippery surfaces")]
    public float slipperyMultiplier = 0.2f;
    [Tooltip("Additional gravity when on sticky surfaces")]
    public float stickyMultiplier = 1.5f;

    [Header("Visual Feedback")]
    [Tooltip("Amount of squash when landing")]
    public float landSquashAmount = 0.2f;
    [Tooltip("Amount of stretch when jumping")]
    public float jumpStretchAmount = 0.2f;
    [Tooltip("Duration of squash and stretch effects")]
    public float squashStretchDuration = 0.1f;

    [Header("Advanced Movement")]
    [Tooltip("Whether to conserve momentum when changing directions")]
    public bool conserveMomentum = true;
    [Tooltip("Maximum speed that can be achieved through momentum")]
    public float maxMomentumSpeed = 1.5f;
    [Tooltip("How quickly momentum decays")]
    public float momentumDecayRate = 0.8f;
    

    [Header("Ground Detection")]
    [Tooltip("Layer mask for ground detection")]
    public LayerMask groundLayer;
    [Tooltip("Distance to check for ground")]
    public float groundCheckDistance = 0.2f;
    [Tooltip("Width of the ground check box")]
    public float groundCheckWidth = 0.8f;
    [Tooltip("Offset from the center for ground detection")]
    public Vector2 groundCheckOffset = new Vector2(0, -0.5f);

    [Header("Wall Detection")]
    [Tooltip("Layer mask for wall detection")]
    public LayerMask wallLayer;
    [Tooltip("Distance to check for walls")]
    public float wallCheckDistance = 0.3f;
    [Tooltip("Height of the wall check box")]
    public float wallCheckHeight = 1f;
    [Tooltip("Vertical offset for wall detection from center")]
    public float wallCheckVerticalOffset = 0f;

    [Header("Attack Properties")]
    public float attackDuration = 0.25f;
    public float attackCooldown = 0.2f;
    public int attackDamage = 1;
    public float attackKnockbackForce = 8f;
    public float knockbackDuration = 0.2f;

    [Header("Hit Detection")]
    public Vector2 horizontalHitboxSize = new Vector2(1.5f, 1f);
    public Vector2 verticalHitboxSize = new Vector2(1f, 1.5f);
    public float hitboxOffset = 1f;

    [Header("Visual Feedback")]
    public float hitStopDuration = 0.1f;
    public float screenShakeIntensity = 0.1f;
    public float screenShakeDuration = 0.1f;
}
