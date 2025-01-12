using UnityEngine;

public class PlayerTouchInput : PlayerInputBase
{
    [SerializeField] MobileJoystickInput _leftJoystick;  // For rotation
    [SerializeField] MobileJoystickInput _rightJoystick;
    [SerializeField] MobileButton _hyperspace;// For thrust/fire

    public override bool AnyInputThisFrame =>
        _leftJoystick.Direction != Vector2.zero || _rightJoystick.Direction != Vector2.zero || _hyperspace.IsPressed;

    public override float GetRotationInput()
    {
        float rotationInput = -_leftJoystick.Direction.x;
        return rotationInput; // Use horizontal input for rotation
    }

    public override bool GetThrustInput()
    {
        // Thrust when joystick moves left (X < 0) or downwards(Y < 0) 
        return _rightJoystick.Direction.x < 0 || _rightJoystick.Direction.y < 0; // Leftward direction
    }

    public override bool GetFireInput()
    {
        // Fire when joystick moves right(X > 0) or upwards(Y > 0)
        return _rightJoystick.Direction.x > 0 || _rightJoystick.Direction.y > 0; // Rightward direction
    }

    public override bool GetHyperspaceInput()
    {
        return _hyperspace.IsPressed;
    }
}
