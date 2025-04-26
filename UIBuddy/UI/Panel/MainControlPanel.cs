using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIBuddy.Classes;

namespace UIBuddy.UI.Panel
{
    public class MainControlPanel: IGenericPanel
    {
        // UI Components
        private TextMeshProUGUI _nameValueText;
        private Slider _scaleSlider;
        private TMP_InputField _scaleInputField;
        private Slider _rotationSlider;
        private TMP_InputField _rotationInputField;

        private UIElement _selectedUIElement;
        private bool _updatingUI = false;
        private GameObject _titleBar;
        private readonly Canvas _ownerCanvas;
        public Vector2 ReferenceResolution { get; set; }

        public RectTransform RootRect { get; set; }
        public GameObject RootObject { get; set; }

        public UIElementDragEx Dragger { get; protected set; }

        public float GetOwnerScaleFactor()
        {
            return _ownerCanvas.scaleFactor;
        }

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
        public MainControlPanel(GameObject parent)
        {
            RootObject = UIFactory.CreateUIObject($"MarkPanel_{Guid.NewGuid()}", parent);

            _ownerCanvas = RootObject.GetComponentInParent<Canvas>();
            ReferenceResolution = RootObject.GetComponentInParent<CanvasScaler>()?.referenceResolution ?? Vector2.one;

            ConstructUI();
        }


        protected void ConstructUI()
        {
            RootObject.SetActive(true);
            RootObject.SetActive(false);

            RootRect = RootObject.GetComponent<RectTransform>();

            // Set size for the main panel
            RootRect.sizeDelta = new Vector2(300, 200);

            // Add background image
            var bgImage = RootObject.AddComponent<Image>();
            bgImage.type = Image.Type.Sliced;
            bgImage.color = Theme.PanelBackground;


            // Create vertical layout for the entire panel
            var mainLayout = UIFactory.SetLayoutGroup<VerticalLayoutGroup>(RootObject,
                childControlWidth: true,
                childControlHeight: false,
                spacing: 5,
                padTop: 5,
                padBottom: 5,
                padLeft: 5,
                padRight: 5,
                childAlignment: TextAnchor.UpperCenter);

            // Create title bar
            CreateTitleBar(RootObject);

            // Create content rows
            CreateNameRow(RootObject);
            CreateScaleRow(RootObject);
            CreateRotationRow(RootObject);

            // Position panel at the top right of the screen
            /* CustomUIRect.anchorMin = new Vector2(1, 1);
             CustomUIRect.anchorMax = new Vector2(1, 1);
             CustomUIRect.pivot = new Vector2(1, 1);
             CustomUIRect.anchoredPosition = new Vector2(-20, -20);*/
            Dragger = new UIElementDragEx(RootObject, this);

            CoroutineUtility.StartCoroutine(LateSetupCoroutine());
        }

        private IEnumerator LateSetupCoroutine()
        {
            yield return null;

            // Activate the UI
            RootObject.SetActive(true);
        }

        private void CreateTitleBar(GameObject parent)
        {
            _titleBar = UIFactory.CreateUIObject("TitleBar", parent);
            UIFactory.SetLayoutElement(_titleBar, minHeight: 30, preferredHeight: 30);

            // Add background image
            var bgImage = _titleBar.AddComponent<Image>();
            bgImage.type = Image.Type.Sliced;
            bgImage.color = Theme.SliderNormal;

            UIFactory.CreateLabel(_titleBar, "TitleLabel", "BuddyUI", fontSize: 18);
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
            var nameLabel = UIFactory.CreateLabel(nameRow, "NameLabel", "Name");
            UIFactory.SetLayoutElement(nameLabel.GameObject, minWidth: 70, preferredWidth: 70);

            // Name value
            var nameValue = UIFactory.CreateLabel(nameRow, "NameValue", "None");
            _nameValueText = nameValue.TextMesh;
            UIFactory.SetLayoutElement(nameValue.GameObject, flexibleWidth: 1);
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
            var scaleLabel = UIFactory.CreateLabel(scaleRow, "ScaleLabel", "Scale");
            UIFactory.SetLayoutElement(scaleLabel.GameObject, minWidth: 70, preferredWidth: 70);

            // Scale slider
            var sliderObj = UIFactory.CreateSlider(scaleRow, "ScaleSlider", out var slider);
            _scaleSlider = slider;
            _scaleSlider.minValue = 20.0f;
            _scaleSlider.maxValue = 100.0f;
            _scaleSlider.value = 60.0f; // Default value
            _scaleSlider.onValueChanged.AddListener(new Action<float>(OnScaleSliderChanged));
            UIFactory.SetLayoutElement(sliderObj, minWidth: 120, preferredWidth: 120, flexibleWidth: 1, minHeight: 35, preferredHeight: 35);

            // Scale input field
            var scaleInputRef = UIFactory.CreateInputField(scaleRow, "ScaleInput", "");
            _scaleInputField = scaleInputRef.Component;
            scaleInputRef.OnValueChanged += OnScaleInputChanged;
            UIFactory.SetLayoutElement(scaleInputRef.Component.gameObject, minWidth: 60, preferredWidth: 60, preferredHeight: 35, minHeight: 35);
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
            var rotationLabel = UIFactory.CreateLabel(rotationRow, "RotationLabel", "Rotation");
            UIFactory.SetLayoutElement(rotationLabel.GameObject, minWidth: 70, preferredWidth: 70);

            // Rotation slider
            var sliderObj = UIFactory.CreateSlider(rotationRow, "RotationSlider", out var slider);
            _rotationSlider = slider;
            _rotationSlider.minValue = 0.0f;
            _rotationSlider.maxValue = 360.0f;
            _rotationSlider.value = 0.0f; // Default value
            _rotationSlider.onValueChanged.AddListener(new Action<float>(OnRotationSliderChanged));
            UIFactory.SetLayoutElement(sliderObj, minWidth: 120, preferredWidth: 120, flexibleWidth: 1, preferredHeight: 35, minHeight: 35);

            // Rotation input field
            var inputRef = UIFactory.CreateInputField(rotationRow, "RotationInput", "");
            _rotationInputField = inputRef.Component;
            inputRef.OnValueChanged += OnScaleInputChanged;
            UIFactory.SetLayoutElement(inputRef.Component.gameObject, minWidth: 60, preferredWidth: 60, preferredHeight: 35, minHeight: 35);

        }

        private void UpdateUIForSelectedElement()
        {
            if(_nameValueText == null)
                return;

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

        public virtual void EnsureValidPosition()
        {
            // Prevent panel going outside screen bounds

            Vector2 pos = RootRect.anchoredPosition;
            Vector2 dimensions = ReferenceResolution;

            var x = RootRect.anchorMax.x;
            var y = RootRect.anchorMax.y;
            var mx = RootRect.anchorMin.x;
            var my = RootRect.anchorMin.y;

            float halfW = dimensions.x * 0.5f;
            float halfH = dimensions.y * 0.5f;

            float minPosX = -halfW + RootRect.rect.width * 0.5f;
            float maxPosX = halfW - RootRect.rect.width * 0.5f;
            float minPosY = -halfH + RootRect.rect.height * 0.5f;
            float maxPosY = halfH - RootRect.rect.height * 0.5f;

            pos.x = Math.Clamp(pos.x, minPosX, maxPosX);
            pos.y = Math.Clamp(pos.y, minPosY, maxPosY);

            RootRect.anchoredPosition = pos;
        }
    }
}