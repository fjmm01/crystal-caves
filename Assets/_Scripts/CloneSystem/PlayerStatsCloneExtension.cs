using UnityEngine;

/// <summary>
/// Extensión del PlayerStatsManager para manejar la energía relacionada con los clones.
/// Siguiendo el principio de Abierto/Cerrado (OCP), extendemos la funcionalidad
/// sin modificar la clase original.
/// </summary>
public static class PlayerStatsCloneExtension
{
    /// <summary>
    /// Verifica si hay suficiente energía para crear un clon
    /// </summary>
    public static bool CanCreateClone(this EnergySystem statsManager)
    {
        if (statsManager == null) return false;

        // Verificar que hay suficiente energía para crear un clon
        return statsManager.CurrentValue >= GetCloneEnergyCost(statsManager);
    }

    /// <summary>
    /// Consume la energía necesaria para crear un clon
    /// </summary>
    public static void ConsumeCloneEnergy(this EnergySystem statsManager)
    {
        if (statsManager == null) return;

        float energyCost = GetCloneEnergyCost(statsManager);
        statsManager.Modify(-energyCost);
    }

    /// <summary>
    /// Obtiene el costo de energía para crear un clon
    /// </summary>
    private static float GetCloneEnergyCost(EnergySystem statsManager)
    {
        // Este valor podría venir de los datos del jugador o ser configurable
        return 20f; // Valor base, podría ser modificado por habilidades o mejoras
    }

    /// <summary>
    /// Regenera una parte de la energía al destruir un clon
    /// </summary>
    public static void RegenerateCloneEnergy(this EnergySystem statsManager, float percentage = 0.5f)
    {
        if (statsManager == null) return;

        float energyToRegenerate = GetCloneEnergyCost(statsManager) * percentage;
        statsManager.Modify(energyToRegenerate);
    }
}
