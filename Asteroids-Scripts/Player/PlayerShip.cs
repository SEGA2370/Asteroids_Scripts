using System;
using UnityEngine;

public class PlayerShip : MonoBehaviour, ICollisionParent
{
    [SerializeField] float _turnSpeed = 200f, _thrustSpeed = 120f;
    [SerializeField] GameObject _exhaust;
    [SerializeField] PlayerWeapons _playerWeapons;

    private bool _isAlive = true; // Track if the ship is alive
    public bool IsAlive => _isAlive; // Public property to check if the ship is alive

    bool _thrusting;
    Rigidbody2D _rigidBody;
    GhostParent _ghostParent;
    Renderer _renderer;
    Collider2D _collider;
    Scorer _scorer;

    private float _reviveCooldown = 1f; // Cooldown duration in seconds
    private float _lastReviveTime;


    public void Collided(Collision2D collision)
    {
        Debug.Log($"{name} collided with {collision.gameObject.name}", this);
        ExplosionSpawner.Instance.SpawnExplosion(collision.gameObject.transform.position);

        if (_isAlive) // Only disable if the ship is alive
        {
            DisableShip(); // This should only be called on actual collisions
        }

        ResetShipToStartPosition();
        _scorer?.ScorePoints(collision);
        GameManager.Instance.PlayerDied();
    }
    public void FireBullet()
    {
        _playerWeapons.FireBullet();
    }
    public void DisableShip()
    {
        if (!_isAlive) return; // Check if the ship is already disabled

        _isAlive = false; // Set alive state to false
        Debug.Log("Ship disabled. IsAlive: " + _isAlive);
        _renderer.enabled = false;
        _collider.enabled = false;
        _exhaust.SetActive(false);
        _ghostParent.EnableGhosts(false);
        EnableInvulnerability();
    }
    public void ReviveShip()
    {
        if (Time.time - _lastReviveTime < _reviveCooldown) return; // Prevent reviving too quickly

        _isAlive = true; // Set alive state to true
        Debug.Log("Ship revived. IsAlive: " + _isAlive);
        _renderer.enabled = true; // Enable the ship's renderer
        _collider.enabled = true; // Enable the ship's collider
        _exhaust.SetActive(true); // Activate the exhaust effect
        ResetShipToStartPosition(); // Reset the ship's position and velocity

        _lastReviveTime = Time.time; // Update the last revive time
    }
    public void EnableRenderer()
    {
        _renderer.enabled = true;
    }
    public void ResetShipToStartPosition()
    {
        if (!_isAlive) return; // Prevent resetting if the ship is not alive
        transform.position = Vector3.zero;
        _rigidBody.velocity = Vector2.zero;
        _rigidBody.angularVelocity = 0f;
        transform.localRotation = Quaternion.identity;
    }
    public void EnableInvulnerability()
    {
        _collider.enabled = false;
        _ghostParent.EnableGhosts(false);
        SetShipAlpha(0.25f);
    }
    public void CancelInvulnerability()
    {
        _ghostParent.EnableGhosts();
        _collider.enabled = true;
        SetShipAlpha(1f);
    }
    public void Rotate(float rotationInput)
    {
        var rotateAmount = rotationInput * _turnSpeed * Time.deltaTime;
        transform.Rotate(0, 0, rotateAmount);
    }
    public void SetThrust(bool thrusting)
    {
        _thrusting = thrusting;
    }
    public void EnterHyperspace()
    {
        transform.position = ViewportHelper.Instance.GetRandomVisiblePosition();
    }
    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider2D>();
        _rigidBody = GetComponent<Rigidbody2D>();
        _ghostParent = GetComponent<GhostParent>();
        _scorer = GetComponent<Scorer>();
    }
    void FixedUpdate()
    {
        HandleThrust();
    }
    void OnCollisionEnter2D(Collision2D other)
    {
        Collided(other);
    }
    void SetShipAlpha(float alpha)
    {
        var color = _renderer.material.color;
        color.a = alpha;
        _renderer.material.color = color;
    }
    void HandleThrust()
    {
        if (!_thrusting)
        {
            _exhaust.gameObject.SetActive(false);
            return;
        }
        _exhaust.gameObject.SetActive(true);
        var thrustAmount = _thrustSpeed * Time.fixedDeltaTime;
        var force = transform.up * thrustAmount;
        _rigidBody.AddForce(force);
    }
}
