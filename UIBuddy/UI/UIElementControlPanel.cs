using System;
using UIBuddy.Classes;
using UnityEngine;
using UnityEngine.UI;

namespace UIBuddy.UI
{
    public class UIElementControlPanel
    {
        private GameObject RootObject { get; }
        public GameObject TitleBar { get; private set; }

        public Action<float> ScaleChanged { get; set; }

        public UIElementControlPanel(GameObject parent, float scaleFactor)
        {
            RootObject = UIFactory.CreateUIObject($"MarkPanel_{Guid.NewGuid()}", parent);
            ConstructUI(scaleFactor);
        }

        private void ConstructUI(float scaleFactor)
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

            // Create title bar
            TitleBar = new GameObject("TitleBar");
            TitleBar.transform.SetParent(RootObject.transform, false);

            var titleRect = TitleBar.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.sizeDelta = new Vector2(0, 25);
            titleRect.anchoredPosition = new Vector2(0, 0);

            // Add title bar background
            var titleBg = TitleBar.AddComponent<Image>();
            titleBg.type = Image.Type.Sliced;
            titleBg.color = Theme.SliderNormal;

            // Add horizontal layout
            var titleLayout = TitleBar.AddComponent<HorizontalLayoutGroup>();
            //titleLayout.padding = new RectOffset(4, 4, 2, 2);
            titleLayout.spacing = 4;
            titleLayout.childAlignment = TextAnchor.MiddleLeft;
            titleLayout.childControlWidth = true;
            titleLayout.childControlHeight = true;
            titleLayout.childForceExpandWidth = false;
            titleLayout.childForceExpandHeight = true;

            // Create scale slider directly without using UIFactory.CreateSlider
            var scaleSliderObj = new GameObject("ScaleSlider");
            scaleSliderObj.transform.SetParent(TitleBar.transform, false);

            var sliderLayoutElement = scaleSliderObj.AddComponent<LayoutElement>();
            sliderLayoutElement.minHeight = 20;
            sliderLayoutElement.minWidth = 100;
            sliderLayoutElement.preferredWidth = 100;
            sliderLayoutElement.flexibleWidth = 0;
            sliderLayoutElement.flexibleHeight = 0;

            // Use factory method for slider which should be safer
            var scaleSlider = UIFactory.CreateSlider(TitleBar, "ScaleSlider", out var slider);

            if (slider != null)
            {
                slider.value = scaleFactor;
                slider.minValue = 0.1f;
                slider.maxValue = 3.0f;

                slider.onValueChanged.AddListener(new Action<float>(value => ScaleChanged?.Invoke(value)));
            }

            // Create close button using the factory
            /*ar closeButton = UIFactory.CreateButton(TitleBar, "CloseButton", "X");
            if (closeButton != null && closeButton.Component != null)
            {
                var buttonLayoutElement = closeButton.Component.gameObject.AddComponent<LayoutElement>();
                buttonLayoutElement.minHeight = 20;
                buttonLayoutElement.minWidth = 25;
                buttonLayoutElement.flexibleWidth = 0;

                // Set close functionality
                closeButton.OnClick = OnCloseButtonClicked;
            }*/

            // Create content area
            var contentArea = new GameObject("ContentArea");
            contentArea.transform.SetParent(RootObject.transform, false);

            var contentRect = contentArea.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.offsetMin = new Vector2(0, 25);
            contentRect.offsetMax = new Vector2(0, 0);

            // Activate the UI
            RootObject.SetActive(true);
        }

        // Separate methods to avoid lambda expressions which might cause issues
        private void OnScaleValueChanged(float value)
        {
            if (ScaleChanged != null)
                ScaleChanged(value);
        }

        private void OnCloseButtonClicked()
        {
            if (RootObject != null)
                RootObject.SetActive(false);
        }

    }
}
