using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Key Bindings")]
    [SerializeField] GameObject _keyBindingsPanel;
    [SerializeField] KeyMapDropdownUI _rotateLeftDropdown;
    [SerializeField] KeyMapDropdownUI _rotateRightDropdown;
    [SerializeField] KeyMapDropdownUI _thrustDropdown;
    [SerializeField] KeyMapDropdownUI _fireDropdown;
    [SerializeField] KeyMapDropdownUI _hyperspaceDropdown;
    [Header("Audio Settings")]
    [SerializeField] Slider _masterVolumeSlider;
    [SerializeField] Slider _musicVolumeSlider;
    [SerializeField] Slider _sfxVolumeSlider;
    [SerializeField] AudioMixer _mainMix;
    [SerializeField] SoundEffectsClip _fireSound = SoundEffectsClip.PlayerBulletFire;
    [Header("Buttons")]
    [SerializeField] Button _saveButton;
    [SerializeField] Button _cancelButton;

    Key _rotateLeftKey, _rotateRightKey, _thrustKey, _fireKey, _hyperspaceKey;
    PlayerKeyboardInput _playerKeyboardInput;

    void OnEnable()
    {
#if !UNITY_IOS && !UNITY_ANDROID
        _playerKeyboardInput = gameObject.AddComponent<PlayerKeyboardInput>();
        LoadKeyBindings();
        InitialKeyboardControls();
#else
        _keyBindingsPanel.SetActive(false);
#endif
        InitializeAudioSettingsControls();
        SubscribeToFormButtonHandlers();
    }

    void OnDisable()
    {
        _saveButton.onClick.RemoveListener(OnSaveButtonClicked);
        _cancelButton.onClick.RemoveListener(OnCancelButtonClicked);
        UnsubscribeFromAudioSettingsControlHandlers();
    }

    void LoadKeyBindings()
    {
        _playerKeyboardInput.LoadKeyBindings();
        _rotateLeftKey = _playerKeyboardInput.RotateLeftKey;
        _rotateRightKey = _playerKeyboardInput.RotateRightKey;
        _thrustKey = _playerKeyboardInput.ThrustKey;
        _fireKey = _playerKeyboardInput.FireKey;
        _hyperspaceKey = _playerKeyboardInput.HyperspaceKey;
    }

    void SubscribeToFormButtonHandlers()
    {
        _saveButton.onClick.AddListener(OnSaveButtonClicked);
        _cancelButton.onClick.AddListener(OnCancelButtonClicked);
    }

    void InitialKeyboardControls()
    {
        _rotateLeftDropdown.Init(OnRotateLeftKeySelected, _rotateLeftKey);
        _rotateRightDropdown.Init(OnRotateRightKeySelected, _rotateRightKey);
        _thrustDropdown.Init(OnThrustKeySelected, _thrustKey);
        _fireDropdown.Init(OnFireKeySelected, _fireKey);
        _hyperspaceDropdown.Init(OnHyperspaceKeySelected, _hyperspaceKey);
        _keyBindingsPanel.SetActive(true);
    }

    void InitializeAudioSettingsControls()
    {
        if (_mainMix.GetFloat(AudioSettings.MasterVolumeKey, out var masterVolume))
        {
            _masterVolumeSlider.value = MixerToSliderValue(masterVolume);
        }

        if (_mainMix.GetFloat(AudioSettings.MusicVolumeKey, out var musicVolume))
        {
            _musicVolumeSlider.value = MixerToSliderValue(musicVolume);
        }

        if (_mainMix.GetFloat(AudioSettings.SfxVolumeKey, out var sfxVolume))
        {
            _sfxVolumeSlider.value = MixerToSliderValue(sfxVolume);
        }
        _masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeSliderChanged);
        _musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeSliderChanged);
        _sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeSliderChanged);
    }

    void OnRotateLeftKeySelected(Key key)
    {
        _rotateLeftKey = key;
    }

    void OnRotateRightKeySelected(Key key)
    {
        _rotateRightKey = key;
    }

    void OnThrustKeySelected(Key key)
    {
        _thrustKey = key;
    }

    void OnFireKeySelected(Key key)
    {
        _fireKey = key;
    }

    void OnHyperspaceKeySelected(Key key)
    {
        _hyperspaceKey = key;
    }

    void OnMasterVolumeSliderChanged(float sliderValue)
    {
        _mainMix.SetFloat(AudioSettings.MasterVolumeKey, SliderToMixerValue(sliderValue));
    }

    void OnMusicVolumeSliderChanged(float sliderValue)
    {
        _mainMix.SetFloat(AudioSettings.MusicVolumeKey, SliderToMixerValue(sliderValue));
    }

    void OnSfxVolumeSliderChanged(float sliderValue)
    {
        _mainMix.SetFloat(AudioSettings.SfxVolumeKey, SliderToMixerValue(sliderValue));
        SfxManager.Instance.PlayClip(_fireSound);
    }

    void UnsubscribeFromAudioSettingsControlHandlers()
    {
        _masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeSliderChanged);
        _musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeSliderChanged);
        _sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeSliderChanged);
    }

    static float MixerToSliderValue(float mixerValue)
    {
        return Mathf.InverseLerp(-80, 20, mixerValue);
    }

    static float SliderToMixerValue(float sliderValue)
    {
        return Mathf.Lerp(-80, 20, sliderValue);
    }

    void OnSaveButtonClicked()
    {
        _playerKeyboardInput.SaveKeyBindings(_rotateLeftKey, _rotateRightKey, _thrustKey, _fireKey, _hyperspaceKey);
        AudioSettings.Instance.SaveAudioSettings(
            SliderToMixerValue(_masterVolumeSlider.value),
            SliderToMixerValue(_musicVolumeSlider.value),
            SliderToMixerValue(_sfxVolumeSlider.value));
        CloseSettings();
    }

    void OnCancelButtonClicked()
    {
        CloseSettings();
    }

    void CloseSettings()
    {
        SceneManager.UnloadSceneAsync("Settings");
        EventBus.Instance.Raise(new SettingsSceneClosedEvent());
    }
}
