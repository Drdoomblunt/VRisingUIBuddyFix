using System;
using UnityEngine;
using UnityEngine.UI;
using static Il2CppSystem.Linq.Expressions.Interpreter.NullableMethodCallInstruction;

namespace UIBuddy.UI
{
    public class UIElementControlPanel
    {
        private GameObject RootObject { get; }
        public GameObject TitleBar { get; private set; }
        private GameObject FloatingControlPanel { get; set; }
        private RectTransform FloatingControlRect { get; set; }
        
        // Offset for the floating panel relative to the root object
        private readonly Vector2 _floatingPanelOffset = new Vector2(0, 10);
        // Track the last position to detect changes
        private Vector2 _lastRootPosition;
        private float _stepSize = 0.05f;

        private RectTransform RootRect { get; set; }
        public Action<float> ScaleChanged { get; set; }

        public UIElementControlPanel(GameObject parent, float scaleFactor)
        {
            RootObject = UIFactory.CreateUIObject($"MarkPanel_{Guid.NewGuid()}", parent);
            RootRect = RootObject.GetComponent<RectTransform>();
            ConstructUI();

            // Initialize the floating control panel
            CreateFloatingControlPanel(scaleFactor);

            // Set initial position of the floating panel
            UpdateFloatingPanelPosition();
        }

        private void CreateFloatingControlPanel(float scaleFactor)
        {
            // Create the floating panel in the same parent as the root object
            Transform parentTransform = RootObject.transform.parent;
            if (parentTransform == null)
                return;

            // Create the floating panel
            FloatingControlPanel = UIFactory.CreateUIObject("FloatingControlPanel", parentTransform.gameObject);
            FloatingControlRect = FloatingControlPanel.GetComponent<RectTransform>();

            // Set up the floating panel's RectTransform
            if (FloatingControlRect != null)
            {
                // Set initial position relative to the root object
                FloatingControlRect.anchorMin = new Vector2(0.5f, 0.5f);
                FloatingControlRect.anchorMax = new Vector2(0.5f, 0.5f);
                FloatingControlRect.pivot = new Vector2(0.5f, 0);  // Pivot at bottom center

                // Set the panel size
                FloatingControlRect.sizeDelta = new Vector2(200, 30);
            }

            // Add background image
            var bgImage = FloatingControlPanel.AddComponent<Image>();
            bgImage.type = Image.Type.Sliced;
            bgImage.color = Theme.SliderNormal;

            // Add horizontal layout
            var layout = FloatingControlPanel.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset() { left = 4, top = 4, right = 4, bottom = 4 };
            layout.spacing = 4;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            // Create scale slider
            var scaleSlider = UIFactory.CreateSlider(FloatingControlPanel, "ScaleSlider", out var slider);

            if (slider != null)
            {
                ScaleSlider = slider;
                // Configure the slider
                slider.value = scaleFactor;
                slider.minValue = 0.2f;
                slider.maxValue = 2.0f;

                // Set up the layout element for the slider
                var sliderLayoutElement = scaleSlider.AddComponent<LayoutElement>();
                sliderLayoutElement.minHeight = 20;
                sliderLayoutElement.minWidth = 150;
                sliderLayoutElement.preferredWidth = 150;
                sliderLayoutElement.flexibleWidth = 0;

                // Register the value change callback
                slider.onValueChanged.AddListener(new Action<float>(OnScaleValueChanged));
            }
        }

        public Slider ScaleSlider { get; set; }

        private float RoundToStepSize(float value)
        {
            // Round the value to the nearest step
            return Mathf.Round(value / _stepSize) * _stepSize;
        }

        private void UpdateFloatingPanelPosition()
        {
            if (RootRect == null || FloatingControlRect == null)
                return;

            // Calculate the position above the root object
            Vector2 rootPos = RootRect.anchoredPosition;
            float yOffset = (RootRect.rect.height / 2) + _floatingPanelOffset.y;

            // Position the floating panel above the target
            FloatingControlRect.anchoredPosition = new Vector2(rootPos.x, rootPos.y + yOffset);
        }


        private void ConstructUI()
        {
            if (RootObject == null)
                return;

            // Get or add RectTransform
            var rect = RootObject.GetComponent<RectTransform>();
            if (rect == null)
                rect = RootObject.AddComponent<RectTransform>();

            // Set anchors manually using individual values instead of Vector2
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(0, 0);
            rect.sizeDelta = new Vector2(0, 0);

            // Add background image
            var bgImage = RootObject.AddComponent<Image>();
            bgImage.type = Image.Type.Sliced;
            bgImage.color = Theme.PanelBackground;

            // Activate the UI
            RootObject.SetActive(true);
        }

        // Separate methods to avoid lambda expressions which might cause issues
        private void OnScaleValueChanged(float value)
        {
            // Round to the nearest step
            float steppedValue = RoundToStepSize(value);

            // Only update if the value has actually changed to avoid infinite loops
            if (Math.Abs(ScaleSlider.value - steppedValue) > 0.001f)
            {
                ScaleSlider.value = steppedValue;
            }

            if (ScaleChanged != null)
                ScaleChanged(value);
        }

        private void OnCloseButtonClicked()
        {
            if (RootObject != null)
                RootObject.SetActive(false);
        }

        public void Update()
        {
            // Check if the root object has moved
            if (RootRect != null && FloatingControlRect != null)
            {
                Vector2 currentPosition = RootRect.anchoredPosition;

                // Only update if position has changed
                if (currentPosition != _lastRootPosition)
                {
                    _lastRootPosition = currentPosition;
                    UpdateFloatingPanelPosition();
                }
            }
        }
    }
}
