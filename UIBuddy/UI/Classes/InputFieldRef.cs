using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UIBuddy.Classes;
using UnityEngine;
using UnityEngine.UI;

namespace UIBuddy.UI.Classes
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
            if (!inputsPendingUpdate.Contains(this))
                inputsPendingUpdate.Add(this);
        }





        // Static

        internal static readonly HashSet<InputFieldRef> inputsPendingUpdate = new();

        internal static void UpdateInstances()
        {
            while (inputsPendingUpdate.Any())
            {
                var inputField = inputsPendingUpdate.First();
                LayoutRebuilder.MarkLayoutForRebuild(inputField.Transform);
                inputField.OnValueChanged?.Invoke(inputField.Component.text);

                inputsPendingUpdate.Remove(inputField);
            }
        }
    }
}
