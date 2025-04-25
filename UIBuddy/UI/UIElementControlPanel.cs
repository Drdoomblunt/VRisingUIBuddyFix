using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIBuddy.UI
{
    public class UIElementControlPanel
    {
        private GameObject RootObject { get; }
        public GameObject TitleBar { get; private set; }

        public Action<float> ScaleChanged { get; set; }

        public UIElementControlPanel(GameObject parent)
        {
            RootObject = UIFactory.CreateUIObject($"MarkPanel_{Guid.NewGuid()}", parent);
            ConstructUI();
        }

        private void ConstructUI()
        {
            var rect = RootObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;

            var bgImage = RootObject.AddComponent<Image>();
            bgImage.type = Image.Type.Filled;
            bgImage.color = Theme.PanelBackground;

            var contentHolder = UIFactory.CreateUIObject("ContentHolder", RootObject);
            UIFactory.SetLayoutElement(contentHolder, 0, 0, flexibleWidth: 9999, flexibleHeight: 9999);

            // Title bar
            TitleBar = UIFactory.CreateHorizontalGroup(contentHolder, "TitleBar", false, true, true, true, 2,
                new Vector4(2, 2, 2, 2));
            UIFactory.SetLayoutElement(TitleBar, minHeight: 25, flexibleHeight: 0);

            var scaleSlider  = UIFactory.CreateSlider(TitleBar, "ScaleSlider", out var slider);
            UIFactory.SetLayoutElement(scaleSlider, minHeight: 25, flexibleHeight: 0, minWidth: 100, preferredWidth: 100, flexibleWidth: 0);
            UIFactory.SetLayoutGroup<HorizontalLayoutGroup>(scaleSlider, false, false, true, true, 3, childAlignment: TextAnchor.MiddleLeft);
            slider.value = 1.0f;
            slider.minValue = 0.1f;
            slider.maxValue = 3.0f;
            slider.onValueChanged.AddListener(ScaleChanged);

            var button = UIFactory.CreateButton(TitleBar, "CloseButton", "X");
            UIFactory.SetLayoutElement(button.Component.gameObject, minHeight: 25, minWidth: 25, flexibleWidth: 0);

            RootObject.SetActive(true);
        }

    }
}
