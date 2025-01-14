using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _scoreText, _highScoreText, _gameOverText, _playAgainText;
    [SerializeField] private Button _playAgainButton;
    [SerializeField] private Transform _playerLivesParent;
    [SerializeField] private UIButton _settingsButton;
    [SerializeField] private GameObject _playerTouchInput;

    private Timer _showPlayAgainPromptTimer;
    private bool _isGameOver;

    private void OnEnable()
    {
        EventBus.Instance.Subscribe<ScoreChangedEvent>(OnScoreChanged);
        EventBus.Instance.Subscribe<PlayerLivesChangedEvent>(OnPlayerLivesChanged);
        EventBus.Instance.Subscribe<GameStateChangedEvent>(OnGameStateChanged);

        _settingsButton.Init(LoadSettingsScene);
        _playAgainButton.onClick.AddListener(RestartGame);

        ResetUI();
        UpdatePlayerLives(3);

        _showPlayAgainPromptTimer = TimerManager.Instance.CreateTimer<CountdownTimer>();
#if UNITY_IOS || UNITY_ANDROID
        _playerTouchInput.SetActive(true);
#else
        _playerTouchInput.SetActive(false);
#endif
    }

    private void OnDisable()
    {
        EventBus.Instance?.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
        EventBus.Instance?.Unsubscribe<PlayerLivesChangedEvent>(OnPlayerLivesChanged);
        EventBus.Instance?.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);

        if (_showPlayAgainPromptTimer != null)
        {
            _showPlayAgainPromptTimer.OnTimerStop -= ShowPlayAgainPrompt;
            TimerManager.Instance.ReleaseTimer<CountdownTimer>(_showPlayAgainPromptTimer);
        }

        _playAgainButton.onClick.RemoveListener(RestartGame);
    }

    private void OnGameStateChanged(GameStateChangedEvent gameStateChangedEvent)
    {
        if (gameStateChangedEvent.GameState == GameState.GameOver)
        {
            _isGameOver = true;
            _gameOverText.enabled = true;

            Debug.Log("GameOver detected. Starting prompt timer...");
            _showPlayAgainPromptTimer.OnTimerStop += ShowPlayAgainPrompt;
            _showPlayAgainPromptTimer.Start(2f);
        }
        else
        {
            _isGameOver = false;
            ResetUI();
        }
    }

    private void ShowPlayAgainPrompt()
    {
        _playAgainButton.gameObject.SetActive(true);
        _playAgainText.enabled = true;

        Debug.Log("Play Again prompt shown.");
        _showPlayAgainPromptTimer.OnTimerStop -= ShowPlayAgainPrompt;
        _showPlayAgainPromptTimer.Stop();
    }

    public void RestartGame()
    {
        if (!_isGameOver) return;

        Debug.Log("RestartGame triggered via Play Again button.");
        ResetUI();
        _isGameOver = false;
        GameManager.Instance.RestartGame();
    }

    private void ResetUI()
    {
        _gameOverText.enabled = false;
        _playAgainText.enabled = false;
        _playAgainButton.gameObject.SetActive(false);
    }

    private void OnScoreChanged(ScoreChangedEvent scoreChangedEvent)
    {
        _scoreText.text = scoreChangedEvent.Score.ToString();
        _highScoreText.text = scoreChangedEvent.HighScore.ToString();
    }

    private void OnPlayerLivesChanged(PlayerLivesChangedEvent playerLivesChangedEvent)
    {
        UpdatePlayerLives(playerLivesChangedEvent.Lives);
    }

    private void UpdatePlayerLives(int lives)
    {
        for (var i = 0; i < _playerLivesParent.childCount; i++)
        {
            _playerLivesParent.GetChild(i).gameObject.SetActive(i < lives);
        }
    }

    private void LoadSettingsScene()
    {
        EventBus.Instance.Subscribe<SettingsSceneClosedEvent>(ResumeGame);
        GameManager.Instance.PauseGame();
        SceneManager.LoadScene("Settings", LoadSceneMode.Additive);
    }

    private void ResumeGame(SettingsSceneClosedEvent _)
    {
        EventBus.Instance.Unsubscribe<SettingsSceneClosedEvent>(ResumeGame);
        GameManager.Instance.ResumeGame();
    }
}
