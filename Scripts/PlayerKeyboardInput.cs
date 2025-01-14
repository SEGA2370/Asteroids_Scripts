using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerKeyboardInput : PlayerInputBase
{
    public Key RotateLeftKey { get; private set; } = Key.Z;
    public Key RotateRightKey { get; private set; } = Key.X;
    public Key ThrustKey { get; private set; } = Key.N;
    public Key FireKey { get; private set; } = Key.M;
    public Key HyperspaceKey { get; private set; } = Key.Space;

    float _rotationInput;
    bool _thrustInput;
    bool _fireInput;
    bool _hyperspaceInput;

    public override bool AnyInputThisFrame =>
        !Mathf.Approximately(_rotationInput, 0f) ||
        _thrustInput || _fireInput || _hyperspaceInput;

    public void LoadKeyBindings()
    {
        RotateLeftKey = (Key)SettingsManager.Instance.GetSetting<int>("RotateLeftKey", (int)Key.Z);
        RotateRightKey = (Key)SettingsManager.Instance.GetSetting<int>("RotateRightKey", (int)Key.X);
        ThrustKey = (Key)SettingsManager.Instance.GetSetting<int>("ThrustKey", (int)Key.N);
        FireKey = (Key)SettingsManager.Instance.GetSetting<int>("FireKey", (int)Key.M);
        HyperspaceKey = (Key)SettingsManager.Instance.GetSetting<int>("HyperspaceKey", (int)Key.Space);
    }

    public void SaveKeyBindings(Key rotateLeftKey, Key rotateRightKey, Key thrustKey, Key fireKey, Key hyperspaceKey)
    {
        RotateLeftKey = rotateLeftKey;
        RotateRightKey = rotateRightKey;
        ThrustKey = thrustKey;
        FireKey = fireKey;
        HyperspaceKey = hyperspaceKey;
        SettingsManager.Instance.SetSetting<int>("RotateLeftKey", (int)RotateLeftKey);
        SettingsManager.Instance.SetSetting<int>("RotateRightKey", (int)RotateRightKey);
        SettingsManager.Instance.SetSetting<int>("ThrustKey", (int)ThrustKey);
        SettingsManager.Instance.SetSetting<int>("FireKey", (int)FireKey);
        SettingsManager.Instance.SetSetting<int>("HyperspaceKey", (int)HyperspaceKey, true);
    }
  
    void Awake()
    {
        LoadKeyBindings();
    }

    void OnEnable()
    {
        EventBus.Instance.Subscribe<SettingsSavedEvent>(OnSettingsUpdated);
    }

    void OnSettingsUpdated(SettingsSavedEvent _)
    {
        LoadKeyBindings();
    }

    void OnDisable()
    {
        EventBus.Instance?.Unsubscribe<SettingsSavedEvent>(OnSettingsUpdated);
    }

    public override float GetRotationInput()
    {
        if (KeyWasPressedThisFrame(RotateLeftKey))
        {
            _rotationInput = -1;
        }
        if (KeyWasPressedThisFrame(RotateRightKey))
        {
            _rotationInput = 1;
        }
        if (KeyWasReleasedThisFrame(RotateLeftKey))
        {
            _rotationInput = 0;
        }
        if (KeyWasReleasedThisFrame(RotateRightKey))
        {
            _rotationInput = 0;
        }
        return _rotationInput;
    }

    public override bool GetThrustInput()
    {
        if (KeyWasPressedThisFrame(ThrustKey))
        {
            _thrustInput = true;
        }
        else if (KeyWasReleasedThisFrame(ThrustKey))
        {
            _thrustInput = false;
        }

        return _thrustInput;
    }

    public override bool GetFireInput()
    {
        return KeyWasPressedThisFrame(FireKey);
    }

    public override bool GetHyperspaceInput()
    {
        return KeyWasPressedThisFrame(HyperspaceKey);
    }

    bool KeyWasPressedThisFrame(Key key)
    {
        var keyboard = Keyboard.current;
        return keyboard != null && keyboard[key].wasPressedThisFrame;
    }

    bool KeyWasReleasedThisFrame(Key rotateLeftKey)
    {
        var keyboard = Keyboard.current;
        return keyboard != null && keyboard[rotateLeftKey].wasReleasedThisFrame;
    }
}
