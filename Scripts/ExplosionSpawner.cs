using UnityEngine;
using UnityEngine.Pool;

public class ExplosionSpawner : SingletonMonoBehaviour<ExplosionSpawner>
{
    [SerializeField] private ExplosionEffect _explosionPrefab;

    private IObjectPool<ExplosionEffect> _explosionPool;

    protected override void Awake()
    {
        base.Awake();
        _explosionPool = new ObjectPool<ExplosionEffect>(
            CreateExplosion,
            explosion => explosion.gameObject.SetActive(true),
            explosion => explosion.gameObject.SetActive(false),
            Destroy
        );
    }

    public void SpawnExplosion(Vector3 position)
    {
        var explosion = _explosionPool.Get();
        explosion.transform.position = position;
    }

    private ExplosionEffect CreateExplosion()
    {
        return Instantiate(_explosionPrefab, transform);
    }
}
