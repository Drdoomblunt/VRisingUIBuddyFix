using UnityEngine;

namespace UIBuddy.UI
{
    internal static class Theme
    {
        private static float _opacity;
        public static Color PanelBackground { get; private set; }
        public static Color SliderFill { get; private set; }
        public static Color SliderHandle { get; private set; }
        public static Color DarkBackground { get; private set; }
        public static Color SliderNormal { get; private set; }
        public static Color SliderHighlighted { get; private set; }
        public static Color SliderPressed { get; private set; }
        public static Color SelectableNormal { get; private set; }
        public static Color SelectableHighlighted { get; private set; }
        public static Color SelectablePressed { get; private set; }
        public static Color White { get; private set; }
        public static Color DefaultText { get; private set; }
        public static Color PlaceHolderText { get; private set; }
        public static Color InputFieldNormal { get; private set; }
        public static Color InputFieldHighlighted { get; private set; }
        public static Color InputFieldPressed { get; private set; }
        public static Color ElementOutlineColor { get; private set; }

        public static float Opacity
        {
            get => _opacity;
            set
            {
                _opacity = value;
                UpdateColors();
            }
        }

        static Theme()
        {
            Opacity = 0.8f;
        }

        private static void UpdateColors()
        {
            PanelBackground = new Color(0.07f, 0.07f, 0.07f, Opacity);
            DarkBackground = new Color(0.07f, 0.07f, 0.07f, Opacity);
            SliderFill = new Color(0.3f, 0.3f, 0.3f, Opacity);
            SliderHandle = new Color(0.5f, 0.5f, 0.5f, Opacity);
            SliderNormal = new Color(0.4f, 0.4f, 0.4f, Opacity);
            SliderHighlighted = new Color(0.55f, 0.55f, 0.55f, Opacity);
            SliderPressed = new Color(0.3f, 0.3f, 0.3f, Opacity);
            SelectableNormal = new Color(0.2f, 0.2f, 0.2f, Opacity);
            SelectableHighlighted = new Color(0.3f, 0.3f, 0.3f, Opacity);
            SelectablePressed = new Color(0.15f, 0.15f, 0.15f, Opacity);
            DefaultText = Color.white;
            White = Color.white;
            PlaceHolderText = SliderHandle;

            InputFieldNormal = new Color(1f, 1f, 1f, Opacity);
            InputFieldHighlighted = new Color(0.95f, 0.95f, 0.95f, Opacity);
            InputFieldPressed = new Color(0.78f, 0.78f, 0.78f, Opacity);
            ElementOutlineColor = new Color(1f, 1f, 0f, Opacity);
        }
    }
}
