using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class MobileJoystickInput : MonoBehaviour
{
    [SerializeField] RectTransform _background;
    [SerializeField] RectTransform _handle;
    [SerializeField] float _handleRange = 100f;

    public Vector2 Direction { get; private set; }

    private Vector2 _input = Vector2.zero;
    private int _touchId = -1;

    void Update()
    {
        if (_touchId == -1)
        {
            CheckForDragStart();
            return;
        }

        HandleDragOrStop();
    }

    void CheckForDragStart()
    {
        if (Touchscreen.current == null) return;

        var touches = Touchscreen.current.touches
            .Where(t => t.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began);

        foreach (var touch in touches)
        {
            var pointerPos = touch.position.ReadValue();
            if (RectTransformUtility.RectangleContainsScreenPoint(_background, pointerPos, null))
            {
                _touchId = touch.touchId.ReadValue();
                break;
            }
        }
    }

    void HandleDragOrStop()
    {
        var touch = Touchscreen.current.touches.FirstOrDefault(t => t.touchId.ReadValue() == _touchId);

        if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended ||
            touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled)
        {
            ResetHandle();
            _touchId = -1;
            return;
        }

        var pointerPos = touch.position.ReadValue();
        var direction = pointerPos - (Vector2)_background.position;

        _input = Vector2.ClampMagnitude(direction, _handleRange);
        _handle.anchoredPosition = _input;
        Direction = _input / _handleRange;
    }

    void ResetHandle()
    {
        _handle.anchoredPosition = Vector2.zero;
        Direction = Vector2.zero;
    }
}
