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

        public static void ResolveAndApply(TMP_Text textComponent, FontDatabase database = null)
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

            string text = textComponent.text;
            if (string.IsNullOrEmpty(text)) return;

            // Detect dominant script
            ScriptType dominant = ScriptDetector.DetectDominantScript(text);
            TMP_FontAsset primaryFont = database.GetFontForScript(dominant);

            if (primaryFont != null)
            {
                textComponent.font = primaryFont;
            }

            // Setup fallbacks for mixed-language rendering
            SetupFallbackChains(dominant, database);
        }

        private static void SetupFallbackChains(ScriptType dominant, FontDatabase database)
        {
            TMP_FontAsset primaryFont = database.GetFontForScript(dominant);
            if (primaryFont == null) return;

            // Ensure fallback list is initialized
            if (primaryFont.fallbackFontAssetList == null)
            {
                primaryFont.fallbackFontAssetList = new List<TMP_FontAsset>();
            }

            // Go through all other supported scripts and add them to the fallback list if not present
            foreach (ScriptType script in System.Enum.GetValues(typeof(ScriptType)))
            {
                if (script == ScriptType.Unknown || script == dominant) continue;

                TMP_FontAsset fallbackFont = database.GetFontForScript(script);
                if (fallbackFont != null && fallbackFont != primaryFont)
                {
                    if (!primaryFont.fallbackFontAssetList.Contains(fallbackFont))
                    {
                        primaryFont.fallbackFontAssetList.Add(fallbackFont);
                    }
                }
            }
        }
    }
}
