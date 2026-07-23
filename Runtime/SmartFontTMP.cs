using UnityEngine;
using TMPro;

namespace MultiLanguageSupporter
{
    public static class SmartFontExtensions
    {
        public static void FixFont(this TMP_Text textComponent, FontDatabase database = null)
        {
            FontResolver.ResolveAndApply(textComponent, database);
        }
    }

    [RequireComponent(typeof(TMP_Text))]
    [AddComponentMenu("SmartFont/Smart Font Applier")]
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

        private void Awake()
        {
            textComponent = GetComponent<TMP_Text>();
        }

        private void Start()
        {
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

        public void Resolve()
        {
            if (textComponent != null)
            {
                string originalText = textComponent.text;
                string shapedText = ScriptShaper.Shape(originalText);
                if (textComponent.text != shapedText)
                {
                    textComponent.text = shapedText;
                }
                textComponent.FixFont(databaseOverride);
                lastText = textComponent.text;
            }
        }
    }
}
