using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDirector : MonoBehaviour
{
    [SerializeField] private PlayerShip _playerShip;
    [SerializeField] private float _fireDelay = 0.15f;
    [SerializeField] private float _invulnerabilityDuration = 3f;
    [SerializeField] private MobileButton _hyperspaceButton;

    private PlayerInputBase _playerInput;
    private bool _fireEnabled, _hyperspaceEnabled, _isInvulnerable;
    private Timer _enableFireTimer, _enableHyperspaceTimer, _cancelInvulnerabilityTimer;

    private void Awake()
    {
#if UNITY_IOS || UNITY_ANDROID
        _playerInput = FindObjectOfType<PlayerTouchInput>();
#else
        _playerInput = gameObject.AddComponent<PlayerKeyboardInput>();
#endif
    }

    private void OnEnable()
    {
        CreateTimers();
        EventBus.Instance.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
    }

    private void OnDisable()
    {
        EventBus.Instance?.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        ReleaseTimers();
    }

    private void Update()
    {
        if (!_playerShip.IsAlive) return;

        CheckInvulnerability();
        HandleRotationInput();
        HandleThrustInput();
        HandleFireInput();
        HandleHyperspaceInput();
    }

    private void CreateTimers()
    {
        _enableFireTimer ??= TimerManager.Instance.CreateTimer<CountdownTimer>();
        _enableHyperspaceTimer ??= TimerManager.Instance.CreateTimer<CountdownTimer>();
        _cancelInvulnerabilityTimer ??= TimerManager.Instance.CreateTimer<CountdownTimer>();
    }

    private void ReleaseTimers()
    {
        TimerManager.Instance?.ReleaseTimer<CountdownTimer>(_enableFireTimer);
        TimerManager.Instance?.ReleaseTimer<CountdownTimer>(_enableHyperspaceTimer);
        TimerManager.Instance?.ReleaseTimer<CountdownTimer>(_cancelInvulnerabilityTimer);
    }

    private void OnGameStateChanged(GameStateChangedEvent gameStateChangedEvent)
    {
        switch (gameStateChangedEvent.GameState)
        {
            case GameState.StartFirstRound:
                _playerShip.DisableShip();
                break;
            case GameState.ShipSpawned:
                _playerShip.ResetShipToStartPosition();
                _playerShip.EnableRenderer();
                EnableInvulnerability();
                EnableFire();
                DisableHyperspace();
                break;
            case GameState.PlayerDied:
            case GameState.GameOver:
                _playerShip.DisableShip();
                break;
        }
    }

    private void CheckInvulnerability()
    {
        if (!_isInvulnerable || !_playerInput.AnyInputThisFrame) return;
        CancelInvulnerability();
    }

    private void EnableInvulnerability()
    {
        if (_isInvulnerable) return;

        _isInvulnerable = true;
        _cancelInvulnerabilityTimer.OnTimerStop += CancelInvulnerability;
        _cancelInvulnerabilityTimer.Start(_invulnerabilityDuration);
        _playerShip.EnableInvulnerability();
    }

    private void CancelInvulnerability()
    {
        if (!_isInvulnerable) return;

        _isInvulnerable = false;
        _cancelInvulnerabilityTimer.OnTimerStop -= CancelInvulnerability;
        _cancelInvulnerabilityTimer.Stop();
        _playerShip.CancelInvulnerability();
    }

    private void HandleRotationInput()
    {
        float rotationInput = _playerInput.GetRotationInput();
        if (Mathf.Approximately(rotationInput, 0f)) return;
        _playerShip.Rotate(rotationInput);
    }

    private void HandleThrustInput()
    {
        _playerShip.SetThrust(_playerInput.GetThrustInput());
    }

    private void EnableFire()
    {
        _fireEnabled = true;
        _enableFireTimer.OnTimerStop -= EnableFire;
        _enableFireTimer.Stop();
    }

    private void DisableFire()
    {
        _fireEnabled = false;
        _enableFireTimer.OnTimerStop += EnableFire;
        _enableFireTimer.Start(_fireDelay);
    }

    private void HandleFireInput()
    {
        if (!_fireEnabled || !_playerInput.GetFireInput()) return;
        DisableFire();
        _playerShip.FireBullet();
    }

    private void EnableHyperspace()
    {
        _hyperspaceEnabled = true;
        _enableHyperspaceTimer.OnTimerStop -= EnableHyperspace;
        _enableHyperspaceTimer.Stop();
        _hyperspaceButton?.FadeHyperspaceButton(1f);
    }

    private void DisableHyperspace()
    {
        _hyperspaceEnabled = false;
        _enableHyperspaceTimer.OnTimerStop += EnableHyperspace;
        _enableHyperspaceTimer.Start(5f);
        _hyperspaceButton?.FadeHyperspaceButton(0f);
    }

    private void HandleHyperspaceInput()
    {
        if (!_hyperspaceEnabled || !_playerInput.GetHyperspaceInput()) return;
        DisableHyperspace();
        _playerShip.EnterHyperspace();
    }
}
