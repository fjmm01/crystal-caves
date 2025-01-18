using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Gestor principal del sistema de clones.
/// Coordina la creación, destrucción y gestión de todos los clones en el juego.
/// </summary>
public class CloneManager : MonoBehaviour
{
    [SerializeField] private CloneDataScriptableObject cloneData;
    [SerializeField] private GameObject clonePrefab;
    [SerializeField] private PlayerStatsManager statsManager;
    [SerializeField] private LightBridgeSystem bridgeSystem;

    private List<CrystalClone> activeClones = new List<CrystalClone>();
    private ObjectPool<CrystalClone> clonePool;
    private Transform clonesContainer;

    // Propiedades públicas para acceso externo
    public IReadOnlyList<CrystalClone> ActiveClones => activeClones.AsReadOnly();
    public CloneDataScriptableObject CloneData => cloneData;
    public int ActiveCloneCount => activeClones.Count;

    private void Awake()
    {
        CreateClonesContainer();
        InitializeClonePool();
    }

    private void CreateClonesContainer()
    {
        GameObject container = GameObject.Find("ClonesContainer");
        if (container == null)
        {
            container = new GameObject("ClonesContainer");
        }
        clonesContainer = container.transform;
    }

    private void InitializeClonePool()
    {
        if (clonePrefab == null)
        {
            Debug.LogError("Clone Prefab is not assigned in CloneManager!");
            return;
        }

        if (cloneData == null)
        {
            Debug.LogError("CloneData is not assigned in CloneManager!");
            return;
        }

        clonePool = new ObjectPool<CrystalClone>(
            createFunc: () => {
                var clone = Instantiate(clonePrefab).GetComponent<CrystalClone>();
                if (clone == null)
                {
                    Debug.LogError("CrystalClone component not found on prefab!");
                    return null;
                }
                clone.gameObject.SetActive(false);
                return clone;
            },
            actionOnGet: (clone) => {
                if (clone != null && clone.gameObject != null)
                    clone.gameObject.SetActive(true);
            },
            actionOnRelease: (clone) => {
                if (clone != null && clone.gameObject != null)
                    clone.gameObject.SetActive(false);
            },
            actionOnDestroy: (clone) => {
                if (clone != null && clone.gameObject != null)
                    Destroy(clone.gameObject);
            },
            defaultCapacity: cloneData.maxClones,
            maxSize: cloneData.maxClones * 2
        );
    }

    /// <summary>
    /// Verifica si es posible crear un nuevo clon
    /// </summary>
    public bool CanCreateClone()
    {
        return activeClones.Count < cloneData.maxClones &&
               statsManager != null &&
               statsManager.CanCreateClone() &&
               !IsPositionTooCloseToExistingClones(transform.position);
    }

    /// <summary>
    /// Crea un nuevo clon en la posición especificada
    /// </summary>
    public CrystalClone CreateClone(Vector2 position)
    {
        if (!CanCreateClone()) return null;
        if (clonePool == null)
        {
            Debug.LogError("Clone pool is not initialized!");
            return null;
        }

        if (statsManager == null)
        {
            Debug.LogError("StatsManager reference is missing in CloneManager!");
            return null;
        }

        statsManager.ConsumeCloneEnergy();

        var clone = clonePool.Get();
        if (clone == null)
        {
            Debug.LogError("Failed to get clone from pool!");
            return null;
        }

        clone.transform.SetParent(clonesContainer);
        clone.transform.position = position;
        clone.Initialize(this, cloneData.duration);

        activeClones.Add(clone);

        if (bridgeSystem != null)
        {
            UpdateLightBridges();
        }

        return clone;
    }

    /// <summary>
    /// Elimina un clon del sistema y lo devuelve al pool
    /// </summary>
    public void RemoveClone(CrystalClone clone)
    {
        if (activeClones.Remove(clone))
        {
            clonePool.Release(clone);
            UpdateLightBridges();
        }
    }

    /// <summary>
    /// Verifica si una posición está demasiado cerca de los clones existentes
    /// </summary>
    private bool IsPositionTooCloseToExistingClones(Vector2 position)
    {
        return activeClones.Any(clone =>
            Vector2.Distance(clone.Position, position) < cloneData.minDistanceBetweenClones);
    }

    /// <summary>
    /// Actualiza los puentes de luz entre todos los clones activos
    /// </summary>
    private void UpdateLightBridges()
    {
        if (bridgeSystem != null)
        {
            bridgeSystem.UpdateBridges(activeClones, cloneData.maxBridgeDistance);
        }
    }

    private void Update()
    {
        // Actualizar cada clon activo
        foreach (var clone in activeClones.ToList())
        {
            clone.UpdateClone();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Dibujar rango de clones activos
        Gizmos.color = Color.blue;
        foreach (var clone in activeClones)
        {
            Gizmos.DrawWireSphere(clone.Position, 0.5f);
        }

        // Dibujar distancia máxima de puentes
        Gizmos.color = new Color(0, 1, 1, 0.3f); // Cyan transparente
        foreach (var clone in activeClones)
        {
            Gizmos.DrawWireSphere(clone.Position, cloneData.maxBridgeDistance);
        }
    }
#endif
}
