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

        public void ApplyTextAndResolve()
        {
            if (textComponent == null)
            {
                textComponent = GetComponent<TMP_Text>();
            }

            if (textComponent != null)
            {
                textComponent.text = textContent;
                textComponent.FixFont(databaseOverride, textContent);
            }
        }

        public string PreprocessText(string text)
        {
            string shaped = ScriptShaper.Shape(text);
            Debug.Log($"[MultiLanguageText] PreprocessText: '{text}' -> '{shaped}'");
            return shaped;
        }
    }
}
