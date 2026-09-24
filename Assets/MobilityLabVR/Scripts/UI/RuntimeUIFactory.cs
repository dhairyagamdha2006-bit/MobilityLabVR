using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace MobilityLabVR
{
    public static class RuntimeUIFactory
    {
        private static Font cachedFont;

        public static Font Font
        {
            get
            {
                if (cachedFont == null) cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return cachedFont;
            }
        }

        public static Canvas CreateCanvas(string name)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = gameObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            EnsureEventSystem();
            return canvas;
        }

        public static GameObject CreatePanel(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            panel.GetComponent<Image>().color = color;
            SetRect(panel.GetComponent<RectTransform>(), anchorMin, anchorMax, offsetMin, offsetMax);
            return panel;
        }

        public static Text CreateText(Transform parent, string name, string content, int size, Color color,
            TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.GetComponent<Text>();
            text.font = Font;
            text.text = content;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            SetRect(text.rectTransform, anchorMin, anchorMax, offsetMin, offsetMax);
            return text;
        }

        public static Button CreateButton(Transform parent, string name, string label, Color normalColor, Color textColor,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.GetComponent<Image>();
            image.color = normalColor;
            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = Color.Lerp(normalColor, Color.white, 0.18f);
            colors.pressedColor = Color.Lerp(normalColor, Color.black, 0.16f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;
            SetRect(buttonObject.GetComponent<RectTransform>(), anchorMin, anchorMax, offsetMin, offsetMax);
            CreateText(buttonObject.transform, "Label", label, 24, textColor, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, new Vector2(12f, 4f), new Vector2(-12f, -4f));
            return button;
        }

        public static InputField CreateInputField(Transform parent, string name, string placeholderText,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject inputObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
            inputObject.transform.SetParent(parent, false);
            Image image = inputObject.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.09f);
            SetRect(inputObject.GetComponent<RectTransform>(), anchorMin, anchorMax, offsetMin, offsetMax);

            Text text = CreateText(inputObject.transform, "Text", string.Empty, 23, MobilityLabPalette.OffWhite,
                TextAnchor.MiddleLeft, Vector2.zero, Vector2.one, new Vector2(18f, 4f), new Vector2(-18f, -4f));
            Text placeholder = CreateText(inputObject.transform, "Placeholder", placeholderText, 21,
                new Color(0.75f, 0.8f, 0.82f, 0.65f), TextAnchor.MiddleLeft,
                Vector2.zero, Vector2.one, new Vector2(18f, 4f), new Vector2(-18f, -4f));
            InputField input = inputObject.GetComponent<InputField>();
            input.textComponent = text;
            input.placeholder = placeholder;
            input.characterLimit = 32;
            input.contentType = InputField.ContentType.Alphanumeric;
            return input;
        }

        public static Slider CreateSlider(Transform parent, string name, float value, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject sliderObject = new GameObject(name, typeof(RectTransform), typeof(Slider));
            sliderObject.transform.SetParent(parent, false);
            SetRect(sliderObject.GetComponent<RectTransform>(), anchorMin, anchorMax, offsetMin, offsetMax);

            GameObject background = CreatePanel(sliderObject.transform, "Background", new Color(1f, 1f, 1f, 0.14f),
                new Vector2(0f, 0.35f), new Vector2(1f, 0.65f), Vector2.zero, Vector2.zero);
            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObject.transform, false);
            SetRect(fillArea.GetComponent<RectTransform>(), new Vector2(0f, 0.35f), new Vector2(1f, 0.65f), Vector2.zero, Vector2.zero);
            GameObject fill = CreatePanel(fillArea.transform, "Fill", MobilityLabPalette.Teal,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderObject.transform, false);
            SetRect(handleArea.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(10f, 0f), new Vector2(-10f, 0f));
            GameObject handle = CreatePanel(handleArea.transform, "Handle", MobilityLabPalette.OffWhite,
                new Vector2(0f, 0.18f), new Vector2(0f, 0.82f), new Vector2(-9f, 0f), new Vector2(9f, 0f));

            Slider slider = sliderObject.GetComponent<Slider>();
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = Mathf.Clamp01(value);
            _ = background;
            return slider;
        }

        public static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null) return;
            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Object.DontDestroyOnLoad(eventSystemObject);
        }
    }
}
