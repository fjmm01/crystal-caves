using UnityEngine;

public interface IAttackValidator
{
    bool CanPerformAttack(AttackDirection attackDirection, IMovementState movementState);
}
