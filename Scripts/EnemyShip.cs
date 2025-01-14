using UnityEngine;
using UnityEngine.Pool;

public class EnemyShip : MonoBehaviour, IScoreable
{
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _speed = 10f, _initialFireDelay = 3f;
    [SerializeField] private float _subsequentFireDelay = 1.5f;
    [SerializeField] private int _pointValue = 50;

    public int PointValue => _pointValue;

    private Vector3[] _waypoints;
    private int _waypointIndex;
    private LayerMask _waypointLayer;
    private EnemyShipSpawner _spawner;
    private IObjectPool<BulletBase> _bulletPool;
    private Timer _fireTimer;
    private bool _eligibleForDespawn;

    private void Awake()
    {
        _waypointLayer = LayerMask.NameToLayer("Waypoint");
        _bulletPool = new ObjectPool<BulletBase>(
            CreateBullet,
            ActivateBullet,
            DeactivateBullet,
            DestroyBullet,
            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 20
        );
    }

    public void Init(EnemyShipSpawner spawner, Vector3 startPosition, Vector3[] waypoints)
    {
        _spawner = spawner;
        _waypoints = waypoints;
        _waypointIndex = 0;
        transform.position = startPosition;
        _spriteRenderer.flipY = startPosition.x > 0f;
        _eligibleForDespawn = false;

        if (_fireTimer == null)
        {
            _fireTimer = TimerManager.Instance.CreateTimer<CountdownTimer>();
            _fireTimer.OnTimerStop += Fire;
        }

        gameObject.SetActive(true);
        _fireTimer.Start(_initialFireDelay);
    }

    private void OnDisable()
    {
        ReleaseFireTimer();
    }

    private void Update()
    {
        MoveTowardsWaypoint();
        CheckForDespawn();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != _waypointLayer) return;

        if (_waypointIndex < _waypoints.Length - 1)
        {
            _waypointIndex++;
            _eligibleForDespawn = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ExplosionSpawner.Instance.SpawnExplosion(transform.position);
        _spawner.DespawnEnemyShip(this);
    }

    private void MoveTowardsWaypoint()
    {
        if (_waypoints == null || _waypointIndex >= _waypoints.Length) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            _waypoints[_waypointIndex],
            _speed * Time.deltaTime
        );
    }

    private void CheckForDespawn()
    {
        if (_eligibleForDespawn && !ViewportHelper.Instance.IsOnScreen(transform.position))
        {
            _spawner.DespawnEnemyShip(this);
        }
    }

    private void Fire()
    {
        var bullet = _bulletPool.Get();
        bullet.transform.position = transform.position;
        bullet.transform.rotation = Quaternion.Euler(GetFireDirection());
        bullet.gameObject.SetActive(true);
        _fireTimer.Start(_subsequentFireDelay);
    }

    protected virtual Vector3 GetFireDirection()
    {
        return new Vector3(0, 0, UnityEngine.Random.Range(0, 360));
    }

    private void ReleaseFireTimer()
    {
        if (_fireTimer == null) return;

        _fireTimer.OnTimerStop -= Fire;
        TimerManager.Instance?.ReleaseTimer<CountdownTimer>(_fireTimer);
        _fireTimer = null;
    }

    #region Bullet Pool Methods
    private BulletBase CreateBullet()
    {
        var bullet = Instantiate(_bulletPrefab).GetComponent<BulletBase>();
        bullet.OnBulletDestroyed += ReturnBulletToPool;
        return bullet;
    }

    private void ActivateBullet(BulletBase bullet)
    {
        bullet.gameObject.SetActive(true);
    }

    private void DeactivateBullet(BulletBase bullet)
    {
        bullet.gameObject.SetActive(false);
    }

    private void DestroyBullet(BulletBase bullet)
    {
        bullet.OnBulletDestroyed -= ReturnBulletToPool;
        Destroy(bullet.gameObject);
    }

    private void ReturnBulletToPool(BulletBase bullet)
    {
        if (bullet != null)
        {
            _bulletPool.Release(bullet);
        }
    }
    #endregion
}
