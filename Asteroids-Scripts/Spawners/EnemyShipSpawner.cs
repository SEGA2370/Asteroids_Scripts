using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShipSpawner : SingletonMonoBehaviour<EnemyShipSpawner>
{
    [SerializeField] EnemyShip[] _enemyShipPrefab;
    [SerializeField] Transform[] _spawnPoints, _waypoints;
    [Header("Spawn delay settings")]
    [SerializeField]
    float /*_initialSpawnDelay = 30f,*/
          _subsequentSpawnDelay = 10f,
          _minSpawnDelay = 5f,
          _spawnDelayDecrement = 0.1f;

    float _fastShipPercentageBase, _fastShipPercentage, _spawnDelay;
    bool _enableSpawning;
    readonly List<Vector3> _waypointsList = new();
    EnemyShip _slowShip;
    EnemyShip _fastShip;
    Timer _spawnTimer;

    public void DespawnEnemyShip(EnemyShip ship)
    {
        ship.gameObject.SetActive(false);
    }

    protected override void Awake()
    {
        base.Awake();
        EventBus.Instance.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
        _fastShipPercentage = _fastShipPercentageBase;
        _spawnDelay = _subsequentSpawnDelay;
        _slowShip = Instantiate(_enemyShipPrefab[(int)EnemyShipClass.Slow], _spawnPoints[0].position,
                                Quaternion.identity);
        _slowShip.transform.SetParent(this.transform);
        _fastShip = Instantiate(_enemyShipPrefab[(int)EnemyShipClass.Fast], _spawnPoints[0].position,
                                Quaternion.identity);
        _fastShip.transform.SetParent(this.transform);
        _spawnTimer = TimerManager.Instance.CreateTimer<CountdownTimer>(_spawnDelay);
    }

    void OnDisable()
    {
        EventBus.Instance?.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        TimerManager.Instance?.ReleaseTimer<CountdownTimer>(_spawnTimer);
    }

    void OnGameStateChanged(GameStateChangedEvent gameStateChangedEvent)
    {
        EnableSpawning(gameStateChangedEvent.GameState != GameState.GameOver);
    }

    void EnableSpawning(bool enable)
    {
        switch (enable)
        {
            case true when !_enableSpawning:
                _spawnTimer.OnTimerStop -= SpawnShip;
                _spawnTimer.OnTimerStop += SpawnShip;
                _spawnTimer.Start(_spawnDelay);
                break;
            case false when _enableSpawning:
                _spawnTimer.OnTimerStop -= SpawnShip;
                _spawnTimer.Stop();
                break;
        }
        _enableSpawning = enable;
    }

    [ContextMenu("Spawn Ship")]
    void SpawnShip()
    {
        if (!_enableSpawning) return;
        Debug.Log($"Spawning enemy ship.");
        var spawnPointIndex = UnityEngine.Random.Range(0, _spawnPoints.Length);
        var ship = GetRandomShip();
        ship.Init(this, _spawnPoints[spawnPointIndex].position, GetRandomWaypoints(spawnPointIndex));
        _fastShipPercentage = Math.Min(1f, _fastShipPercentage + 0.05f);
        _spawnDelay = Math.Max(_minSpawnDelay, _spawnDelay - _spawnDelayDecrement);
        _spawnTimer.Start(_spawnDelay);
    }

    EnemyShip GetRandomShip()
    {
        return UnityEngine.Random.value < _fastShipPercentage ? _fastShip : _slowShip;
    }

    Vector3[] GetRandomWaypoints(int spawnPointIndex)
    {
        _waypointsList.Clear();
        var verticalIndex = 0;
        if (_spawnPoints[spawnPointIndex].position.x < 0)
        {
            _waypointsList.Add(_waypoints[spawnPointIndex].position);
            for (var i = 1; i < 3; i++)
            {
                verticalIndex = UnityEngine.Random.Range(0, 3);
                var index = i * 3 + verticalIndex;
                _waypointsList.Add(_waypoints[index].position);
            }
            _waypointsList.Add(_spawnPoints[verticalIndex + 3].position);
        }
        else
        {
            _waypointsList.Add(_waypoints[spawnPointIndex + 3].position);
            for (var i = 1; i >= 0; i--)
            {
                verticalIndex = UnityEngine.Random.Range(0, 3);
                var index = i * 3 + verticalIndex;
                _waypointsList.Add(_waypoints[index].position);
            }
            _waypointsList.Add(_spawnPoints[verticalIndex].position);
        }
        return _waypointsList.ToArray();
    }
}

public enum EnemyShipClass
{
    Slow = 0,
    Fast
}