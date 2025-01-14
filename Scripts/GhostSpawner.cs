using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GhostSpawner : SingletonMonoBehaviour<GhostSpawner>
{
    /*[SerializeField] private Ghost _ghostPrefab;
    [SerializeField] private int _initialPoolSize = 8;
    [SerializeField] private int _maxPoolSize = 16;
    [SerializeField] private Transform _parentTransform;

    private readonly Dictionary<string, IObjectPool<Ghost>> _ghostPools = new();
    private readonly Vector3 _offScreenPosition = new(999, 999, 0);

    private IObjectPool<Ghost> GetOrCreatePool(Ghost prefab)
    {
        if (_ghostPools.TryGetValue(prefab.name, out var pool))
        {
            return pool;
        }

        pool = new ObjectPool<Ghost>(
            createFunc: () => InstantiateGhost(prefab),
            actionOnGet: OnTakeGhostFromPool,
            actionOnRelease: OnReturnGhostToPool,
            actionOnDestroy: DestroyGhost,
            collectionCheck: true,
            defaultCapacity: _initialPoolSize,
            maxSize: _maxPoolSize
        );

        _ghostPools.Add(prefab.name, pool);
        return pool;
    }

   *//* public Ghost SpawnGhost(GhostParent ghostParent, Ghost.GhostPosition ghostPosition)
    {
        //var pool = GetOrCreatePool(ghostParent.GhostPrefab);
        //var ghost = pool.Get();
        //ghost.Init(ghostParent, ghostPosition);
        //return ghost;
    }*//*

    public void ReleaseGhost(Ghost ghost)
    {
        if (!ghost) return;

        var pool = GetOrCreatePool(ghost);
        pool.Release(ghost);
    }

    private Ghost InstantiateGhost(Ghost prefab)
    {
        var ghost = Instantiate(prefab, _parentTransform);
        ghost.gameObject.SetActive(false);
        ghost.transform.position = _offScreenPosition;
        return ghost;
    }

    private void OnTakeGhostFromPool(Ghost ghost)
    {
        ghost.gameObject.SetActive(true);
        ghost.transform.position = _offScreenPosition;
    }

    private void OnReturnGhostToPool(Ghost ghost)
    {
        ghost.gameObject.SetActive(false);
        ghost.transform.position = _offScreenPosition;
    }

    private void DestroyGhost(Ghost ghost)
    {
        if (ghost != null)
        {
            Destroy(ghost.gameObject);
        }
    }*/
}
