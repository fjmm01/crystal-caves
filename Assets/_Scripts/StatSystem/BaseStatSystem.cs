using UnityEngine;

public abstract class BaseStatSystem : IStatSystem
{
    protected float currentValue;
    protected float maxValue;

    public float CurrentValue => currentValue;
    public float MaxValue => maxValue;
    public bool IsEmpty => currentValue <= 0;
    public bool IsFull => currentValue >= maxValue;

    protected BaseStatSystem(float maxValue)
    {
        this.maxValue = maxValue;
        currentValue = maxValue;
    }

    public virtual void Modify(float amount)
    {
        currentValue = Mathf.Clamp(currentValue + amount, 0, maxValue);
        OnValueChanged();
    }

    public virtual void SetToMax()
    {
        currentValue = maxValue;
        OnValueChanged();
    }

    protected virtual void OnValueChanged()
    {
        //Para sobreescribir en las clases derivadas
    }
}
