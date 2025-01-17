using UnityEngine;

public class PlayerCombatState : ICombatState
{
    public bool IsAttacking { get; set; }
    public string CurrentAttackType { get; set; }
}
