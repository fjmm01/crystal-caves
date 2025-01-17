using UnityEngine;

public interface IStatSystem
{
    float CurrentValue { get; }
    float MaxValue { get; }
    bool IsEmpty { get; }
    bool IsFull {  get; }
    void Modify(float amount);
    void SetToMax();
}
