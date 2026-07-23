using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace MultiLanguageSupporter
{
    public static class FontResolver
    {
        private static FontDatabase defaultDatabase;

        public static void SetDefaultDatabase(FontDatabase database)
        {
            defaultDatabase = database;
        }

        public static FontDatabase GetDefaultDatabase()
        {
            if (defaultDatabase == null)
            {
                // Try loading from Resources
                defaultDatabase = Resources.Load<FontDatabase>("SmartFontDefaultDatabase");
            }
            return defaultDatabase;
        }

        public static void ResolveAndApply(TMP_Text textComponent, FontDatabase database = null, string originalText = null)
        {
            if (textComponent == null) return;

            if (database == null)
            {
                database = GetDefaultDatabase();
            }

            if (database == null)
            {
                Debug.LogWarning("[SmartFont] No FontDatabase provided and no default database found in Resources.");
                return;
            }

            string text = !string.IsNullOrEmpty(originalText) ? originalText : textComponent.text;
            if (string.IsNullOrEmpty(text)) return;

            // Detect dominant script
            ScriptType dominant = ScriptDetector.DetectDominantScript(text);
            TMP_FontAsset primaryFont = database.GetFontForScript(dominant);

            if (primaryFont != null)
            {
                textComponent.font = primaryFont;
            }
        }
    }
}
