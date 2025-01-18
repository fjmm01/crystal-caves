using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Sistema que maneja la creación y gestión de puentes de luz entre clones.
/// Sigue el principio de Responsabilidad Única manejando solo la lógica de puentes.
/// </summary>
public class LightBridgeSystem : MonoBehaviour
{
    private LineRenderer bridgeRenderer;
    private List<(CrystalClone clone1, CrystalClone clone2)> activeBridges = new List<(CrystalClone, CrystalClone)>();

    private void Awake()
    {
        bridgeRenderer = GetComponent<LineRenderer>();
        ConfigureLineRenderer();
    }

    private void ConfigureLineRenderer()
    {
        if (bridgeRenderer != null)
        {
            bridgeRenderer.startWidth = 0.2f;
            bridgeRenderer.endWidth = 0.2f;
            bridgeRenderer.positionCount = 0;
            bridgeRenderer.useWorldSpace = true;
        }
    }

    /// <summary>
    /// Actualiza los puentes de luz entre todos los clones activos
    /// </summary>
    public void UpdateBridges(List<CrystalClone> activeClones, float maxDistance)
    {
        activeBridges.Clear();

        // Verificar todas las posibles conexiones entre clones
        for (int i = 0; i < activeClones.Count; i++)
        {
            for (int j = i + 1; j < activeClones.Count; j++)
            {
                float distance = Vector2.Distance(activeClones[i].Position, activeClones[j].Position);
                if (distance <= maxDistance)
                {
                    activeBridges.Add((activeClones[i], activeClones[j]));
                    CreateBridge(activeClones[i].Position, activeClones[j].Position);
                }
            }
        }
    }

    private void CreateBridge(Vector2 start, Vector2 end)
    {
        if (bridgeRenderer == null) return;

        // Configurar el puente visual
        bridgeRenderer.positionCount = 2;
        bridgeRenderer.SetPosition(0, start);
        bridgeRenderer.SetPosition(1, end);
    }

    /// <summary>
    /// Verifica si hay un puente de luz en una posición específica
    /// </summary>
    public bool IsBridgePresent(Vector2 position)
    {
        foreach (var bridge in activeBridges)
        {
            Vector2 bridgeStart = bridge.clone1.Position;
            Vector2 bridgeEnd = bridge.clone2.Position;

            // Verificar si el punto está en la línea del puente
            float distance = HandleUtility.DistancePointLine(position, bridgeStart, bridgeEnd);
            if (distance < 0.5f) // Tolerancia ajustable
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Limpia todos los puentes activos
    /// </summary>
    public void ClearBridges()
    {
        activeBridges.Clear();
        if (bridgeRenderer != null)
        {
            bridgeRenderer.positionCount = 0;
        }
    }
}       
