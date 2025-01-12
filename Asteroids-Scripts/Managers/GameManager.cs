using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    [SerializeField] float _spawnShipDelayTime = 2f;
    [SerializeField] int _pointsForExtraLife = 10000;
    public int Round { get; private set; }
    public int Score { get; private set; }
    public int HighScore { get; private set; }
    public PlayerShip PlayerShip => _playerShip;

    GameState _gameState = GameState.StartGame;
    int Lives { get; set; }
    int _nextExtraLifeScore;
    PlayerShip _playerShip;
    Timer _nextRoundTimer, _spawnShipTimer;

    public void RestartGame()
    {
        AsteroidSpawner.Instance.ReleaseAllAsteroids();
        ReleaseTimers();
        SetGameState(GameState.StartGame); // Reset GameState
        StartGame(); // Restart from scratch
        Resources.UnloadUnusedAssets();
    }

    protected override void Awake()
    {
        base.Awake();
        _playerShip = FindObjectOfType<PlayerShip>();
    }

    void Start()
    {
        HighScore = SettingsManager.Instance.GetSetting<int>("HighScore", 0);
        StartGame();
    }

    void StartFirstRound()
    {
        if (_gameState == GameState.StartFirstRound) return; // Prevent re-triggering
        Debug.Log("Starting First Round...");
        CreateTimers();
        ++Round;
        AddPoints(0);
        SetGameState(GameState.StartFirstRound);
        StartSpawnShipTimer();
    }

    public void RoundOver()
    {
        SetGameState(GameState.RoundOver);
        StartNextRoundTimer();
    }

    public void PlayerDied()
    {
        Debug.Log("Player died");
        EventBus.Instance.Raise(new StopAllMusicEvent());
        if (Lives > 0)
        {
            SetGameState(GameState.PlayerDied);
            StartSpawnShipTimer();
            return;
        }
        GameOver();
    }

    void GameOver()
    {
        ReleaseTimers();
        SetGameState(GameState.GameOver);
        EventBus.Instance.Raise(new PlayMusicEvent("GameOver"));
        DisableAllActiveAsteroids(); // Ensure all asteroids are disabled
        System.GC.Collect();
        SettingsManager.Instance.SetSetting("HighScore", HighScore.ToString(), true);
    }
    void DisableAllActiveAsteroids()
    {
        foreach (var asteroid in FindObjectsOfType<Asteroid>())
        {
            asteroid.gameObject.SetActive(false);
        }
    }

    void StartNextRoundTimer()
    {
        _nextRoundTimer.Start(3f);
    }

    void StartSpawnShipTimer()
    {
        _spawnShipTimer.Start(_spawnShipDelayTime);
    }

    public void AddPoints(int points)
    {
        Score += points;
        if (Score > HighScore) HighScore = Score;
        EventBus.Instance.Raise(new ScoreChangedEvent(Score, HighScore));
        CheckForExtraLife();
    }
    void CheckForExtraLife()
    {
        if (Score < _nextExtraLifeScore) return;
        _nextExtraLifeScore += _pointsForExtraLife;
        SfxManager.Instance.PlayClip(SoundEffectsClip.ExtraLife);
        EventBus.Instance.Raise(new PlayerLivesChangedEvent(++Lives));
    }
    void CreateTimers()
    {
        _nextRoundTimer = TimerManager.Instance.CreateTimer<CountdownTimer>();
        _spawnShipTimer = TimerManager.Instance.CreateTimer<CountdownTimer>();
        _nextRoundTimer.OnTimerStop += StartNextRound;
        _spawnShipTimer.OnTimerStop += SpawnShip;
    }
    void ReleaseTimers()
    {
        if (_nextRoundTimer != null)
        {
            _nextRoundTimer.Stop();
            TimerManager.Instance?.ReleaseTimer<CountdownTimer>(_nextRoundTimer);
            _nextRoundTimer = null;
        }

        if (_spawnShipTimer != null)
        {
            _spawnShipTimer.Stop();
            TimerManager.Instance?.ReleaseTimer<CountdownTimer>(_spawnShipTimer);
            _spawnShipTimer = null;
        }
    }
    void StartNextRound()
    {
        ++Round;
        SetGameState(GameState.StartRound);
    }
    void StartGame()
    {
        Debug.Log("Starting Game...");
        Lives = 3;
        Score = 0;
        Round = 0;
        _nextExtraLifeScore = _pointsForExtraLife;
        _playerShip.ReviveShip();
        StartFirstRound();
    }
    void SpawnShip()
    {
        EventBus.Instance.Raise(new PlayerLivesChangedEvent(--Lives));
        SetGameState(GameState.ShipSpawned);
        _playerShip.ReviveShip(); // Call ReviveShip here to enable the ship
        _playerShip.ResetShipToStartPosition();
        _playerShip.EnableInvulnerability();
        EventBus.Instance.Raise(new PlayMusicEvent("Game"));
    }
    void SetGameState(GameState gameState)
    {
        _gameState = gameState;
        EventBus.Instance.Raise(new GameStateChangedEvent(_gameState));
    }
    public void PauseGame()
    {
        Time.timeScale = 0;
    }
    public void ResumeGame()
    {
        Time.timeScale = 1;
    }   
}
public enum GameState
{
    StartGame,
    StartFirstRound,
    StartRound,
    ShipSpawned,
    PlayerDied,
    RoundOver,
    GameOver,
}
