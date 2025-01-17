using UnityEngine;

public interface ICombatState
{
    bool IsAttacking { get; }
    string CurrentAttackType { get; }
}
