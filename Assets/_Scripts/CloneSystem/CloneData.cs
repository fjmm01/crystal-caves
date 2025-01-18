using UnityEngine;

/// <summary>
/// ScriptableObject que contiene toda la configuración de los clones.
/// Permite ajustar los valores desde el editor de Unity y mantiene la configuración
/// separada de la lógica (Principio de Responsabilidad Única).
/// </summary>
[CreateAssetMenu(fileName = "CloneData", menuName = "Crystal Caves/Player/CloneData")]
public class CloneDataScriptableObject : ScriptableObject
{
    [Header("Clone Properties")]
    [Tooltip("Duration in seconds that each clone will last")]
    public float duration = 30f;

    [Tooltip("Energy cost to create a clone")]
    public float energyCost = 20f;

    [Tooltip("Maximum number of clones that can exist simultaneously")]
    public int maxClones = 3;

    [Tooltip("Minimum distance required between clones")]
    public float minDistanceBetweenClones = 1f;

    [Header("Light Bridge Properties")]
    [Tooltip("Maximum distance for creating light bridges between clones")]
    public float maxBridgeDistance = 8f;

    [Header("Visual Effects")]
    [Tooltip("Color of the clone")]
    public Color cloneColor = new Color(0.5f, 0.6f, 1f, 0.8f);

    [Tooltip("Color of the light bridge")]
    public Color bridgeColor = new Color(0.5f, 0.6f, 1f, 0.6f);

    [Header("Gameplay Settings")]
    [Tooltip("Whether momentum is preserved when teleporting")]
    public bool preserveMomentum = true;

    [Tooltip("Whether clones can be created in mid-air")]
    public bool allowMidAirCloning = true;

    [Tooltip("Time before a clone can be teleported to after creation")]
    public float teleportDelay = 0.1f;
}

