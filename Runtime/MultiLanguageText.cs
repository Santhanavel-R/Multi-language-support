using UnityEngine;
using TMPro;

namespace MultiLanguageSupporter
{
    [RequireComponent(typeof(TMP_Text))]
    [AddComponentMenu("Multi-Language/Multi-Language Text")]
    [ExecuteAlways]
    public class MultiLanguageText : MonoBehaviour, ITextPreprocessor
    {
        private FontDatabase databaseOverride = null;

        [Tooltip("The text content to display. This will automatically update the TextMesh Pro component with the correct font.")]
        [SerializeField]
        [TextArea(3, 10)]
        private string textContent = "";

        private TMP_Text textComponent;

        public string Text
        {
            get => textContent;
            set
            {
                if (textContent != value)
                {
                    textContent = value;
                    ApplyTextAndResolve();
                }
            }
        }

        private void Awake()
        {
            textComponent = GetComponent<TMP_Text>();
            InitializePreprocessor();
        }

        private void OnEnable()
        {
            InitializePreprocessor();
            ApplyTextAndResolve();
        }

        private void OnValidate()
        {
            InitializePreprocessor();
            ApplyTextAndResolve();
        }

        private void InitializePreprocessor()
        {
            if (textComponent == null)
            {
                textComponent = GetComponent<TMP_Text>();
            }

            if (textComponent != null && textComponent.textPreprocessor != this)
            {
                textComponent.textPreprocessor = this;
            }
        }

        private static bool IsAlreadyShaped(string text)
        {
            return !string.IsNullOrEmpty(text) && text.Contains("<font=");
        }

        private static string StripFontTags(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '<' && i + 5 < text.Length && text.Substring(i, 5).ToLowerInvariant() == "<font")
                {
                    int close = text.IndexOf('>', i);
                    if (close == -1) break;
                    i = close;
                    continue;
                }
                if (text[i] == '<' && i + 6 < text.Length && text.Substring(i, 7).ToLowerInvariant() == "</font>")
                {
                    int close = text.IndexOf('>', i);
                    if (close == -1) break;
                    i = close;
                    continue;
                }
                sb.Append(text[i]);
            }
            return sb.ToString();
        }

        public void ApplyTextAndResolve()
        {
            if (textComponent == null)
            {
                textComponent = GetComponent<TMP_Text>();
            }

            if (textComponent != null)
            {
                textComponent.richText = true;
                string processedText = textContent;
                if (textComponent.textPreprocessor != null && !IsAlreadyShaped(textContent))
                {
                    processedText = textComponent.textPreprocessor.PreprocessText(textContent);
                }

                textComponent.text = processedText;
                textComponent.FixFont(databaseOverride, textContent);
            }
        }

        public string PreprocessText(string text)
        {
            string shaped = ScriptShaper.Shape(text, databaseOverride);
#if UNITY_EDITOR
            if (Debug.isDebugBuild)
            {
                Debug.Log($"[MultiLanguageText] PreprocessText: '{text}' -> '{shaped}'");
            }
#endif
            return shaped;
        }
    }
}
