using UnityEngine;

public class PlayerTouchInput : PlayerInputBase
{
    [SerializeField] private MobileJoystickInput _leftJoystick;  // For rotation
    [SerializeField] private MobileJoystickInput _rightJoystick; // For thrust and fire
    [SerializeField] private MobileButton _hyperspaceButton;     // For hyperspace

    public override bool AnyInputThisFrame =>
        (_leftJoystick != null && _leftJoystick.Direction != Vector2.zero) ||
        (_rightJoystick != null && _rightJoystick.Direction != Vector2.zero) ||
        (_hyperspaceButton != null && _hyperspaceButton.IsPressed);

    public override float GetRotationInput()
    {
        float rotationInput = -_leftJoystick.Direction.x; // Negative for clockwise rotation
        return rotationInput;
    }

    public override bool GetThrustInput()
    {
        // Thrust when joystick moves left (X < 0) or downwards (Y < 0)
        return _rightJoystick.Direction.x < 0 || _rightJoystick.Direction.y < 0;
    }

    public override bool GetFireInput()
    {
        // Fire when joystick moves right (X > 0) or upwards (Y > 0)
        return _rightJoystick.Direction.x > 0 || _rightJoystick.Direction.y > 0;
    }

    public override bool GetHyperspaceInput()
    {
        return _hyperspaceButton != null && _hyperspaceButton.IsPressed;
    }
}
