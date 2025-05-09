using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIBuddy.UI.Refs
{
    public class SliderRef
    {
        public GameObject GameObject { get; }
        public Slider Slider { get; }

        public SliderRef(Slider slider, GameObject gameObject)
        {
            Slider = slider;
            GameObject = gameObject;

            slider.onValueChanged.AddListener(new Action<float>((value) => { OnValueChanged?.Invoke(value); }));
        }

        public Action<float> OnValueChanged;
    }
}
