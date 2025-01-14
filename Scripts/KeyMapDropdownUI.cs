using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeyMapDropdownUI : MonoBehaviour
{
    [SerializeField] TMP_Dropdown _dropdown;
    static readonly List<Key> Keys = new();
    static readonly List<TMP_Dropdown.OptionData> Options = new();
    Action<Key> _keySelectedAction;

    public void Init(Action<Key> onKeySelected, Key defaultKey)
    {
        PopulateDropdownOptions();
        _dropdown.value = Keys.IndexOf(defaultKey);
        _dropdown.onValueChanged.AddListener(OnIndexChanged);
        _keySelectedAction = onKeySelected;
    }

    void OnDisable()
    {
        _dropdown.onValueChanged.RemoveListener(OnIndexChanged);
    }

    void PopulateDropdownOptions()
    {
        if (Keys.Count == 0)
        {
            var keys = Enum.GetValues(typeof(Key));
            foreach (Key key in keys)
            {
                Options.Add(new TMP_Dropdown.OptionData(key.ToString()));
                Keys.Add(key);
            }
        }

        _dropdown.options = Options;
    }

    void OnIndexChanged(int index)
    {
        _keySelectedAction?.Invoke(Keys[index]);
    }
}
