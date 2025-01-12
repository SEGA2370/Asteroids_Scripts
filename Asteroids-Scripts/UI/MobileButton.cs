using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class MobileButton : MonoBehaviour
{
    [SerializeField] CanvasGroup _hyperspaceButtonCanvasGroup;
    public bool IsPressed { get; private set; }

    RectTransform _buttonRect;
    TouchControl _cachedTouch;
    int _touchId = -1;

    void Awake()
    {
        _buttonRect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (CheckForTouchBegan()) return;

        if (_cachedTouch == null ||
            _cachedTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended ||
            _cachedTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled)
        {
            IsPressed = false;
            _touchId = -1;
            _cachedTouch = null;  // Clear the cached touch
        }
    }

    bool CheckForTouchBegan()
    {
        if (_touchId != -1) return false;
        if (Touchscreen.current == null) return false;

        var touches = Touchscreen.current.touches
            .Where(t => t.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began);

        foreach (var touch in touches)
        {
            var pointerPos = touch.position.ReadValue();
            if (!RectTransformUtility.RectangleContainsScreenPoint(_buttonRect, pointerPos, null))
            {
                continue;
            }

            _touchId = touch.touchId.ReadValue();
            _cachedTouch = touch;  // Cache the specific TouchControl
            IsPressed = true;
            return true;
        }

        return false;
    }
    public void FadeHyperspaceButton(float targetAlpha)
    {
        StartCoroutine(FadeCanvasGroup(_hyperspaceButtonCanvasGroup, targetAlpha, 1f));
    }

    // Coroutine to gradually fade the button
    IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        // Ensure it ends exactly at the target alpha value
        canvasGroup.alpha = targetAlpha;
    }
}
