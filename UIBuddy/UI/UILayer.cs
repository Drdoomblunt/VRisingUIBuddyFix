using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIBuddy.Classes;

namespace UIBuddy.UI
{
    public class UILayer : UIElementDrag
    {
        // UI Components
        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _nameValueText;
        private TextMeshProUGUI _nameLabelText;
        private TextMeshProUGUI _scaleLabelText;
        private TextMeshProUGUI _textMeshScale;
        private TextMeshProUGUI _placeholderScale;
        private Slider _scaleSlider;
        private TMP_InputField _scaleInputField;
        private Slider _rotationSlider;
        private TMP_InputField _rotationInputField;

        private UIElement _selectedUIElement;
        private bool _updatingUI = false;
        private GameObject _titleBar;
        private GameObject _nameValue;
        private GameObject _nameLabel;
        private GameObject _scaleLabel;
        private GameObject _textComponentScale;
        private GameObject _placeholderObjScale;

        // Properties
        public UIElement SelectedUIElement
        {
            get => _selectedUIElement;
            set
            {
                _selectedUIElement = value;
                UpdateUIForSelectedElement();
            }
        }

        // Constructor
        public UILayer() : base(null)
        {
        }

        protected override void ConstructUI()
        {
            if (CustomUIObject != null)
                return;

            // Create the main panel
            CustomUIObject = Rect.gameObject;
            CustomUIRect = Rect;

            // Set size for the main panel
            CustomUIRect.sizeDelta = new Vector2(300, 200);

            // Add background image
            var bgImage = CustomUIObject.AddComponent<Image>();
            bgImage.type = Image.Type.Sliced;
            bgImage.color = Theme.PanelBackground;

            // Create vertical layout for the entire panel
            var mainLayout = UIFactory.SetLayoutGroup<VerticalLayoutGroup>(CustomUIObject,
                childControlWidth: true,
                childControlHeight: false,
                spacing: 5,
                padTop: 5,
                padBottom: 5,
                padLeft: 5,
                padRight: 5,
                childAlignment: TextAnchor.UpperCenter);

            // Create title bar
            CreateTitleBar(CustomUIObject);

            // Create content rows
            CreateNameRow(CustomUIObject);
            CreateScaleRow(CustomUIObject);
            //CreateRotationRow(CustomUIObject);

            // Position panel at the top right of the screen
            Rect.anchorMin = new Vector2(1, 1);
            Rect.anchorMax = new Vector2(1, 1);
            Rect.pivot = new Vector2(1, 1);
            Rect.anchoredPosition = new Vector2(-20, -20);

            CoroutineUtility.StartCoroutine(LateSetupCoroutine());
        }

        private IEnumerator LateSetupCoroutine()
        {
            yield return null;

            LateConstructUI();
            // Activate the UI
            CustomUIObject.SetActive(true);
        }

        private void LateConstructUI()
        {
            // Add title text
            _titleText = _titleBar.GetComponent<TextMeshProUGUI>();
            if (_titleText != null)
            {
                _titleText.text = "UIBuddy";
                _titleText.font = UIFactory.Font;
                _titleText.fontMaterial = UIFactory.FontMaterial;
                _titleText.fontSize = 18;
                _titleText.alignment = TextAlignmentOptions.Center;
                _titleText.color = Theme.DefaultText;
            }

            _nameValueText = _nameValue.GetComponent<TextMeshProUGUI>();
            if (_nameLabelText != null)
            {
                _nameValueText.text = "None";
                _nameValueText.font = UIFactory.Font;
                _nameValueText.fontMaterial = UIFactory.FontMaterial;
                _nameValueText.fontSize = 14;
                _nameValueText.alignment = TextAlignmentOptions.Left;
                _nameValueText.color = Theme.DefaultText;
            }

            _nameLabelText = _nameLabel.GetComponent<TextMeshProUGUI>();
            if (_nameLabelText != null)
            {
                _nameLabelText.text = "Name:";
                _nameLabelText.font = UIFactory.Font;
                _nameLabelText.fontMaterial = UIFactory.FontMaterial;
                _nameLabelText.fontSize = 14;
                _nameLabelText.alignment = TextAlignmentOptions.Left;
                _nameLabelText.color = Theme.DefaultText;
            }

            _scaleLabelText = _scaleLabel.GetComponent<TextMeshProUGUI>();
            if (_scaleLabelText != null)
            {
                _scaleLabelText.text = "Scale:";
                _scaleLabelText.font = UIFactory.Font;
                _scaleLabelText.fontMaterial = UIFactory.FontMaterial;
                _scaleLabelText.fontSize = 14;
                _scaleLabelText.alignment = TextAlignmentOptions.Left;
                _scaleLabelText.color = Theme.DefaultText;
            }

            _textMeshScale = _textComponentScale.GetComponent<TextMeshProUGUI>();
            if (_textMeshScale != null)
            {
                _textMeshScale.font = UIFactory.Font;
                _textMeshScale.fontMaterial = UIFactory.FontMaterial;
                _textMeshScale.fontSize = 14;
                _textMeshScale.alignment = TextAlignmentOptions.Center;
                _textMeshScale.color = Theme.DefaultText;
                _scaleInputField.textComponent = _textMeshScale;
            }
            
            _placeholderScale = _placeholderObjScale.GetComponent<TextMeshProUGUI>();
            if (_placeholderScale != null)
            {
                _placeholderScale.font = UIFactory.Font;
                _placeholderScale.fontMaterial = UIFactory.FontMaterial;
                _placeholderScale.fontSize = 14;
                _placeholderScale.alignment = TextAlignmentOptions.Center;
                _placeholderScale.text = "Scale";
                _placeholderScale.color =
                    new Color(Theme.DefaultText.r, Theme.DefaultText.g, Theme.DefaultText.b, 0.5f);
                _scaleInputField.placeholder = _placeholderScale;
            }
        }

        private void CreateTitleBar(GameObject parent)
        {
            _titleBar = UIFactory.CreateUIObject("TitleBar", parent);
            UIFactory.SetLayoutElement(_titleBar, minHeight: 30, preferredHeight: 30);

            // Add background image
            var bgImage = _titleBar.AddComponent<Image>();
            bgImage.type = Image.Type.Sliced;
            bgImage.color = Theme.SliderNormal;
            _titleBar.AddComponent<TextMeshProUGUI>();
        }

        private void CreateNameRow(GameObject parent)
        {
            var nameRow = UIFactory.CreateHorizontalGroup(parent, "NameRow",
                forceExpandWidth: false,
                forceExpandHeight: false,
                childControlWidth: true,
                childControlHeight: true,
                spacing: 10,
                new Vector4(5,5,5,5));

            UIFactory.SetLayoutElement(nameRow, minHeight: 30, preferredHeight: 30);

            // Name label
            _nameLabel = UIFactory.CreateUIObject("NameLabel", nameRow);
            //////_nameLabel.AddComponent<TextMeshProUGUI>();
            UIFactory.SetLayoutElement(_nameLabel, minWidth: 70, preferredWidth: 70);

            // Name value
            _nameValue = UIFactory.CreateUIObject("NameValue", nameRow);
            //////////_nameValue.AddComponent<TextMeshProUGUI>();
            UIFactory.SetLayoutElement(_nameValue, flexibleWidth: 1);
        }

        private void CreateScaleRow(GameObject parent)
        {
            var scaleRow = UIFactory.CreateHorizontalGroup(parent, "ScaleRow",
                forceExpandWidth: false,
                forceExpandHeight: false,
                childControlWidth: true,
                childControlHeight: true,
                spacing: 10,
                new Vector4(5,5,5,5));

            UIFactory.SetLayoutElement(scaleRow, minHeight: 40, preferredHeight: 40);

            // Scale label
            _scaleLabel = UIFactory.CreateUIObject("ScaleLabel", scaleRow);
            //////_scaleLabel.AddComponent<TextMeshProUGUI>();
            UIFactory.SetLayoutElement(_scaleLabel, minWidth: 70, preferredWidth: 70);

            // Scale slider
            var sliderObj = UIFactory.CreateSlider(scaleRow, "ScaleSlider", out var slider);
            _scaleSlider = slider;
            _scaleSlider.minValue = 20.0f;
            _scaleSlider.maxValue = 100.0f;
            _scaleSlider.value = 60.0f; // Default value
            _scaleSlider.onValueChanged.AddListener(new Action<float>(OnScaleSliderChanged));
            UIFactory.SetLayoutElement(sliderObj, minWidth: 120, preferredWidth: 120, flexibleWidth: 1);

            // Scale input field
            var inputObj = UIFactory.CreateUIObject("ScaleInput", scaleRow);
            _scaleInputField = inputObj.AddComponent<TMP_InputField>();
            _scaleInputField.text = "1.0";

            // Setup input field visuals
            var inputImage = inputObj.AddComponent<Image>();
            inputImage.type = Image.Type.Sliced;
            inputImage.color = Theme.SliderFill;

            var textArea = UIFactory.CreateUIObject("Text Area", inputObj);
            _textComponentScale = UIFactory.CreateUIObject("Text", textArea);
            _placeholderObjScale = UIFactory.CreateUIObject("Placeholder", textArea);

            ////_textComponentScale.AddComponent<TextMeshProUGUI>();

            ////////_placeholderObjScale.AddComponent<TextMeshProUGUI>();

            _scaleInputField.textViewport = textArea.GetComponent<RectTransform>();
            _scaleInputField.onEndEdit.AddListener(new Action<string>(OnScaleInputChanged));

            UIFactory.SetLayoutElement(inputObj, minWidth: 60, preferredWidth: 60);
        }

        private void CreateRotationRow(GameObject parent)
        {
            var rotationRow = UIFactory.CreateHorizontalGroup(parent, "RotationRow",
                forceExpandWidth: false,
                forceExpandHeight: false,
                childControlWidth: true,
                childControlHeight: true,
                spacing: 10,
                new Vector4(5, 5, 5, 5));

            UIFactory.SetLayoutElement(rotationRow, minHeight: 40, preferredHeight: 40);

            // Rotation label
            var rotationLabel = UIFactory.CreateUIObject("RotationLabel", rotationRow);
            var rotationLabelText = rotationLabel.AddComponent<TextMeshProUGUI>();
            rotationLabelText.text = "Rotation:";
            rotationLabelText.font = UIFactory.Font;
            rotationLabelText.fontMaterial = UIFactory.FontMaterial;
            rotationLabelText.fontSize = 14;
            rotationLabelText.alignment = TextAlignmentOptions.Left;
            rotationLabelText.color = Theme.DefaultText;
            UIFactory.SetLayoutElement(rotationLabel, minWidth: 70, preferredWidth: 70);

            // Rotation slider
            var sliderObj = UIFactory.CreateSlider(rotationRow, "RotationSlider", out var slider);
            _rotationSlider = slider;
            _rotationSlider.minValue = 0.0f;
            _rotationSlider.maxValue = 360.0f;
            _rotationSlider.value = 0.0f; // Default value
            _rotationSlider.onValueChanged.AddListener(new Action<float>(OnRotationSliderChanged));
            UIFactory.SetLayoutElement(sliderObj, minWidth: 120, preferredWidth: 120, flexibleWidth: 1);

            // Rotation input field
            var inputObj = UIFactory.CreateUIObject("RotationInput", rotationRow);
            _rotationInputField = inputObj.AddComponent<TMP_InputField>();
            _rotationInputField.text = "0.0";

            // Setup input field visuals
            var inputImage = inputObj.AddComponent<Image>();
            inputImage.type = Image.Type.Sliced;
            inputImage.color = Theme.SliderFill;

            var textArea = UIFactory.CreateUIObject("Text Area", inputObj);
            var textComponent = UIFactory.CreateUIObject("Text", textArea);
            var placeholderObj = UIFactory.CreateUIObject("Placeholder", textArea);

            var textMesh = textComponent.AddComponent<TextMeshProUGUI>();
            textMesh.font = UIFactory.Font;
            textMesh.fontMaterial = UIFactory.FontMaterial;
            textMesh.fontSize = 14;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.color = Theme.DefaultText;

            var placeholder = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholder.font = UIFactory.Font;
            placeholder.fontMaterial = UIFactory.FontMaterial;
            placeholder.fontSize = 14;
            placeholder.alignment = TextAlignmentOptions.Center;
            placeholder.text = "Rotate";
            placeholder.color = new Color(Theme.DefaultText.r, Theme.DefaultText.g, Theme.DefaultText.b, 0.5f);

            _rotationInputField.textViewport = textArea.GetComponent<RectTransform>();
            _rotationInputField.textComponent = textMesh;
            _rotationInputField.placeholder = placeholder;
            _rotationInputField.onEndEdit.AddListener(new Action<string>(OnRotationInputChanged));

            UIFactory.SetLayoutElement(inputObj, minWidth: 60, preferredWidth: 60);
        }

        private void UpdateUIForSelectedElement()
        {
            if (_selectedUIElement == null)
            {
                // No element selected, set default values
                _nameValueText.text = "None";
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
                _nameValueText.text = _selectedUIElement.Name;

                // Get current scale
                float currentScale = 1.0f;
                if (_selectedUIElement.OwnerCanvasScaler != null)
                {
                    currentScale = _selectedUIElement.OwnerCanvasScaler.scaleFactor;
                }
                else if (_selectedUIElement.Transform != null)
                {
                    currentScale = _selectedUIElement.Transform.localScale.x;
                }

                // Update scale UI
                _scaleInputField.text = currentScale.ToString("F2");
                _scaleSlider.value = ScaleFactorToSliderValue(currentScale);

                // Get current rotation
                float currentRotation = 0f;
                if (_selectedUIElement.Transform != null)
                {
                    currentRotation = _selectedUIElement.Transform.localEulerAngles.z;
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
            if (_updatingUI || _selectedUIElement == null)
                return;

            float scaleFactor = SliderValueToScaleFactor(value);
            _scaleInputField.text = scaleFactor.ToString("F2");

            // Apply the scale to the selected element
            _selectedUIElement.ApplyScale(scaleFactor);
        }

        private void OnScaleInputChanged(string value)
        {
            if (_updatingUI || _selectedUIElement == null)
                return;

            if (float.TryParse(value, out float scaleFactor))
            {
                // Clamp the value to valid range
                scaleFactor = Mathf.Clamp(scaleFactor, 0.2f, 2.0f);

                // Update the slider
                _scaleSlider.value = ScaleFactorToSliderValue(scaleFactor);

                // Apply the scale to the selected element
                _selectedUIElement.ApplyScale(scaleFactor);
            }
        }

        private void OnRotationSliderChanged(float value)
        {
            if (_updatingUI || _selectedUIElement == null || _selectedUIElement.Transform == null)
                return;

            _rotationInputField.text = value.ToString("F1");

            // Apply rotation to the selected element
            Vector3 eulerAngles = _selectedUIElement.Transform.localEulerAngles;
            _selectedUIElement.Transform.localEulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, value);
        }

        private void OnRotationInputChanged(string value)
        {
            if (_updatingUI || _selectedUIElement == null || _selectedUIElement.Transform == null)
                return;

            if (float.TryParse(value, out float rotationValue))
            {
                // Clamp the value to valid range
                rotationValue = Mathf.Clamp(rotationValue, 0f, 360f);

                // Update the slider
                _rotationSlider.value = rotationValue;

                // Apply rotation to the selected element
                Vector3 eulerAngles = _selectedUIElement.Transform.localEulerAngles;
                _selectedUIElement.Transform.localEulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, rotationValue);
            }
        }

        // Helper methods for scale conversion
        private float SliderValueToScaleFactor(float sliderValue)
        {
            // Map 0-100 to 0.2-2.0
            // Using a non-linear mapping for better control
            if (sliderValue <= 50)
            {
                // Map 0-50 to 0.2-1.0 (more precise control for smaller scales)
                return 0.2f + (sliderValue / 50f) * 0.8f;
            }
            else
            {
                // Map 50-100 to 1.0-2.0
                return 1.0f + ((sliderValue - 50f) / 50f);
            }
        }

        private float ScaleFactorToSliderValue(float scaleFactor)
        {
            // Map 0.2-2.0 to 0-100
            // Inverse of the non-linear mapping above
            if (scaleFactor <= 1.0f)
            {
                // Map 0.2-1.0 to 0-50
                return ((scaleFactor - 0.2f) / 0.8f) * 50f;
            }
            else
            {
                // Map 1.0-2.0 to 50-100
                return 50f + ((scaleFactor - 1.0f) / 1.0f) * 50f;
            }
        }
    }
}