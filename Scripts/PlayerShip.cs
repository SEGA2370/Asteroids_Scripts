using System;
using UnityEngine;

public class PlayerShip : MonoBehaviour, ICollisionParent
{
    [SerializeField] private float _turnSpeed = 200f;
    [SerializeField] private float _thrustSpeed = 120f;
    [SerializeField] private GameObject _exhaust;
    [SerializeField] private PlayerWeapons _playerWeapons;

    private bool _isAlive = true;
    public bool IsAlive => _isAlive;

    private bool _isInvulnerable;
    private bool _thrusting;
    private bool _canResetPosition = true;

    private Rigidbody2D _rigidBody;
    private Renderer _renderer;
    private Collider2D _collider;
    private Scorer _scorer;

    private float _reviveCooldown = 1f;
    private float _lastReviveTime;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider2D>();
        _rigidBody = GetComponent<Rigidbody2D>();
        _scorer = GetComponent<Scorer>();

        _thrusting = false;
        _isInvulnerable = false;
        _exhaust.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (_isAlive)
        {
            HandleThrust();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Collided(collision);
    }

    public void Collided(Collision2D collision)
    {
        if (!_isAlive) return;

        ExplosionSpawner.Instance.SpawnExplosion(transform.position);
        _scorer?.ScorePoints(collision);

        DisableShip();
        GameManager.Instance.PlayerDied();
    }

    public void FireBullet()
    {
        if (_isAlive)
        {
            _playerWeapons.FireBullet();
        }
    }

    public void DisableShip()
    {
        if (!_isAlive) return;

        _isAlive = false;
        _renderer.enabled = false;
        _collider.enabled = false;
        _exhaust.SetActive(false);
        EnableInvulnerability();
    }

    public void ReviveShip()
    {
        if (Time.time - _lastReviveTime < _reviveCooldown) return;

        _isAlive = true;
        _renderer.enabled = true;
        _collider.enabled = true;
        _exhaust.SetActive(false);
        ResetShipToStartPosition();
        _lastReviveTime = Time.time;
    }

    public void EnableRenderer()
    {
        _renderer.enabled = true;
    }

    public void ResetShipToStartPosition()
    {
        if (!_isAlive || !_canResetPosition) return;

        transform.position = Vector3.zero;
        _rigidBody.velocity = Vector2.zero;
        _rigidBody.angularVelocity = 0f;
        transform.localRotation = Quaternion.identity;
    }

    public void AllowPositionReset(bool allow)
    {
        _canResetPosition = allow;
    }

    public void EnableInvulnerability()
    {
        if (_isInvulnerable) return;

        _isInvulnerable = true;
        _collider.enabled = false;
        SetShipAlpha(0.25f); // Make the ship semi-transparent
    }

    public void CancelInvulnerability()
    {
        if (!_isInvulnerable) return;

        _isInvulnerable = false;
        _collider.enabled = true;
        SetShipAlpha(1f); // Reset the ship to full opacity
    }

    public void Rotate(float rotationInput)
    {
        if (!_isAlive) return;

        var rotateAmount = rotationInput * _turnSpeed * Time.deltaTime;
        transform.Rotate(0, 0, rotateAmount);
    }

    public void SetThrust(bool thrusting)
    {
        _thrusting = thrusting;
    }

    public void EnterHyperspace()
    {
        if (_isAlive)
        {
            transform.position = ViewportHelper.Instance.GetRandomVisiblePosition();
        }
    }

    private void HandleThrust()
    {
        _exhaust.SetActive(_thrusting);

        if (_thrusting)
        {
            var thrustAmount = _thrustSpeed * Time.fixedDeltaTime;
            _rigidBody.AddForce(transform.up * thrustAmount);
        }
    }

    private void SetShipAlpha(float alpha)
    {
        var color = _renderer.material.color;
        color.a = alpha;
        _renderer.material.color = color;
    }
}
