using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [SerializeField] TMP_Text _scoreText, _highScoreText, _gameOverText, _playAgainText;
    [SerializeField] Button _playAgainButton;
    [SerializeField] Transform _playerLivesParent;
    [SerializeField] UIButton _settingsButton;
    [SerializeField] GameObject _playerTouchInput;

    Timer _showPlayAgainPromptTimer;

    void OnEnable()
    {
        EventBus.Instance.Subscribe<ScoreChangedEvent>(OnScoreChanged);
        EventBus.Instance.Subscribe<PlayerLivesChangedEvent>(OnPlayerLivesChanged);
        EventBus.Instance.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
        _settingsButton.Init(LoadSettingsScene);
        UpdatePlayerLives(3);
        _gameOverText.enabled = false;
        _playAgainText.enabled = false;
        _playAgainButton.gameObject.SetActive(false);
        _showPlayAgainPromptTimer = TimerManager.Instance.CreateTimer<CountdownTimer>();
#if UNITY_IOS || UNITY_ANDROID
        _playAgainButton.onClick.AddListener(RestartGame);
        _playerTouchInput.SetActive(true);
#else
        _playerTouchInput.SetActive(false);
#endif
    }

    void OnDisable()
    {
        EventBus.Instance?.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
        EventBus.Instance?.Unsubscribe<PlayerLivesChangedEvent>(OnPlayerLivesChanged);
        EventBus.Instance?.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        _showPlayAgainPromptTimer.OnTimerStop -= ShowPlayAgainPrompt;
        TimerManager.Instance?.ReleaseTimer<CountdownTimer>(_showPlayAgainPromptTimer);
#if UNITY_IOS || UNITY_ANDROID
        _playAgainButton.onClick.RemoveListener(RestartGame);
#endif
    }

    void Update()
    {
        if (!_playAgainText.enabled) return;
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
        {
            RestartGame();
        }
    }

    public void RestartGame()
    {
        GameManager.Instance.RestartGame();
    }

    void OnGameStateChanged(GameStateChangedEvent gameStateChangedEvent)
    {
        if (gameStateChangedEvent.GameState == GameState.GameOver)
        {
            _gameOverText.enabled = true;
            _showPlayAgainPromptTimer.OnTimerStop += ShowPlayAgainPrompt;
            _showPlayAgainPromptTimer.Start(3f);
            return;
        }
        _showPlayAgainPromptTimer.OnTimerStop -= ShowPlayAgainPrompt;
        _showPlayAgainPromptTimer.Stop();
        _gameOverText.enabled = false;
        _playAgainText.enabled = false;
        _playAgainButton.gameObject.SetActive(false);
    }

    void LoadSettingsScene()
    {
        EventBus.Instance.Subscribe<SettingsSceneClosedEvent>(ResumeGame);
        GameManager.Instance.PauseGame();
        SceneManager.LoadScene("Settings", LoadSceneMode.Additive);
    }

    void ResumeGame(SettingsSceneClosedEvent _)
    {
        EventBus.Instance.Unsubscribe<SettingsSceneClosedEvent>(ResumeGame);
        GameManager.Instance.ResumeGame();
    }

    void ShowPlayAgainPrompt()
    {
#if UNITY_IOS || UNITY_ANDROID
        _playAgainButton.gameObject.SetActive(true);
#else
        _playAgainText.enabled = true;
#endif
    }

    void OnScoreChanged(ScoreChangedEvent scoreChangedEvent)
    {
        UpdateScore(scoreChangedEvent.Score, scoreChangedEvent.HighScore);
    }

    void OnPlayerLivesChanged(PlayerLivesChangedEvent playerLivesChangedEvent)
    {
        UpdatePlayerLives(playerLivesChangedEvent.Lives);
    }

    void UpdateScore(int score, int highScore)
    {
        _scoreText.text = score.ToString();
        _highScoreText.text = highScore.ToString();
    }

    void UpdatePlayerLives(int lives)
    {
        for (var i = 0; i < _playerLivesParent.childCount; i++)
        {
            _playerLivesParent.GetChild(i).gameObject.SetActive(i < lives);
        }
    }
}
