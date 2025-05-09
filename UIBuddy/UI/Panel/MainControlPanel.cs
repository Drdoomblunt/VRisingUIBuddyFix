using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIBuddy.Classes;
using UIBuddy.UI.Classes;

namespace UIBuddy.UI.Panel
{
    public class MainControlPanel : GenericPanelBase
    {
        // UI Components
        private TextMeshProUGUI _nameValueText;
        private Slider _scaleSlider;
        private TMP_InputField _scaleInputField;
        private Slider _rotationSlider;
        private TMP_InputField _rotationInputField;

        private ElementPanel _selectedElementPanel;
        private bool _updatingUI = false;
        private GameObject _titleBar;
        private ToggleRef _selectPanelsToggleRef;
        private ToggleRef _closeAllToggleRef;

        // Properties
        public ElementPanel SelectedElementPanel
        {
            get => _selectedElementPanel;
            set
            {
                _selectedElementPanel = value;
                UpdateUIForSelectedElement();
            }
        }

        // Constructor
        public MainControlPanel(GameObject parent)
            : base(parent, nameof(MainControlPanel))
        {
        }

        #region UI CREATION
        protected override void ConstructUI()
        {
            RootObject.SetActive(false);

            // Set size for the main panel
            RootRect.sizeDelta = new Vector2(300, 300);

            // Add background image
            var bgImage = RootObject.AddComponent<Image>();
            bgImage.type = Image.Type.Sliced;
            bgImage.color = Theme.PanelBackground;

            // Create title bar (must be created first as we'll use it for the dragger)
            CreateTitleBar(RootObject);

            // Create vertical layout for the content area (excludes the title bar)
            var contentArea = UIFactory.CreateUIObject($"ContentArea_{nameof(MainControlPanel)}", RootObject);
            var contentRect = contentArea.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 0.5f);
            // Position below the title bar
            contentRect.offsetMin = new Vector2(0, 0);
            contentRect.offsetMax = new Vector2(0, -35); // Height of the title bar

            var mainLayout = UIFactory.SetLayoutGroup<VerticalLayoutGroup>(contentArea,
                childControlWidth: true,
                childControlHeight: false,
                spacing: 5,
                padTop: 5,
                padBottom: 5,
                padLeft: 5,
                padRight: 5,
                childAlignment: TextAnchor.UpperCenter);

            // Create content rows
            CreateNameRow(contentArea);
            CreateScaleRow(contentArea);
            CreateRotationRow(contentArea);
            CreateCheckRow(contentArea);
            CreateCheckFocusRow(contentArea);
            CreateButtonsRow(contentArea);

            // Create the dragger that uses the title bar for dragging
            ConstructDrag(_titleBar, true);

            CoroutineUtility.StartCoroutine(LateSetupCoroutine());
        }

        private IEnumerator LateSetupCoroutine()
        {
            yield return null;

            // Activate the UI
            if(ConfigManager.IsModVisible)
                RootObject.SetActive(true);
        }

        private void CreateTitleBar(GameObject parent)
        {
            _titleBar = UIFactory.CreateUIObject($"TitleBar_{nameof(MainControlPanel)}", parent);
            var titleRect = _titleBar.GetComponent<RectTransform>();

            // Position the title bar at the top of the panel
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.sizeDelta = new Vector2(0, 35); // Set height to 35
            titleRect.anchoredPosition = Vector2.zero;

            // Add background image
            var bgImage = _titleBar.AddComponent<Image>();
            bgImage.type = Image.Type.Sliced;
            bgImage.color = Theme.SliderNormal;

            // Create a horizontal layout for the title label
            UIFactory.SetLayoutGroup<HorizontalLayoutGroup>(_titleBar,
                childControlWidth: true,
                childControlHeight: true,
                spacing: 0,
                padTop: 0,
                padBottom: 0,
                padLeft: 10,
                padRight: 10,
                childAlignment: TextAnchor.MiddleCenter);

            UIFactory.CreateLabel(_titleBar, $"TitleLabel_{nameof(MainControlPanel)}",
                $"UI Buddy v{MyPluginInfo.PLUGIN_VERSION}", fontSize: 18);

            var toggleContainer = UIFactory.CreateUIObject($"ToggleContainer_{Name}", _titleBar);
            var toggleContainerRect = toggleContainer.GetComponent<RectTransform>();
            toggleContainerRect.anchorMin = new Vector2(0.9f, 0);
            toggleContainerRect.anchorMax = new Vector2(1, 1);
            toggleContainerRect.pivot = new Vector2(1, 0.5f);
            toggleContainerRect.anchoredPosition = new Vector2(-10, 0);
            toggleContainerRect.sizeDelta = Vector2.zero;
            _closeAllToggleRef = UIFactory.CreateToggle(toggleContainer, $"EnableToggle_{Name}");
            _closeAllToggleRef.OnValueChanged += value =>
            {
                EnableMainPanelInternal(value);
            };
            _closeAllToggleRef.Toggle.isOn = ConfigManager.IsModVisible;
        }

        private void CreateNameRow(GameObject parent)
        {
            var nameRow = UIFactory.CreateHorizontalGroup(parent, $"NameRow_{nameof(MainControlPanel)}",
                forceExpandWidth: false,
                forceExpandHeight: false,
                childControlWidth: true,
                childControlHeight: true,
                spacing: 10,
                new Vector4(5, 5, 5, 5));

            UIFactory.SetLayoutElement(nameRow, minHeight: 30, preferredHeight: 30);

            // Name label
            var nameLabel = UIFactory.CreateLabel(nameRow, $"NameLabel_{nameof(MainControlPanel)}", "Name",
                alignment: TextAlignmentOptions.Left, fontSize: 16);
            UIFactory.SetLayoutElement(nameLabel.GameObject, minWidth: 70, preferredWidth: 70);

            // Name value
            var nameValue = UIFactory.CreateLabel(nameRow, $"NameValue_{nameof(MainControlPanel)}", "None",
                alignment: TextAlignmentOptions.Left, fontSize: 16);
            _nameValueText = nameValue.TextMesh;
            UIFactory.SetLayoutElement(nameValue.GameObject, flexibleWidth: 1);
        }

        private void CreateScaleRow(GameObject parent)
        {
            var scaleRow = UIFactory.CreateHorizontalGroup(parent, $"ScaleRow_{nameof(MainControlPanel)}",
                forceExpandWidth: false,
                forceExpandHeight: false,
                childControlWidth: true,
                childControlHeight: true,
                spacing: 10,
                new Vector4(5, 5, 5, 5));

            UIFactory.SetLayoutElement(scaleRow, minHeight: 35, preferredHeight: 35);

            // Scale label
            var scaleLabel = UIFactory.CreateLabel(scaleRow, $"ScaleLabel_{nameof(MainControlPanel)}", "Scale",
                alignment: TextAlignmentOptions.Left, fontSize: 16);
            UIFactory.SetLayoutElement(scaleLabel.GameObject, minWidth: 70, preferredWidth: 70);

            // Scale slider
            var sliderObj = UIFactory.CreateSlider(scaleRow, $"ScaleSlider_{nameof(MainControlPanel)}", out var slider);
            _scaleSlider = slider;
            _scaleSlider.minValue = 20.0f;
            _scaleSlider.maxValue = 100.0f;
            _scaleSlider.value = 60.0f; // Default value
            _scaleSlider.onValueChanged.AddListener(new Action<float>(OnScaleSliderChanged));
            UIFactory.SetLayoutElement(sliderObj, minWidth: 120, preferredWidth: 120, flexibleWidth: 1, minHeight: 35, preferredHeight: 35);

            // Scale input field
            var scaleInputRef = UIFactory.CreateInputField(scaleRow, $"ScaleInput_{nameof(MainControlPanel)}", "");
            _scaleInputField = scaleInputRef.Component;
            scaleInputRef.OnValueChanged += OnScaleInputChanged;
            UIFactory.SetLayoutElement(scaleInputRef.Component.gameObject, minWidth: 60, preferredWidth: 60, minHeight: 35, preferredHeight: 35);
        }

        private void CreateRotationRow(GameObject parent)
        {
            var rotationRow = UIFactory.CreateHorizontalGroup(parent, $"RotationRow_{nameof(MainControlPanel)}",
                forceExpandWidth: false,
                forceExpandHeight: false,
                childControlWidth: true,
                childControlHeight: true,
                spacing: 10,
                new Vector4(5, 5, 5, 5));

            UIFactory.SetLayoutElement(rotationRow, minHeight: 35, preferredHeight: 35);

            // Rotation label
            var rotationLabel = UIFactory.CreateLabel(rotationRow, $"RotationLabel_{nameof(MainControlPanel)}", "Rotation",
                alignment: TextAlignmentOptions.Left, fontSize: 16);
            UIFactory.SetLayoutElement(rotationLabel.GameObject, minWidth: 70, preferredWidth: 70);

            // Rotation slider
            var sliderObj = UIFactory.CreateSlider(rotationRow, $"RotationSlider_{nameof(MainControlPanel)}", out var slider);
            _rotationSlider = slider;
            _rotationSlider.minValue = 0.0f;
            _rotationSlider.maxValue = 360.0f;
            _rotationSlider.value = 0.0f; // Default value
            _rotationSlider.onValueChanged.AddListener(new Action<float>(OnRotationSliderChanged));
            UIFactory.SetLayoutElement(sliderObj, minWidth: 120, preferredWidth: 120, flexibleWidth: 1, minHeight: 35, preferredHeight: 35);

            // Rotation input field
            var inputRef = UIFactory.CreateInputField(rotationRow, $"RotationInput_{nameof(MainControlPanel)}", "");
            _rotationInputField = inputRef.Component;
            inputRef.OnValueChanged += OnRotationInputChanged;
            UIFactory.SetLayoutElement(inputRef.Component.gameObject, minWidth: 60, preferredWidth: 60, minHeight: 35, preferredHeight: 35);
        }

        private void CreateCheckRow(GameObject parent)
        {
            var row = UIFactory.CreateHorizontalGroup(parent, $"CheckRow_{nameof(MainControlPanel)}",
                forceExpandWidth: false,
                forceExpandHeight: false,
                childControlWidth: true,
                childControlHeight: true,
                spacing: 10,
                new Vector4(5, 5, 5, 5));

            UIFactory.SetLayoutElement(row, minHeight: 30, preferredHeight: 30);

            var toggleRef = UIFactory.CreateToggle(row, $"EnableCheck_{nameof(MainControlPanel)}");
            toggleRef.OnValueChanged += ToggleAllEnabled;
            toggleRef.Toggle.isOn = true; // Default value
            toggleRef.Text.text = "Show Panels";
            toggleRef.Text.fontSize = 16;
        }

        private void CreateCheckFocusRow(GameObject parent)
        {
            var row = UIFactory.CreateHorizontalGroup(parent, $"CheckFocusRow_{nameof(MainControlPanel)}",
                forceExpandWidth: false,
                forceExpandHeight: false,
                childControlWidth: true,
                childControlHeight: true,
                spacing: 10,
                new Vector4(5, 5, 5, 5));

            UIFactory.SetLayoutElement(row, minHeight: 30, preferredHeight: 30);

            _selectPanelsToggleRef = UIFactory.CreateToggle(row, $"EnableCheckFocus_{nameof(MainControlPanel)}");
            _selectPanelsToggleRef.OnValueChanged += (value) =>
            {
                ConfigManager.SelectPanelsWithMouse = value;
            };
            _selectPanelsToggleRef.Toggle.isOn = ConfigManager.SelectPanelsWithMouse; // Default value
            _selectPanelsToggleRef.Text.text = "Select panels with mouse";
            _selectPanelsToggleRef.Text.fontSize = 16;
        }

        private void CreateButtonsRow(GameObject parent)
        {
            var row = UIFactory.CreateHorizontalGroup(parent, $"ButtonsRow1_{nameof(MainControlPanel)}",
                forceExpandWidth: false,
                forceExpandHeight: false,
                childControlWidth: true,
                childControlHeight: true,
                spacing: 10,
                new Vector4(5, 5, 5, 5));

            UIFactory.SetLayoutElement(row, minHeight: 30, preferredHeight: 30);

            var buttonReload = UIFactory.CreateButton(row, $"ParseButton_{nameof(MainControlPanel)}", "Reload Elements");
            UIFactory.SetLayoutElement(buttonReload.GameObject, minWidth: 150, preferredWidth: 150, minHeight: 35,
                preferredHeight: 35);
            buttonReload.OnClick += () =>
            {
                buttonReload.DisableWithTimer(3000);
                PanelManager.ReloadElements();
            };
        }

        public void ToggleMainPanel()
        {
            _closeAllToggleRef.Toggle.isOn = !_closeAllToggleRef.Toggle.isOn;
        }

        private void EnableMainPanelInternal(bool value)
        {
            if(RootObject == null) return;
            PanelManager.SetPanelsActive(value);
            PanelManager.ElementListPanel.SetActive(value);
            base.SetActive(value);
            ConfigManager.IsModVisible = value;
        }

        private void UpdateUIForSelectedElement()
        {
            if (_nameValueText == null)
                return;

            if (_selectedElementPanel == null)
            {
                // No element selected, set default values
                _nameValueText.text = "None";
                _nameValueText.color = Theme.White;
                _scaleSlider.value = 60.0f; // Default value maps to 1.0 scale
                _scaleInputField.text = "1.0";
                _rotationSlider.value = 0.0f;
                _rotationInputField.text = "0.0";
                return;
            }

            _updatingUI = true;

            try
            {
                // Update UI with values from the selected element
                _nameValueText.text = _selectedElementPanel.Name;
                _nameValueText.color = Theme.ElementOutlineColor;

                // Get current scale
                float currentScale = 1.0f;
                if (_selectedElementPanel.OwnerCanvasScaler != null)
                {
                    currentScale = _selectedElementPanel.OwnerCanvasScaler.scaleFactor;
                }
                else if (_selectedElementPanel.Transform != null)
                {
                    currentScale = _selectedElementPanel.Transform.localScale.x;
                }

                // Update scale UI
                _scaleInputField.text = currentScale.ToString("F2");
                _scaleSlider.value = ScaleFactorToSliderValue(currentScale);

                // Get current rotation
                float currentRotation = 0f;
                if (_selectedElementPanel.Transform != null)
                {
                    currentRotation = _selectedElementPanel.Transform.localEulerAngles.z;
                }

                // Update rotation UI
                _rotationInputField.text = currentRotation.ToString("F1");
                _rotationSlider.value = currentRotation;
            }
            finally
            {
                _updatingUI = false;
            }
        }

        private void OnScaleSliderChanged(float value)
        {
            if (_updatingUI || _selectedElementPanel == null)
                return;

            float scaleFactor = SliderValueToScaleFactor(value);
            _scaleInputField.text = scaleFactor.ToString("F2");

            // Apply the scale to the selected element
            _selectedElementPanel.ApplyScale(scaleFactor);
        }

        private void OnScaleInputChanged(string value)
        {
            if (_updatingUI || _selectedElementPanel == null)
                return;

            if (float.TryParse(value, out float scaleFactor))
            {
                // Clamp the value to valid range
                scaleFactor = Mathf.Clamp(scaleFactor, 0.2f, 2.0f);

                // Update the slider
                _scaleSlider.value = ScaleFactorToSliderValue(scaleFactor);

                // Apply the scale to the selected element
                _selectedElementPanel.ApplyScale(scaleFactor);
            }
        }

        private void OnRotationSliderChanged(float value)
        {
            if (_updatingUI || _selectedElementPanel == null || _selectedElementPanel.Transform == null)
                return;

            _rotationInputField.text = value.ToString("F1");

            // Apply rotation to the selected element
            _selectedElementPanel.ApplyRotation(value);
        }

        private void OnRotationInputChanged(string value)
        {
            if (_updatingUI || _selectedElementPanel == null || _selectedElementPanel.Transform == null)
                return;

            if (float.TryParse(value, out float rotationValue))
            {
                // Clamp the value to valid range
                rotationValue = Mathf.Clamp(rotationValue, 0f, 360f);

                // Update the slider
                _rotationSlider.value = rotationValue;

                // Apply rotation to the selected element
                _selectedElementPanel.ApplyRotation(rotationValue);
            }
        }

        private void ToggleAllEnabled(bool value)
        {
            PanelManager.SetPanelsActive(value);
        }

        // Helper methods for scale conversion
        private float SliderValueToScaleFactor(float sliderValue)
        {
            // Map 0-100 to 0.2-2.0
            // Using a non-linear mapping for better control
            if (sliderValue <= 50)
            {
                // Map 0-50 to 0.2-1.0 (more precise control for smaller scales)
                return 0.2f + sliderValue / 50f * 0.8f;
            }
            else
            {
                // Map 50-100 to 1.0-2.0
                return 1.0f + (sliderValue - 50f) / 50f;
            }
        }

        private float ScaleFactorToSliderValue(float scaleFactor)
        {
            // Map 0.2-2.0 to 0-100
            // Inverse of the non-linear mapping above
            if (scaleFactor <= 1.0f)
            {
                // Map 0.2-1.0 to 0-50
                return (scaleFactor - 0.2f) / 0.8f * 50f;
            }
            else
            {
                // Map 1.0-2.0 to 50-100
                return 50f + (scaleFactor - 1.0f) / 1.0f * 50f;
            }
        }

        #endregion

        public override void SetActive(bool value)
        {
            _selectPanelsToggleRef.Toggle.isOn = value;
        }
    }
}