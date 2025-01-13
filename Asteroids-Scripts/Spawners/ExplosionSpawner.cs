using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.InputSystem;
#endif
using UnityEngine.Pool;

public class ExplosionSpawner : SingletonMonoBehaviour<ExplosionSpawner>
{
    [SerializeField] ExplosionEffect _explosionPrefab;

    IObjectPool<ExplosionEffect> _explosionPool;

    public void SpawnExplosion(Vector3 position)
    {
        var explosion = _explosionPool.Get();
        explosion.transform.position = position;
        explosion.gameObject.SetActive(true);
    }

    protected override void Awake()
    {
        base.Awake();
        _explosionPool = new ObjectPool<ExplosionEffect>(
            CreateExplosion, OnGetExplosion, OnReleaseExplosion, OnDestroyExplosion,
            true, 10, 20);
        EventBus.Instance.Subscribe<ExplosionCompletedEvent>(OnExplosionCompleted);
    }

    void OnExplosionCompleted(ExplosionCompletedEvent explosion)
    {
        _explosionPool.Release(explosion.Explosion);
    }

    #region Explosion Pool methods
    ExplosionEffect CreateExplosion()
    {
        var explosion = Instantiate(_explosionPrefab, transform);
        explosion.gameObject.SetActive(false);
        return explosion.GetComponent<ExplosionEffect>();
    }

    void OnGetExplosion(ExplosionEffect explosion)
    {
    }

    void OnReleaseExplosion(ExplosionEffect explosion)
    {
        explosion.gameObject.SetActive(false);
    }

    void OnDestroyExplosion(ExplosionEffect explosion)
    {
        Destroy(explosion);
    }
    #endregion Explosion Pool methods

#if UNITY_EDITOR
    void Update()
    {
        if (!Mouse.current.rightButton.wasPressedThisFrame) return;
        var mousePosition = Mouse.current.position.ReadValue();
        var worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        worldPosition.z = 0;
        SpawnExplosion(worldPosition);
    }
#endif
}