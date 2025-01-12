using UnityEngine;
using UnityEngine.Pool;

public class EnemyShip : MonoBehaviour, IScoreable
{
    [SerializeField] protected GameObject _bulletPrefab;
    [SerializeField] SpriteRenderer _spriteRenderer;
    [SerializeField] float _speed = 10f, _initialFireDelay = 3f;
    [SerializeField] float _subsequentFireDelay = 1.5f;
    [SerializeField] int _pointValue = 50;

    public int PointValue => _pointValue;

    public Vector3[] _waypoints;
    int _waypointIndex;
    LayerMask _waypointLayer;
    EnemyShipSpawner _spawner;
    IObjectPool<BulletBase> _bulletPool;
    bool _eligibleForDespawn;
    Timer _fireTimer;

    void Awake()
    {
        _waypointLayer = LayerMask.NameToLayer("Waypoint");
        _bulletPool = new ObjectPool<BulletBase>(
            InstantiateBullet,
            OnGetBullet,
            OnReleaseBullet,
            OnDestroyBullet,
            true, 5, 10);
    }

    public void Init(EnemyShipSpawner spawner, Vector3 startPosition, Vector3[] waypoints)
    {
        _spawner = spawner;
        _waypoints = waypoints;
        _waypointIndex = 0;
        transform.position = startPosition;
        _spriteRenderer.flipY = startPosition.x > 0f;
        _eligibleForDespawn = false;
        CreateFireTimer();
        gameObject.SetActive(true);
        _fireTimer.Start(_initialFireDelay);
    }

    void OnDisable()
    {
        ReleaseFireTimer();
    }

    void Update()
    {
        CheckForDespawn();
        MoveShip();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != _waypointLayer) return;
        // Ensure the index does not exceed the array bounds
        if (_waypointIndex < _waypoints.Length - 1)
        {
            ++_waypointIndex;
            _eligibleForDespawn = true;
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        ExplosionSpawner.Instance.SpawnExplosion(transform.position);
        _spawner.DespawnEnemyShip(this);
    }

    void CreateFireTimer()
    {
        _fireTimer = TimerManager.Instance.CreateTimer<CountdownTimer>();
        _fireTimer.OnTimerStop += Fire;
    }

    void ReleaseFireTimer()
    {
        _fireTimer.OnTimerStop -= Fire;
        TimerManager.Instance?.ReleaseTimer<CountdownTimer>(_fireTimer);
    }

    void Fire()
    {
        SfxManager.Instance.PlayClip(SoundEffectsClip.EnemyBulletFire);
        var bullet = _bulletPool.Get();
        bullet.transform.SetParent(_spawner.transform);
        bullet.transform.position = transform.position;
        bullet.transform.eulerAngles = GetFireDirection();
        bullet.gameObject.SetActive(true);
        _fireTimer.Start(_subsequentFireDelay);
    }

    protected virtual Vector3 GetFireDirection()
    {
        return new Vector3(0, 0, UnityEngine.Random.Range(0, 360));
    }

    void MoveShip()
    {
        if (_waypoints == null || _waypoints.Length == 0 || _waypointIndex >= _waypoints.Length) return;
        transform.position = Vector3.MoveTowards(transform.position, _waypoints[_waypointIndex], _speed * Time.deltaTime);
    }

    void CheckForDespawn()
    {
        if (!_eligibleForDespawn || ViewportHelper.Instance.IsOnScreen(transform)) return;
        _spawner.DespawnEnemyShip(this);
    }

    #region Bullet pool
    BulletBase InstantiateBullet()
    {
        var bullet = Instantiate(_bulletPrefab, transform.position, Quaternion.identity).GetComponent<BulletBase>();
        bullet.OnBulletDestroyed += OnBulletDestroyed;
        return bullet;
    }

    void OnGetBullet(BulletBase bullet)
    {
        bullet.gameObject.SetActive(false);
    }

    void OnReleaseBullet(BulletBase bullet)
    {
        bullet.transform.SetParent(null);
        bullet.gameObject.SetActive(false);
    }

    void OnDestroyBullet(BulletBase bullet)
    {
        bullet.transform.SetParent(null);
        bullet.OnBulletDestroyed -= OnBulletDestroyed;
        Destroy(bullet.gameObject);
    }

    void OnBulletDestroyed(BulletBase bullet)
    {
        bullet.gameObject.SetActive(false);
        _bulletPool.Release(bullet);
    }
    #endregion Bullet pool
}