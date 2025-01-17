using UnityEngine;

public interface IRegenerativeSystem : IStatSystem
{
    bool CanRegenerate { get; }
    void UpdateRegeneration(float deltaTime);
}
