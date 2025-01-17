using System;
using UnityEngine;

public class HealthSystem : BaseStatSystem,IDamageable
{
    public event Action<float> OnHealthChanged;
    public event Action OnDeath;

    private bool isInvulnerable;
    private float invulnerabilityTimer;

    public HealthSystem(float maxHealth): base(maxHealth) { }

    public void TakeDamage(DamageData damageData)
    {
        if(isInvulnerable)  return;

        Modify(-damageData.damage);

        if(IsEmpty)
        {
            OnDeath?.Invoke();
        }
        else
        {
            StartInvulnerability(0.5f); //Tiempo configurable
        }
    }

    private void StartInvulnerability(float duration)
    {
        isInvulnerable = true;
        invulnerabilityTimer = duration;
    }

    public void UpdateInvulnerability(float deltaTime)
    {
        if (!isInvulnerable) return;

        invulnerabilityTimer -= deltaTime;
        if(invulnerabilityTimer <= 0)
        {
            isInvulnerable = false;
        }
    }

    protected override void OnValueChanged()
    {
        OnHealthChanged?.Invoke(currentValue);
    }
}
