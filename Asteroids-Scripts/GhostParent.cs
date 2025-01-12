using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostParent : MonoBehaviour
{
    [SerializeField] Ghost _ghostPrefab = null;
    [SerializeField] float _swapDelay = 2f;

    public Ghost GhostPrefab => _ghostPrefab;

    public void EnableGhosts(bool enable = true)
    {
        foreach (var ghost in _ghosts)
            ghost.gameObject.SetActive(enable);
    }

    readonly List<Ghost> _ghosts = new();
    Camera _mainCamera;
    Transform _transform;
    Renderer _renderer;
    Collider2D _collider;
    Timer _swapXTimer, _swapYTimer;

    bool CanSwapX { get; set; }
    bool CanSwapY { get; set; }

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider2D>();
        _mainCamera = Camera.main;
        _transform = transform;
    }

    void OnEnable()
    {
        CreateGhosts();
        CanSwapX = CanSwapY = true;
        CreateTimers();
        EnableComponents();
    }

    void Update()
    {
        HandleScreenWrap();
    }

    void OnDisable()
    {
        ReleaseGhosts();
        DisableComponents();
        ReleaseTimers();
    }

    void CreateGhosts()
    {
        foreach (Ghost.GhostPosition position in Enum.GetValues(typeof(Ghost.GhostPosition)))
        {
            var ghost = GhostSpawner.Instance.SpawnGhost(this, position);
            _ghosts.Add(ghost);
        }
    }

    void ReleaseGhosts()
    {
        foreach (var ghost in _ghosts)
        {
            GhostSpawner.Instance?.ReleaseGhost(ghost);
        }

        _ghosts?.Clear();
    }

    void CreateTimers()
    {
        _swapXTimer = TimerManager.Instance.CreateTimer<CountdownTimer>();
        _swapYTimer = TimerManager.Instance.CreateTimer<CountdownTimer>();
        _swapXTimer.OnTimerStop += OnSwapXTimerStop;
        _swapYTimer.OnTimerStop += OnSwapYTimerStop;
    }

    void ReleaseTimers()
    {
        _swapXTimer.OnTimerStop -= OnSwapXTimerStop;
        _swapYTimer.OnTimerStop -= OnSwapYTimerStop;
        TimerManager.Instance?.ReleaseTimer<CountdownTimer>(_swapXTimer);
        TimerManager.Instance?.ReleaseTimer<CountdownTimer>(_swapYTimer);
    }

    void OnSwapXTimerStop()
    {
        CanSwapX = true;
    }

    void OnSwapYTimerStop()
    {
        CanSwapY = true;
    }

    void EnableComponents()
    {
        _renderer.enabled = true;
        _collider.enabled = true;
    }

    void DisableComponents()
    {
        _renderer.enabled = false;
        _collider.enabled = false;
    }

    void HandleScreenWrap()
    {
        if (ViewportHelper.Instance.IsOnScreen(_transform)) return;
        SwapWithGhost();
    }

    private void SwapWithGhost()
    {
        var viewportPosition = _mainCamera.WorldToViewportPoint(_transform.position);
        var newPosition = _transform.position;

        if (CanSwapX && viewportPosition.x is > 1 or < 0)
        {
            var ghostPosition = viewportPosition.x > 1
                ? Ghost.GhostPosition.MiddleLeft
                : Ghost.GhostPosition.MiddleRight;

            var ghost = _ghosts.SafeGetByIndex((int)ghostPosition);
            if (ghost == null) return;

            newPosition.x = ghost.transform.position.x;
            CanSwapX = false;
            _swapXTimer.Start(_swapDelay);
        }

        if (CanSwapY && viewportPosition.y is > 1 or < 0)
        {
            var ghostPosition = viewportPosition.y > 1
                ? Ghost.GhostPosition.LowerMiddle
                : Ghost.GhostPosition.UpperMiddle;

            var ghost = _ghosts.SafeGetByIndex((int)ghostPosition);
            if (ghost == null) return;

            newPosition.y = ghost.transform.position.y;
            CanSwapY = false;
            _swapYTimer.Start(_swapDelay);
        }

        _transform.position = newPosition;
    }

}
