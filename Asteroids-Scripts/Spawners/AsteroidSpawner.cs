using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.InputSystem;
#endif
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public class AsteroidSpawner : SingletonMonoBehaviour<AsteroidSpawner>
{
    [SerializeField] Asteroid[] _smallAsteroidPrefabs;
    [SerializeField] Asteroid[] _mediumAsteroidPrefabs;
    [SerializeField] Asteroid[] _largeAsteroidPrefabs;
    [SerializeField] GameObject _explosionPrefab;
    [SerializeField] int _asteroidsToSpawn = 4, _maxAsteroids = 10;
    [SerializeField] float _minSpawnDistanceFromPlayer = 2f;

    readonly Dictionary<AsteroidSize, IObjectPool<Asteroid>> _asteroidPools = new();
    readonly HashSet<Asteroid> _asteroids = new();
    Transform _transform;

    int SpawnCount => Math.Min(_asteroidsToSpawn + GameManager.Instance.Round - 1, _maxAsteroids);

    public void DestroyAsteroid(Asteroid asteroid, Vector3 position)
    {
        ExplosionSpawner.Instance.SpawnExplosion(position);
        SplitAsteroid(asteroid);
        ReleaseAsteroidToPool(asteroid);
        if (_asteroids.Count == 0)
        {
            GameManager.Instance.RoundOver();
        }
    }
    protected override void Awake() // Use `override` to replace base implementation
    {
        base.Awake(); // Ensure the base class's Awake is called
        _transform = transform;
        EventBus.Instance.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
    }
    protected override void OnDestroy() // Use `override` to replace base implementation
    {
        base.OnDestroy(); // Ensure the base class's OnDestroy is called
        EventBus.Instance?.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
    }
    void OnGameStateChanged(GameStateChangedEvent gameState)
    {
        if (gameState.GameState == GameState.StartFirstRound || gameState.GameState == GameState.StartRound)
        {
            SpawnAsteroids();
        }
        else if (gameState.GameState == GameState.GameOver)
        {
            ReleaseAllAsteroids(); // Ensure all asteroids are released
        }
        if (gameState.GameState is GameState.StartFirstRound or GameState.StartRound) SpawnAsteroids();
    }
    void SpawnAsteroids()
    {
        _asteroids.Clear();
        var pool = GetPool(AsteroidSize.Large);
        for (var i = 0; i < SpawnCount; i++)
        {
            var asteroid = pool.Get();
            if (!asteroid) continue;
            var spawnPoint = GetRandomSpawnPoint();
            asteroid.Initialize(this, spawnPoint);
            _asteroids.Add(asteroid);
        }
    }
    public void ReleaseAllAsteroids()
    {
        foreach (var asteroid in _asteroids)
        {
            asteroid.Disable();
            GetPool(asteroid.Settings.Size).Release(asteroid);
        }
        _asteroids.Clear();
    }
    Vector3 GetRandomSpawnPoint()
    {
        var playerPosition = GameManager.Instance.PlayerShip?.transform.position ?? Vector3.zero;
        Vector3 spawnPoint;
        do
        {
            spawnPoint = ViewportHelper.Instance.GetRandomVisiblePosition();
        } while (Vector3.Distance(spawnPoint, playerPosition) < _minSpawnDistanceFromPlayer);

        return spawnPoint;
    }
    void SplitAsteroid(Asteroid asteroid)
    {
        if (asteroid.Settings.Size == AsteroidSize.Small) return;
        var pool = GetPool(asteroid.Settings.Size - 1);
        for (var i = 0; i < 2; i++)
        {
            var newAsteroid = pool.Get();
            if (!newAsteroid) continue;
            newAsteroid.Initialize(this, asteroid.transform.position);
            _asteroids.Add(newAsteroid);
        }
    }   
    void ReleaseAsteroidToPool(Asteroid asteroid)
    {
        // Check if the asteroid is in the list before trying to remove it
        if (_asteroids.Contains(asteroid))
        {
            _asteroids.Remove(asteroid);
        }
        else
        {
            Debug.LogWarning("Attempted to remove an asteroid that is not in the list.", this);
        }
        asteroid.gameObject.SetActive(false);
        GetPool(asteroid.Settings.Size).Release(asteroid);
    }
    IObjectPool<Asteroid> GetPool(AsteroidSize size)
    {
        if (_asteroidPools.TryGetValue(size, out var pool)) return pool;
        pool = new ObjectPool<Asteroid>(
            () => InstantiateAsteroid(size), OnTakeAsteroidFromPool, OnReturnAsteroidToPool, OnDestroyAsteroid,
            collectionCheck: true, 20, 100);
        _asteroidPools.Add(size, pool);
        return pool;
    }
    Asteroid InstantiateAsteroid(AsteroidSize size)
    {
        var prefab = GetRandomPrefab(size);
        if (!prefab)
        {
            Debug.LogError("Asteroid prefab is null.", this);
            return null;
        }
        var asteroid = Instantiate(prefab, _transform);
        asteroid.gameObject.SetActive(false);
        return asteroid;
    }
    void OnTakeAsteroidFromPool(Asteroid asteroid)
    {
    }
    void OnReturnAsteroidToPool(Asteroid asteroid)
    {
        asteroid.gameObject.SetActive(false);
    }
    void OnDestroyAsteroid(Asteroid asteroid)
    {
        Destroy(asteroid);
    }
    Asteroid GetRandomPrefab(AsteroidSize size)
    {
        Asteroid[] prefabs = size switch
        {
            AsteroidSize.Small => _smallAsteroidPrefabs,
            AsteroidSize.Medium => _mediumAsteroidPrefabs,
            AsteroidSize.Large => _largeAsteroidPrefabs,
            _ => null
        };

        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogError("Asteroid prefab is null or empty.", this);
            return null;
        }

        int randomIndex = Random.Range(0, prefabs.Length);
        return prefabs[randomIndex];
    }
}
