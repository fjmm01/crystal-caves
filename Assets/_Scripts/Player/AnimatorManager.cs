using UnityEngine;

public class AnimatorManager : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("Animation Parameters")]
    [SerializeField] private string groundedParam = "isGrounded";
    [SerializeField] private string jumpingParam = "isJumping";
    [SerializeField] private string fallingParam = "isFalling";
    [SerializeField] private string wallSlidingParam = "isWallSliding";
    [SerializeField] private string dashingParam = "isDashing";
    [SerializeField] private string speedParam = "speed";
    [SerializeField] private string directionXParam = "directionX";
    [SerializeField] private string directionYParam = "directionY";
    [Header("Combat Animation Parameters")]
    [SerializeField] private string attackingParam = "isAttacking";
    [SerializeField] private string attackTypeParam = "attackType";

    public void UpdateAnimationState(IAnimationState state)
    {
        // Update boolean parameters
        animator.SetBool(groundedParam, state.IsGrounded);
        animator.SetBool(jumpingParam, state.IsJumping);
        animator.SetBool(fallingParam, state.IsFalling);
        animator.SetBool(wallSlidingParam, state.IsWallSliding);
        animator.SetBool(dashingParam, state.IsDashing);

        // Update float parameters
        animator.SetFloat(speedParam, state.MovementSpeed);
        animator.SetFloat(directionXParam, state.MovementDirection.x);
        animator.SetFloat(directionYParam, state.MovementDirection.y);
        
    }
    public void UpdateCombatState(ICombatState state)
    {
        animator.SetBool(attackingParam, state.IsAttacking);

        // Convertir el tipo de ataque a un valor numérico para el animator
        int attackTypeValue = state.CurrentAttackType switch
        {
            "Horizontal" => 1,
            "Up" => 2,
            "Down" => 3,
            _ => 0
        };

        animator.SetInteger(attackTypeParam, attackTypeValue);
    }

    // Método para reproducir animaciones específicas directamente
    public void PlayAnimation(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }

    // Método para reproducir animaciones con transición personalizada
    public void PlayAnimation(string triggerName, float transitionTime)
    {
        animator.CrossFade(triggerName, transitionTime);
    }

    // Método para interrumpir una animación específica
    public void StopAnimation(string triggerName)
    {
        animator.ResetTrigger(triggerName);
    }
}
