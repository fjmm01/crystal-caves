using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

public class CloneManager : MonoBehaviour
{
    [SerializeField] private CloneData cloneData;
    [SerializeField] private GameObject clonePrefab;
    [SerializeField] private PlayerStatsManager statsManager;
    [SerializeField] private LightBridgeSystem bridgeSystem;

    [SerializeField] private List<CrystalClone> activeClones = new List<CrystalClone>();
    [SerializeField]private ObjectPool<CrystalClone> clonePool;

    public CloneData CloneData => cloneData;
    public int ActiveCloneCount => activeClones.Count;

    private void Awake()
    {
        InitializeClonePool();
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

    public bool CanCreateClone()
    {
        return activeClones.Count < cloneData.maxClones && statsManager.CanCreateClone() && !IsPositionTooCloseToExistingClones(transform.position);
    }

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

        clone.transform.position = position;
        clone.Initialize(this, cloneData.duration);

        activeClones.Add(clone);

        if (bridgeSystem != null)
        {
            UpdateLightBridges();
        }

        return clone;
    }

    public void RemoveClone(CrystalClone clone)
    {
        if(activeClones.Remove(clone))
        {
            clonePool.Release(clone);
            UpdateLightBridges();
        }
    }

    private bool IsPositionTooCloseToExistingClones(Vector2 position)
    {
        return activeClones.Any(clone => Vector2.Distance(clone.Position,position) < cloneData.minDistanceBetweenClones);
    }

    private void UpdateLightBridges()
    {
        bridgeSystem.UpdateBridges(activeClones, cloneData.maxBridgeDistance);
    }

    private void Update()
    {
        //Actualizar cada clon activo
        foreach(var clone in activeClones.ToList())
        {
            clone.UpdateClone();
        }
    }
}
