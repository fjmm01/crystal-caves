using UnityEngine;

public class DefaultAttackValidator : IAttackValidator
{
    public bool CanPerformAttack(AttackDirection attackDirection, IMovementState movementState)
    {
        // No permitir ataque hacia abajo si estamos en el suelo
        if (attackDirection == AttackDirection.Down && movementState.IsGrounded)
        {
            return false;
        }

        // No permitir ataques durante el dash
        if (movementState.IsDashing)
        {
            return false;
        }

        // Aqu� podr�amos a�adir m�s reglas en el futuro, como:
        // - Ataques especiales en las paredes (movementState.IsWallSliding)
        // - Combos espec�ficos seg�n el estado
        // - Restricciones durante el salto (movementState.IsJumping)
        // - Ataques a�reos especiales (movementState.IsFalling)

        return true;
    }
}
