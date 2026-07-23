using UnityEngine;
using TMPro;

namespace MultiLanguageSupporter
{
    [RequireComponent(typeof(TMP_Text))]
    [AddComponentMenu("Multi-Language/Multi-Language Text")]
    [ExecuteAlways]
    public class MultiLanguageText : MonoBehaviour
    {
        [Tooltip("Optional database override. If null, the default database in Resources will be used.")]
        [SerializeField]
        private FontDatabase databaseOverride;

        [Tooltip("The text content to display. This will automatically update the TextMesh Pro component with the correct font.")]
        [SerializeField]
        [TextArea(3, 10)]
        private string textContent = "";

        private TMP_Text textComponent;
        private SmartFontPreprocessor preprocessor;

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

            if (textComponent != null && (textComponent.textPreprocessor == null || !(textComponent.textPreprocessor is SmartFontPreprocessor)))
            {
                if (preprocessor == null)
                {
                    preprocessor = new SmartFontPreprocessor();
                }
                textComponent.textPreprocessor = preprocessor;
            }
        }

        public void ApplyTextAndResolve()
        {
            if (textComponent == null)
            {
                textComponent = GetComponent<TMP_Text>();
            }

            InitializePreprocessor();

            if (textComponent != null)
            {
                textComponent.text = textContent;
                textComponent.FixFont(databaseOverride);
            }
        }
    }
}
