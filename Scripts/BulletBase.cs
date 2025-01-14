using System;
using UnityEngine;

public class BulletBase : MonoBehaviour
{
    public event Action<BulletBase> OnBulletDestroyed;

    [SerializeField] GameObject _muzzleFlash, _hitEffect;
    [SerializeField] float _speed = 100f, _duration = 3f;

    Transform _transform;
    Rigidbody2D _rigidBody;
    Camera _camera;
    bool _swappingX, _swappingY;
    bool _destroyed;
    Scorer _scorer;
    Timer _timer;

    void Awake()
    {
        _camera = Camera.main;
        _transform = transform;
        _rigidBody = GetComponent<Rigidbody2D>();
        _scorer = GetComponent<Scorer>();
        _timer = TimerManager.Instance.CreateTimer<CountdownTimer>();
    }

    void OnEnable()
    {
        _destroyed = false;
        _rigidBody.AddForce(_transform.up * _speed);
        _timer.OnTimerStop += DestroyBullet;
        _timer.Start(_duration);
    }

    void Update()
    {
        if (ViewportHelper.Instance.IsOnScreen(_transform))
        {
            _swappingX = _swappingY = false;
            return;
        }
        HandleExpandedUniverseSwap();
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        DestroyBullet();
        _scorer?.ScorePoints(other);
    }

    void DestroyBullet()
    {
        if (_destroyed) return;
        _destroyed = true;
        _timer.OnTimerStop -= DestroyBullet;
        _timer.Stop();
        OnBulletDestroyed?.Invoke(this);
    }

    void HandleExpandedUniverseSwap()
    {
        if (_swappingX && _swappingY) return;
        var viewportPosition = _camera.WorldToViewportPoint(_transform.position);
        var newPosition = _transform.position;
        if (!_swappingX && (viewportPosition.x > 1 || viewportPosition.x < 0))
        {
            newPosition.x = -newPosition.x;
            _swappingX = true;
        }

        if (!_swappingY && (viewportPosition.y > 1 || viewportPosition.y < 0))
        {
            newPosition.y = -newPosition.y;
            _swappingY = true;
        }

        _transform.position = newPosition;
    }
}
