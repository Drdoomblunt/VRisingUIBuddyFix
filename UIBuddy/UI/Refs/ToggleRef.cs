using System;
using TMPro;
using UIBuddy.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace UIBuddy.UI.Refs
{
    public class ToggleRef
    {
        public Toggle Toggle
        {
            get => _toggle;
            set
            {
                _toggle = value;
                if (_toggle != null)
                {
                    _toggle.onValueChanged.AddListener(OnToggleValueChanged);
                    _toggle.onValueChanged.AddListener(_ => { _toggle.OnDeselect(null); });
                }
            }
        }

        public TextMeshProUGUI Text { get; set; }
        public GameObject GameObject { get; set; }
        public Action<bool> OnValueChanged;
        private Toggle _toggle;

        public void SetToggleValueWithoutEvent(bool value)
        {
            // Temporarily remove listener from Unity's toggle event
            Toggle.onValueChanged.RemoveListener(OnToggleValueChanged);

            // Change the value
            Toggle.isOn = value;

            // Re-add the listener
            Toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void OnToggleValueChanged(bool value)
        {
            // Call your custom action
            OnValueChanged?.Invoke(value);
        }
    }
}
