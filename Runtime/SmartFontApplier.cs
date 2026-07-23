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

    public class SmartFontPreprocessor : ITextPreprocessor
    {
        public string PreprocessText(string text)
        {
            return ScriptShaper.Shape(text);
        }
    }

    [RequireComponent(typeof(TMP_Text))]
    [AddComponentMenu("SmartFont/Smart Font Applier")]
    [ExecuteAlways]
    [DefaultExecutionOrder(-100)]
    public class SmartFontApplier : MonoBehaviour
    {
        [Tooltip("Optional database override. If null, the default database will be resolved.")]
        [SerializeField]
        private FontDatabase databaseOverride;

        [Tooltip("Should the font be resolved automatically on Start?")]
        [SerializeField]
        private bool resolveOnStart = true;

        [Tooltip("Should the font be resolved dynamically when the text is modified?")]
        [SerializeField]
        private bool observeTextChanges = true;

        private TMP_Text textComponent;
        private string lastText;
        private SmartFontPreprocessor preprocessor;

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

        private void Update()
        {
            if (textComponent == null)
            {
                textComponent = GetComponent<TMP_Text>();
            }

            InitializePreprocessor();

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
            if (textComponent != null && (textComponent.textPreprocessor == null || !(textComponent.textPreprocessor is SmartFontPreprocessor)))
            {
                if (preprocessor == null)
                {
                    preprocessor = new SmartFontPreprocessor();
                }
                textComponent.textPreprocessor = preprocessor;
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
                InitializePreprocessor();
                textComponent.FixFont(databaseOverride);
                lastText = textComponent.text;
            }
        }
    }
}
