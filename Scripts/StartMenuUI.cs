using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
#if !UNITY_IOS && !UNITY_ANDROID
using UnityEngine.InputSystem;
#endif

public class StartMenuUI : MonoBehaviour
{
    [SerializeField] Image _fadePanel;
    [SerializeField] GameObject _controlsPanel, _pressSpaceText;
    [SerializeField] Button _playButton;
    [SerializeField] TMP_Text _rotateControlsText, _thrustFireControlsText, _hyperspaceControlText;
    [SerializeField] AudioSource _audioSource;
    [SerializeField] UIButton _settingsButton;
    PlayerInputBase _playerInput;
#if !UNITY_IOS && !UNITY_ANDROID
    PlayerKeyboardInput _playerKeyboardInput;
#endif

    void Awake()
    {
#if UNITY_IOS || UNITY_ANDROID
        _playButton.gameObject.SetActive(true);
        _pressSpaceText.SetActive(false);
#else
        _playerInput = gameObject.AddComponent<PlayerKeyboardInput>();
        _playerKeyboardInput = (PlayerKeyboardInput)_playerInput;
        _playButton.gameObject.SetActive(false);
        _pressSpaceText.SetActive(true);
#endif
    }

    void OnEnable()
    {
        _audioSource.volume = 1f;
        _fadePanel.DOFade(0f, 1f);
        _settingsButton?.Init(LoadSettingsScene);
#if !UNITY_IOS && !UNITY_ANDROID
        EventBus.Instance.Subscribe<SettingsSavedEvent>(OnSettingsUpdated);
#endif
        _playButton.onClick.AddListener(StartGame);
        UpdateControlsText();
    }

    void OnDisable()
    {
#if !UNITY_IOS && !UNITY_ANDROID
        EventBus.Instance?.Unsubscribe<SettingsSavedEvent>(OnSettingsUpdated);
#endif
        _playButton.onClick.RemoveListener(StartGame);
    }

#if !UNITY_IOS && !UNITY_ANDROID
    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
        {
            StartGame();
        }
    }
#endif

    void StartGame()
    {
        _audioSource.DOFade(0f, 2f);
        _fadePanel.DOFade(1f, 1f).OnComplete(() =>
        {
            SceneManager.LoadScene("Game");
        });
    }

    public void StartGameButton()
    {
        SceneManager.LoadScene("Game");
    }

    void LoadSettingsScene()
    {
        SceneManager.LoadScene("Settings", LoadSceneMode.Additive);
    }

#if !UNITY_IOS && !UNITY_ANDROID
    void OnSettingsUpdated(SettingsSavedEvent _)
    {
        _playerKeyboardInput.LoadKeyBindings();
        UpdateControlsText();
    }
#endif

    void UpdateControlsText()
    {
#if !UNITY_IOS && !UNITY_ANDROID
        _controlsPanel.SetActive(true);
        var rotateLeftKey = _playerKeyboardInput.RotateLeftKey;
        var rotateRightKey = _playerKeyboardInput.RotateRightKey;
        var thrustKey = _playerKeyboardInput.ThrustKey;
        var fireKey = _playerKeyboardInput.FireKey;
        var hyperspaceKey = _playerKeyboardInput.HyperspaceKey;

        _rotateControlsText.text = $"{rotateLeftKey} / {rotateRightKey} - Rotate Left / Right";
        _thrustFireControlsText.text = $"{thrustKey} / {fireKey} - Thrust Fire";
        _hyperspaceControlText.text = $"{hyperspaceKey} - Hyperspace";
#else
        _controlsPanel.gameObject.SetActive(false);
#endif
    }
}
