using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIBuddy.UI.Classes
{
    public class ToggleRef
    {
        public Toggle Toggle { get; set; }
        public TextMeshProUGUI Text { get; set; }
        public GameObject GameObject { get; set; }
        public Action<bool> OnValueChanged;
    }
}
