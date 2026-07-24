using UnityEngine;
using TMPro;

namespace MultiLanguageSupporter
{
    public static class SmartFontExtensions
    {
        public static void FixFont(this TMP_Text textComponent, FontDatabase database = null, string originalText = null)
        {
            FontResolver.ResolveAndApply(textComponent, database, originalText);
        }
    }

    [RequireComponent(typeof(TMP_Text))]
    [AddComponentMenu("SmartFont/Smart Font Applier")]
    [ExecuteAlways]
    [DefaultExecutionOrder(-100)]
    public class SmartFontApplier : MonoBehaviour, ITextPreprocessor
    {
        private FontDatabase databaseOverride = null;

        [Tooltip("Should the font be resolved automatically on Start?")]
        [SerializeField]
        private bool resolveOnStart = true;

        [Tooltip("Should the font be resolved dynamically when the text is modified?")]
        [SerializeField]
        private bool observeTextChanges = true;

        private TMP_Text textComponent;
        private string lastText;
        private string lastRawText;
        private bool ownsPreprocessor = false;

        private void Awake()
        {
            textComponent = GetComponent<TMP_Text>();
            InitializePreprocessor();
        }

        private void OnEnable()
        {
            InitializePreprocessor();
            if (resolveOnStart)
            {
                Resolve();
            }
        }

        private void Start()
        {
            InitializePreprocessor();
            if (resolveOnStart)
            {
                Resolve();
            }
        }

        private void OnValidate()
        {
            InitializePreprocessor();
            if (resolveOnStart)
            {
                Resolve();
            }
        }

        private void Update()
        {
            if (observeTextChanges && textComponent != null)
            {
                string currentText = textComponent.text;
                if (currentText != lastText)
                {
                    Resolve();
                    lastText = currentText;
                }
            }
        }

        private static bool IsAlreadyShaped(string text)
        {
            return !string.IsNullOrEmpty(text) && text.Contains("<font=");
        }

        private void InitializePreprocessor()
        {
            if (textComponent == null)
            {
                textComponent = GetComponent<TMP_Text>();
            }

            if (textComponent != null)
            {
                if (textComponent.textPreprocessor != null && textComponent.textPreprocessor != this)
                {
                    Debug.LogWarning("[SmartFontApplier] Another TMP text preprocessor is already assigned on this GameObject. SmartFontApplier will not override it to avoid double preprocessing.");
                    return;
                }

                if (textComponent.textPreprocessor != this)
                {
                    textComponent.textPreprocessor = this;
                    ownsPreprocessor = true;
                }
            }
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

        private static string GetVisibleTextOutsideFontBlocks(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var sb = new System.Text.StringBuilder();
            bool insideFontBlock = false;

            for (int i = 0; i < text.Length; i++)
            {
                if (!insideFontBlock && text[i] == '<' && i + 5 < text.Length && text.Substring(i, 5).ToLowerInvariant() == "<font")
                {
                    int close = text.IndexOf('>', i);
                    if (close == -1) break;
                    insideFontBlock = true;
                    i = close;
                    continue;
                }

                if (insideFontBlock)
                {
                    if (text[i] == '<' && i + 6 < text.Length && text.Substring(i, 7).ToLowerInvariant() == "</font>")
                    {
                        int close = text.IndexOf('>', i);
                        if (close == -1) break;
                        insideFontBlock = false;
                        i = close;
                    }
                    continue;
                }

                sb.Append(text[i]);
            }

            return sb.ToString();
        }

        public void Resolve()
        {
            if (textComponent == null)
            {
                textComponent = GetComponent<TMP_Text>();
            }

            if (textComponent != null)
            {
                string currentText = textComponent.text;
                bool alreadyShaped = IsAlreadyShaped(currentText);
                string originalText = alreadyShaped ? (!string.IsNullOrEmpty(lastRawText) ? lastRawText : GetVisibleTextOutsideFontBlocks(currentText)) : currentText;
                string processedText = currentText;

                if (!alreadyShaped && textComponent.textPreprocessor != null)
                {
                    processedText = textComponent.textPreprocessor.PreprocessText(currentText);
                }

                textComponent.text = processedText;
                textComponent.FixFont(databaseOverride, originalText);
                lastText = textComponent.text;

                if (!alreadyShaped)
                {
                    lastRawText = currentText;
                }
            }
        }

        public string PreprocessText(string text)
        {
            string shaped = ScriptShaper.Shape(text);
#if UNITY_EDITOR
            if (Debug.isDebugBuild)
            {
                Debug.Log($"[SmartFontApplier] PreprocessText: '{text}' -> '{shaped}'");
            }
#endif
            return shaped;
        }
    }
}
