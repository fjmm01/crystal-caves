using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LightBridgeSystem : MonoBehaviour
{
    private LineRenderer bridgeRenderer;
    private List<(CrystalClone clone1,CrystalClone clone2)> activeBridges = new List<(CrystalClone,CrystalClone)>();

    private void Awake()
    {
        bridgeRenderer = GetComponent<LineRenderer>();
    }

    public void UpdateBridges(List<CrystalClone> activeClones, float maxDistance)
    {
        activeBridges.Clear();

        //Verificar todas las posibles conexiones entre clones
        for(int i = 0; i < activeClones.Count; i++)
        {
            for(int j = i; j < activeClones.Count; j++)
            {
                float distance = Vector2.Distance(activeClones[i].Position, activeClones[j].Position);
                if(distance <= maxDistance)
                {
                    activeBridges.Add((activeClones[i],activeClones[j]));
                    CreateBridge(activeClones[i].Position,activeClones[j].Position);
                }
            }
        }
    }
    
    private void CreateBridge(Vector2 start,Vector2 end)
    {
        //Crear el puente visual usando LineRenderer
        bridgeRenderer.positionCount = 2;
        bridgeRenderer.SetPosition(0,start);
        bridgeRenderer.SetPosition(1,end);
    }

    public bool IsBridgePresent(Vector2 position)
    {
        //Verificar si hay puente en la posicion dada
        foreach(var bridge in activeBridges)
        {
            Vector2 bridgeStart = bridge.clone1.Position;
            Vector2 bridgeEnd = bridge.clone2.Position;

            //Verificar si el punto está en linea del puente
            float distance = HandleUtility.DistancePointLine(position, bridgeStart, bridgeEnd);
            if(distance < 0.5f)
            {
                return true;
            }
        }
        return false;
    }

}
