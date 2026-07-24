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

            ScriptType dominant = ScriptDetector.DetectDominantScript(text);
            TMP_FontAsset primaryFont = database.GetFontForScript(dominant);

            if (dominant != ScriptType.Latin && ContainsLatin(text))
            {
                TMP_FontAsset latinFont = database.GetFontForScript(ScriptType.Latin);
                if (latinFont != null)
                {
                    primaryFont = latinFont;
                }
            }

            if (primaryFont == null)
            {
                primaryFont = database.GetFontForScript(ScriptType.Latin);
            }

            if (primaryFont != null)
            {
                textComponent.font = primaryFont;
            }
        }

        private static bool ContainsLatin(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;

            bool insideFontBlock = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '<' && i < text.Length - 1)
                {
                    int closeIdx = text.IndexOf('>', i);
                    if (closeIdx != -1)
                    {
                        string tag = text.Substring(i, closeIdx - i + 1).ToLowerInvariant();
                        if (tag.StartsWith("<font"))
                        {
                            insideFontBlock = true;
                        }
                        else if (tag.StartsWith("</font"))
                        {
                            insideFontBlock = false;
                        }
                        i = closeIdx;
                        continue;
                    }
                }

                if (insideFontBlock)
                {
                    continue;
                }

                if (ScriptDetector.GetCharScriptType(c) == ScriptType.Latin)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
