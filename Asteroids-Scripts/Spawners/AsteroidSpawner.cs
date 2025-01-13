using System.Collections.Generic;
using System;
using UnityEngine.Pool;
using UnityEngine;
using Random = UnityEngine.Random;

public class AsteroidSpawner : MonoBehaviour
{
    [SerializeField] private Asteroid[] _smallAsteroidPrefabs;
    [SerializeField] private Asteroid[] _mediumAsteroidPrefabs;
    [SerializeField] private Asteroid[] _largeAsteroidPrefabs;
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private int _asteroidsToSpawn = 4, _maxAsteroids = 10;
    [SerializeField] private float _minSpawnDistanceFromPlayer = 2f;

    private readonly Dictionary<AsteroidSize, IObjectPool<Asteroid>> _asteroidPools = new();
    private readonly List<Asteroid> _asteroids = new();
    private Transform _transform;
    private bool _isSpawning = false; // Prevent multiple spawn triggers

    public static AsteroidSpawner Instance { get; private set; }
    // Calculate the number of asteroids to spawn based on the round
    private int SpawnCount => Mathf.Min(_asteroidsToSpawn + GameManager.Instance.Round - 1, _maxAsteroids);
    public int ActiveAsteroidsCount => _asteroids.Count;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        _transform = transform;
        EventBus.Instance.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
    }

    private void OnDestroy()
    {
        EventBus.Instance?.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
    }

    private void OnGameStateChanged(GameStateChangedEvent gameState)
    {
        // Only start spawning when the game state is appropriate
        if (gameState.GameState == GameState.StartFirstRound || gameState.GameState == GameState.StartRound)
        {
            SpawnAsteroids();
        }
    }

    private void SpawnAsteroids()
    {
        if (_isSpawning || ActiveAsteroidsCount > 0) return; // Prevent spawning if already in progress or if asteroids are active

        _isSpawning = true;
        _asteroids.Clear();
        var pool = GetPool(AsteroidSize.Large); // Start with large asteroids for the round

        for (var i = 0; i < SpawnCount; i++)
        {
            var asteroid = pool.Get();
            if (!asteroid) continue;

            var spawnPoint = GetRandomSpawnPoint();
            asteroid.Initialize(this, spawnPoint);
            _asteroids.Add(asteroid);
        }

        _isSpawning = false; // Allow spawning to occur again next round
    }

    private Vector3 GetRandomSpawnPoint()
    {
        var playerPosition = GameManager.Instance.PlayerShip?.transform.position ?? Vector3.zero;
        Vector3 spawnPoint;
        do
        {
            spawnPoint = ViewportHelper.Instance.GetRandomVisiblePosition();
        } while (Vector3.Distance(spawnPoint, playerPosition) < _minSpawnDistanceFromPlayer);

        return spawnPoint;
    }

    // Handle asteroid destruction and splitting
    public void DestroyAsteroid(Asteroid asteroid, Vector3 position)
    {
        ExplosionSpawner.Instance.SpawnExplosion(position);
        SplitAsteroid(asteroid);
        ReleaseAsteroidToPool(asteroid);

        // If no asteroids are left, notify the game manager
        if (_asteroids.Count == 0)
        {
            GameManager.Instance.RoundOver();
        }
    }

    private void SplitAsteroid(Asteroid asteroid)
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

    private void ReleaseAsteroidToPool(Asteroid asteroid)
    {
        // Prevent releasing an already released asteroid
        if (!_asteroids.Contains(asteroid)) return;

        asteroid.gameObject.SetActive(false);
        _asteroids.Remove(asteroid);
        GetPool(asteroid.Settings.Size).Release(asteroid);
    }

    private IObjectPool<Asteroid> GetPool(AsteroidSize size)
    {
        if (_asteroidPools.TryGetValue(size, out var pool)) return pool;

        pool = new ObjectPool<Asteroid>(
            () => InstantiateAsteroid(size),
            OnTakeAsteroidFromPool,
            OnReturnAsteroidToPool,
            OnDestroyAsteroid,
            collectionCheck: true, 20, 100
        );

        _asteroidPools.Add(size, pool);
        return pool;
    }

    private Asteroid InstantiateAsteroid(AsteroidSize size)
    {
        var prefab = GetRandomPrefab(size);
        if (!prefab)
        {
            return null;
        }

        var asteroid = Instantiate(prefab, _transform);
        asteroid.gameObject.SetActive(false);
        return asteroid;
    }

    private void OnTakeAsteroidFromPool(Asteroid asteroid) { }

    private void OnReturnAsteroidToPool(Asteroid asteroid)
    {
        asteroid.gameObject.SetActive(false);
    }

    private void OnDestroyAsteroid(Asteroid asteroid)
    {
        Destroy(asteroid);
    }

    private Asteroid GetRandomPrefab(AsteroidSize size)
    {
        return size switch
        {
            AsteroidSize.Small => _smallAsteroidPrefabs[Random.Range(0, _smallAsteroidPrefabs.Length)],
            AsteroidSize.Medium => _mediumAsteroidPrefabs[Random.Range(0, _mediumAsteroidPrefabs.Length)],
            AsteroidSize.Large => _largeAsteroidPrefabs[Random.Range(0, _largeAsteroidPrefabs.Length)],
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public void ReleaseAllAsteroids()
    {
        foreach (var asteroid in _asteroids)
        {
            asteroid.gameObject.SetActive(false);
            GetPool(asteroid.Settings.Size).Release(asteroid);
        }

        _asteroids.Clear();
    }
    public void ResetSpawner()
    {
        ReleaseAllAsteroids();
        _isSpawning = false;
    }
}
