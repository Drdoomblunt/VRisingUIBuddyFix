using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UIBuddy.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace UIBuddy.UI.Refs
{
    public class InputFieldRef
    {
        /// <summary>
        /// The RectTransform for this InputField.
        /// </summary>
        public RectTransform Transform { get; }
        /// <summary>
        /// Invoked at most once per frame, if the input was changed in the previous frame.
        /// </summary>
        public event Action<string> OnValueChanged;

        /// <summary>
        /// The actual InputField component which this object is a reference to.
        /// </summary>
        public TMP_InputField Component { get; }

        /// <summary>
        /// The placeholder Text component.
        /// </summary>
        public TextMeshProUGUI PlaceholderText { get; }

        public InputFieldRef(TMP_InputField component)
        {
            Component = component;
            Transform = component.GetComponent<RectTransform>();
            PlaceholderText = component.placeholder.TryCast<TextMeshProUGUI>();
            component.onValueChanged.AddListener(OnInputChanged);
        }


        private void OnInputChanged(string value)
        {
            if (!InputsPendingUpdate.Contains(this))
                InputsPendingUpdate.Add(this);
        }

        // Static
        private static readonly HashSet<InputFieldRef> InputsPendingUpdate = new();

        internal static void UpdateInstances()
        {
            while (InputsPendingUpdate.Any())
            {
                var inputField = InputsPendingUpdate.First();
                LayoutRebuilder.MarkLayoutForRebuild(inputField.Transform);
                inputField.OnValueChanged?.Invoke(inputField.Component.text);

                InputsPendingUpdate.Remove(inputField);
            }
        }
    }
}
