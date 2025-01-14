using System;
using UnityEngine;
using UnityEngine.UI;

public class UIButton : MonoBehaviour
{
    Button _button;
    Action _onButtonClickedAction;

    public void Init(Action onButtonClickedAction)
    {
        _onButtonClickedAction = onButtonClickedAction;
    }

    void Awake()
    {
        _button = GetComponent<Button>();
    }

    void OnEnable()
    {
        _button?.onClick.AddListener(OnButtonClicked);
    }

    void OnDisable()
    {
        _button?.onClick.RemoveListener(OnButtonClicked);
    }

    void OnButtonClicked()
    {
        _onButtonClickedAction?.Invoke();
    }
}