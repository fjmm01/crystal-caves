using System;
using UnityEngine;

public class EnergySystem : BaseStatSystem,IRegenerativeSystem
{
    public event Action<float> OnEnergyChanged;

    private readonly float regenerationRate;
    private readonly float regenerationDelay;
    private float timeSinceLastUse;

    public bool CanRegenerate => timeSinceLastUse >= regenerationDelay;

    public EnergySystem(float maxEnergy, float regenerationRate, float regenerationDelay) : base (maxEnergy)
    {
        this.regenerationRate = regenerationRate;
        this.regenerationDelay = regenerationDelay;
        timeSinceLastUse = regenerationDelay;
    }

    public override void Modify(float amount)
    {
        base.Modify(amount);
        if(amount < 0 )
        {
            timeSinceLastUse = 0;
        }
    }

    public void UpdateRegeneration(float deltaTime)
    {
        timeSinceLastUse += deltaTime;

        if(CanRegenerate && !IsFull)
        {
            Modify(regenerationRate * deltaTime);
        }
    }

    protected override void OnValueChanged()
    {
        OnEnergyChanged?.Invoke(currentValue);
    }
}
