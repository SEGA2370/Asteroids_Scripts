using DG.Tweening.Core.Easing;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class Asteroid : MonoBehaviour, ICollisionParent, IScoreable
{
    [SerializeField] AsteroidSettings _settings;

    public AsteroidSettings Settings => _settings;

    Rigidbody2D _rigidBody;
    Transform _transform;
    bool _destroyed;
    AsteroidSpawner _asteroidSpawner;
    PolygonCollider2D _collider;
    Renderer _renderer;

    public void Initialize(AsteroidSpawner asteroidSpawner, Vector3 position)
    {
        _asteroidSpawner = asteroidSpawner;
        _transform.position = position;
        _destroyed = false;
        EnableAndApplyForce();
    }

    public void Disable()
    {
        _collider.enabled = false;
        _renderer.enabled = false;
        _rigidBody.velocity = Vector2.zero;
        _rigidBody.angularVelocity = 0f;
        _rigidBody.simulated = false; // Stop physics calculations
        gameObject.SetActive(false);
    }

    public void Collided(Collision2D collision)
    {
        if (_destroyed) return;
        _destroyed = true;
        _asteroidSpawner?.DestroyAsteroid(this, collision.transform.position);
    }

    public int PointValue => _settings.Points;

    void Awake()
    {
        _transform = transform;
        _rigidBody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<PolygonCollider2D>();
        _renderer = GetComponent<Renderer>();
    }

    void OnEnable()
    {
        _destroyed = false;
        DisableComponents();
    }

    void OnDisable()
    {
        DisableComponents();
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        Collided(other);
    }

    void DisableComponents()
    {
        _collider.enabled = false;
        _renderer.enabled = false;
        _rigidBody.velocity = Vector2.zero;
        _rigidBody.angularVelocity = 0f;
    }

    void EnableAndApplyForce()
    {
        _rigidBody.simulated = true; // Re-enable physics
        gameObject.gameObject.SetActive(true);
        var force = Random.insideUnitCircle.normalized * GetRandomSpeed();
        _rigidBody.velocity = Vector2.zero;
        _rigidBody.angularVelocity = 0f;
        _rigidBody.AddForce(force, ForceMode2D.Impulse);
        _rigidBody.AddTorque(Random.Range(_settings.MinimumRotation, _settings.MaximumRotation));
        _collider.enabled = true;
        _renderer.enabled = true;
    }

    float GetRandomSpeed()
    {
        return Random.Range(_settings.MinimumSpeed, _settings.MaximumSpeed + GameManager.Instance.Round * 0.1f);
    }
}
