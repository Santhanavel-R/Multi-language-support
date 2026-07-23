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

        /// <summary>
        /// Gets or sets the text content. Setting this will automatically update the TMP text and resolve the font.
        /// </summary>
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
        }

        private void OnEnable()
        {
            ApplyTextAndResolve();
        }

        private void OnValidate()
        {
            ApplyTextAndResolve();
        }

        /// <summary>
        /// Applies the text to the TMP component and resolves the correct font mapping.
        /// </summary>
        public void ApplyTextAndResolve()
        {
            if (textComponent == null)
            {
                textComponent = GetComponent<TMP_Text>();
            }

            if (textComponent != null)
            {
                string shapedText = ScriptShaper.Shape(textContent);
                textComponent.text = shapedText;
                textComponent.FixFont(databaseOverride);
            }
        }
    }
}
