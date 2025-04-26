using System;
using TMPro;
using UIBuddy.Classes;
using UIBuddy.UI.Classes;
using UnityEngine;
using UnityEngine.UI;

namespace UIBuddy.UI
{
    internal static class UIFactory
    {
        private static readonly Vector2 SmallElementSize = new(25, 25);
        private static readonly Vector2 OutlineDistance = new(2, 2);
        public static TMP_FontAsset Font { get; set; }
        public static Material FontMaterial { get; set; }

        public static GameObject CreateUIObject(string name, GameObject parent, Vector2 sizeDelta = default)
        {
            GameObject obj = new(name)
            {
                layer = 5,
                hideFlags = HideFlags.HideAndDontSave,
            };

            if (parent)
            {
                obj.transform.SetParent(parent.transform, false);
            }

            var rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = sizeDelta;
            return obj;
        }

        public static LabelRef CreateLabel(GameObject parent, string name, string defaultText, TextAlignmentOptions alignment = TextAlignmentOptions.Center,
            Color? color = null, int fontSize = 14)
        {
            var obj = CreateUIObject(name, parent);
            var textComp = obj.AddComponent<TextMeshProUGUI>();


            textComp.color = color ?? Theme.DefaultText;
            textComp.font = Font;
            //textComp.fontMaterial = FontMaterial;

            textComp.text = defaultText;
            textComp.alignment = alignment;
            textComp.fontSize = fontSize;

            try
            {
                //textComp.outlineWidth = 0.15f;
                //textComp.outlineColor = Color.black;
            }
            catch (Exception)
            {
                // This can throw if the mod is attempting to run this when exiting the application.
            }

            return new LabelRef
            {
                GameObject = obj,
                TextMesh = textComp
            };
        }

        public static InputFieldRef CreateInputField(GameObject parent, string name, string placeHolderText, int charLimit = 255)
        {
            GameObject mainObj = CreateUIObject(name, parent);

            Image mainImage = mainObj.AddComponent<Image>();
            mainImage.type = Image.Type.Sliced;
            mainImage.color = Theme.DarkBackground;

            TMP_InputField inputField = mainObj.AddComponent<TMP_InputField>();
            Navigation nav = inputField.navigation;
            nav.mode = Navigation.Mode.None;
            inputField.navigation = nav;
            inputField.lineType = TMP_InputField.LineType.SingleLine;
            inputField.interactable = true;
            inputField.transition = Selectable.Transition.ColorTint;
            inputField.targetGraphic = mainImage;

            var colourBlock = new ColorBlock()
            {
                normalColor = Theme.InputFieldNormal,
                highlightedColor = Theme.InputFieldHighlighted,
                pressedColor = Theme.InputFieldPressed,
                colorMultiplier = 1
            };
            inputField.colors = colourBlock;

            GameObject textArea = CreateUIObject("TextArea", mainObj);
            textArea.AddComponent<RectMask2D>();

            RectTransform textAreaRect = textArea.GetComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = Vector2.zero;
            textAreaRect.offsetMax = Vector2.zero;

            GameObject placeHolderObj = CreateUIObject("Placeholder", textArea);
            TextMeshProUGUI placeholderText = placeHolderObj.AddComponent<TextMeshProUGUI>();
            SetDefaultTextValues(placeholderText);
            placeholderText.text = placeHolderText ?? "...";
            placeholderText.color = Theme.PlaceHolderText;
            placeholderText.enableWordWrapping = true;
            placeholderText.alignment = TextAlignmentOptions.MidlineLeft;
            placeholderText.fontSize = 14;

            RectTransform placeHolderRect = placeHolderObj.GetComponent<RectTransform>();
            placeHolderRect.anchorMin = Vector2.zero;
            placeHolderRect.anchorMax = Vector2.one;
            placeHolderRect.offsetMin = Vector2.zero;
            placeHolderRect.offsetMax = Vector2.zero;

            inputField.placeholder = placeholderText;

            GameObject inputTextObj = CreateUIObject("Text", textArea);
            TextMeshProUGUI inputText = inputTextObj.AddComponent<TextMeshProUGUI>();
            SetDefaultTextValues(inputText);
            inputText.text = "";
            inputText.color = Theme.DefaultText;
            inputText.enableWordWrapping = true;
            inputText.alignment = TextAlignmentOptions.MidlineLeft;
            inputText.fontSize = 14;

            RectTransform inputTextRect = inputTextObj.GetComponent<RectTransform>();
            inputTextRect.anchorMin = Vector2.zero;
            inputTextRect.anchorMax = Vector2.one;
            inputTextRect.offsetMin = Vector2.zero;
            inputTextRect.offsetMax = Vector2.zero;

            inputField.textComponent = inputText;
            inputField.characterLimit = charLimit;

            return new InputFieldRef(inputField);
        }

        public static LayoutElement SetLayoutElement(GameObject gameObject, int? minWidth = null, int? minHeight = null,
            int? flexibleWidth = null, int? flexibleHeight = null, int? preferredWidth = null,
            int? preferredHeight = null,
            bool? ignoreLayout = null)
        {
            var layout = gameObject.GetComponent<LayoutElement>();
            if (!layout)
                layout = gameObject.AddComponent<LayoutElement>();

            if (minWidth != null)
                layout.minWidth = (int)minWidth;

            if (minHeight != null)
                layout.minHeight = (int)minHeight;

            if (flexibleWidth != null)
                layout.flexibleWidth = (int)flexibleWidth;

            if (flexibleHeight != null)
                layout.flexibleHeight = (int)flexibleHeight;

            if (preferredWidth != null)
                layout.preferredWidth = (int)preferredWidth;

            if (preferredHeight != null)
                layout.preferredHeight = (int)preferredHeight;

            if (ignoreLayout != null)
                layout.ignoreLayout = (bool)ignoreLayout;

            return layout;
        }

        /// <summary>
        /// Get and/or Add a HorizontalOrVerticalLayoutGroup (must pick one) to the GameObject, and set the values on it.
        /// </summary>
        public static T SetLayoutGroup<T>(GameObject gameObject, bool? forceWidth = null, bool? forceHeight = null,
            bool? childControlWidth = null, bool? childControlHeight = null, int? spacing = null, int? padTop = null,
            int? padBottom = null, int? padLeft = null, int? padRight = null, TextAnchor? childAlignment = null)
            where T : HorizontalOrVerticalLayoutGroup
        {
            var group = gameObject.GetComponent<T>();
            if (!group) group = gameObject.AddComponent<T>();

            return SetLayoutGroup(group, forceWidth, forceHeight, childControlWidth, childControlHeight, spacing, padTop,
                padBottom, padLeft, padRight, childAlignment);
        }

        /// <summary>
        /// Set the values on a HorizontalOrVerticalLayoutGroup.
        /// </summary>
        public static T SetLayoutGroup<T>(T group, bool? forceWidth = null, bool? forceHeight = null,
            bool? childControlWidth = null, bool? childControlHeight = null, int? spacing = null, int? padTop = null,
            int? padBottom = null, int? padLeft = null, int? padRight = null, TextAnchor? childAlignment = null)
            where T : HorizontalOrVerticalLayoutGroup
        {
            if (forceWidth != null)
                group.childForceExpandWidth = (bool)forceWidth;
            if (forceHeight != null)
                group.childForceExpandHeight = (bool)forceHeight;
            if (childControlWidth != null)
                group.childControlWidth = (bool)childControlWidth;
            if (childControlHeight != null)
                group.childControlHeight = (bool)childControlHeight;
            if (spacing != null)
                group.spacing = (int)spacing;
            if (padTop != null)
                group.padding.top = (int)padTop;
            if (padBottom != null)
                group.padding.bottom = (int)padBottom;
            if (padLeft != null)
                group.padding.left = (int)padLeft;
            if (padRight != null)
                group.padding.right = (int)padRight;
            if (childAlignment != null)
                group.childAlignment = (TextAnchor)childAlignment;

            return group;
        }

        /// <summary>
        /// Create a VerticalLayoutGroup object with an Image component. Use SetLayoutGroup to create one without an image.
        /// </summary>
        public static GameObject CreateVerticalGroup(GameObject parent, string name, bool forceWidth, bool forceHeight,
            bool childControlWidth, bool childControlHeight, int spacing = 0, Vector4 padding = default, Color? bgColor = null,
            TextAnchor? childAlignment = null)
        {
            var groupObj = CreateUIObject(name, parent);

            SetLayoutGroup<VerticalLayoutGroup>(groupObj, forceWidth, forceHeight, childControlWidth, childControlHeight,
                spacing, (int)padding.x, (int)padding.y, (int)padding.z, (int)padding.w, childAlignment);

            if (bgColor != null)
                groupObj.AddComponent<Image>().color = bgColor.Value;

            return groupObj;
        }

        /// <summary>
        /// Create a HorizontalLayoutGroup object with an Image component. Use SetLayoutGroup to create one without an image.
        /// </summary>
        public static GameObject CreateHorizontalGroup(GameObject parent, string name, bool forceExpandWidth, bool forceExpandHeight,
            bool childControlWidth, bool childControlHeight, int spacing = 0, Vector4 padding = default, Color? bgColor = null,
            TextAnchor? childAlignment = null)
        {
            var groupObj = CreateUIObject(name, parent);

            SetLayoutGroup<HorizontalLayoutGroup>(groupObj, forceExpandWidth, forceExpandHeight, childControlWidth, childControlHeight,
                spacing, (int)padding.x, (int)padding.y, (int)padding.z, (int)padding.w, childAlignment);

            if(bgColor != null)
                groupObj.AddComponent<Image>().color = bgColor.Value;

            return groupObj;
        }

        public static GameObject CreateSlider(GameObject parent, string name, out Slider slider)
        {
            var sliderObj = CreateUIObject(name, parent, SmallElementSize);

            var bgObj = CreateUIObject("Background", sliderObj);
            var fillAreaObj = CreateUIObject("Fill Area", sliderObj);
            var fillObj = CreateUIObject("Fill", fillAreaObj);
            var handleSlideAreaObj = CreateUIObject("Handle Slide Area", sliderObj);
            var handleObj = CreateUIObject("Handle", handleSlideAreaObj);

            var bgImage = bgObj.AddComponent<Image>();
            bgImage.type = Image.Type.Sliced;
            bgImage.color = Theme.PanelBackground;

            var bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 0.25f);
            bgRect.anchorMax = new Vector2(1f, 0.75f);
            bgRect.sizeDelta = new Vector2(0f, 0f);

            var fillAreaRect = fillAreaObj.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRect.anchoredPosition = new Vector2(-5f, 0f);
            fillAreaRect.sizeDelta = new Vector2(-20f, 0f);

            var fillImage = fillObj.AddComponent<Image>();
            fillImage.type = Image.Type.Sliced;
            fillImage.color = Theme.SliderFill;

            fillObj.GetComponent<RectTransform>().sizeDelta = new Vector2(10f, 0f);

            var handleSlideRect = handleSlideAreaObj.GetComponent<RectTransform>();
            handleSlideRect.sizeDelta = new Vector2(-20f, 0f);
            handleSlideRect.anchorMin = new Vector2(0f, 0f);
            handleSlideRect.anchorMax = new Vector2(1f, 1f);

            var handleImage = handleObj.AddComponent<Image>();
            handleImage.color = Theme.SliderHandle;

            var outline = handleObj.AddComponent<Outline>();
            outline.effectColor = Theme.DarkBackground;
            outline.effectDistance = OutlineDistance;

            handleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(20f, 0f);

            slider = sliderObj.AddComponent<Slider>();
            slider.fillRect = fillObj.GetComponent<RectTransform>();
            slider.handleRect = handleObj.GetComponent<RectTransform>();
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;

            var colorBlock = new ColorBlock
            {
                normalColor = Theme.SliderNormal,
                highlightedColor = Theme.SliderHighlighted,
                pressedColor = Theme.SliderPressed,
                colorMultiplier = 1
            };
            slider.colors = colorBlock;

            return sliderObj;
        }

        public static ButtonRef CreateButton(GameObject parent, string name, string text, Color? normalColor = null)
        {
            var baseColor = normalColor ?? Theme.SliderFill;
            var colorBlock = new ColorBlock()
            {
                normalColor = baseColor,
                highlightedColor = (baseColor * 1.2f),
                selectedColor = (baseColor * 1.1f),
                pressedColor = (baseColor * 0.7f),
                disabledColor = (baseColor * 0.4f),
                colorMultiplier = 1
            };

            var buttonRef = CreateButton(parent, name, text, default(ColorBlock));
            buttonRef.Component.colors = colorBlock;

            return buttonRef;
        }

        public static ButtonRef CreateButton(GameObject parent, string name, string text, ColorBlock colors)
        {
            var buttonObj = CreateUIObject(name, parent, SmallElementSize);

            var textObj = CreateUIObject("Text", buttonObj);

            // Setting the background to white, so that the color block can tint it correctly
            var image = buttonObj.AddComponent<Image>();
            image.type = Image.Type.Sliced;
            image.color = Theme.White;

            var outline = buttonObj.AddComponent<Outline>();
            outline.effectColor = Theme.DarkBackground;
            outline.effectDistance = OutlineDistance;

            var button = buttonObj.AddComponent<Button>();
            SetDefaultSelectableValues(button);

            colors.colorMultiplier = 1;
            button.colors = colors;

            var textComp = textObj.AddComponent<TextMeshProUGUI>();
            textComp.text = text;
            SetDefaultTextValues(textComp);
            textComp.alignment = TextAlignmentOptions.Center;

            var rect = textObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            SetButtonDeselectListener(button);

            return new ButtonRef(button);
        }

        private static void SetDefaultSelectableValues(Selectable selectable)
        {
            var nav = selectable.navigation;
            nav.mode = Navigation.Mode.Explicit;
            selectable.navigation = nav;

            var colorBlock = new ColorBlock()
            {
                normalColor = Theme.SelectableNormal,
                highlightedColor = Theme.SelectableHighlighted,
                pressedColor = Theme.SelectablePressed,
                colorMultiplier = 1
            };

            selectable.colors = colorBlock;
        }

        private static void SetDefaultTextValues(TextMeshProUGUI text)
        {
            text.color = Theme.DefaultText;
            text.font = Font;
            text.fontSize = 14;
        }

        private static void SetButtonDeselectListener(Button button)
        {
            button.onClick.AddListener(() =>
            {
                button.OnDeselect(null);
            });
        }
    }
}
