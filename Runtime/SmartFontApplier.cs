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

        public void Resolve()
        {
            if (textComponent == null)
            {
                textComponent = GetComponent<TMP_Text>();
            }

            if (textComponent != null)
            {
                string processedText = textComponent.textPreprocessor != null
                    ? textComponent.textPreprocessor.PreprocessText(textComponent.text)
                    : textComponent.text;

                textComponent.text = processedText;
                textComponent.FixFont(databaseOverride);
                lastText = textComponent.text;
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
