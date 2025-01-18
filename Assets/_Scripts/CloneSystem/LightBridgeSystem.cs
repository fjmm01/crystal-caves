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

        // Si no hay clones activos o solo hay uno, desactivar el LineRenderer
        if (activeClones == null || activeClones.Count < 2)
        {
            if (bridgeRenderer != null)
            {
                bridgeRenderer.positionCount = 0;
            }
            return;
        }

        // Calcular el número total de posibles conexiones
        int totalPossibleConnections = (activeClones.Count * (activeClones.Count - 1)) / 2;
        if (bridgeRenderer != null)
        {
            bridgeRenderer.positionCount = totalPossibleConnections * 2; // Cada conexión necesita 2 puntos
        }

        int currentPositionIndex = 0;

        // Verificar todas las posibles conexiones entre clones
        for (int i = 0; i < activeClones.Count; i++)
        {
            for (int j = i + 1; j < activeClones.Count; j++)
            {
                float distance = Vector2.Distance(activeClones[i].Position, activeClones[j].Position);
                if (distance <= maxDistance)
                {
                    activeBridges.Add((activeClones[i], activeClones[j]));
                    CreateBridge(activeClones[i].Position, activeClones[j].Position, ref currentPositionIndex);
                }
            }
        }

        // Si el número real de conexiones es menor que el total posible,
        // ajustar el positionCount al número real
        if (bridgeRenderer != null && currentPositionIndex < bridgeRenderer.positionCount)
        {
            bridgeRenderer.positionCount = currentPositionIndex;
        }
    }

    private void CreateBridge(Vector2 start, Vector2 end, ref int positionIndex)
    {
        if (bridgeRenderer == null) return;

        // Configurar el puente visual usando el índice actual
        bridgeRenderer.SetPosition(positionIndex, start);
        bridgeRenderer.SetPosition(positionIndex + 1, end);
        positionIndex += 2; // Incrementar el índice para la siguiente conexión
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
