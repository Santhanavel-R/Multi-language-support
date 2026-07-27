using System;
using System.Text;
using System.Collections.Generic;
using TMPro;

namespace MultiLanguageSupporter
{
    public static class ScriptShaper
    {
        public static string Shape(string input)
        {
            return Shape(input, null);
        }

        public static string Shape(string input, FontDatabase database)
        {
            if (string.IsNullOrEmpty(input)) return input;

            if (database == null)
            {
                database = FontResolver.GetDefaultDatabase();
            }

            string tamilFontName = "SaiSaiSDF";
            string hindiFontName = "KrutiDev010SDF";
            string latinFontName = "NotoSansRegularSDF";

            if (database != null)
            {
                var tamilFont = database.GetFontForScript(ScriptType.Tamil);
                if (IsFontAssetHealthy(tamilFont)) tamilFontName = tamilFont.name;

                var hindiFont = database.GetFontForScript(ScriptType.Hindi);
                if (IsFontAssetHealthy(hindiFont)) hindiFontName = hindiFont.name;

                var latinFont = database.GetFontForScript(ScriptType.Latin);
                if (IsFontAssetHealthy(latinFont)) latinFontName = latinFont.name;
            }

            return SegmentAndTagText(input, tamilFontName, hindiFontName, latinFontName);
        }

        private static string SegmentAndTagText(string input, string tamilFont, string hindiFont, string latinFont)
        {
            if (string.IsNullOrEmpty(input)) return input;

            StringBuilder sb = new StringBuilder();
            int i = 0;
            int len = input.Length;

            ScriptType currentScript = ScriptType.Unknown;
            StringBuilder runBuffer = new StringBuilder();

            while (i < len)
            {
                // Parse TMPro Rich Text tags to avoid processing them
                if (input[i] == '<')
                {
                    int closeIdx = input.IndexOf('>', i);
                    if (closeIdx != -1)
                    {
                        // Flush active run before appending tag
                        FlushRun(sb, runBuffer, currentScript, tamilFont, hindiFont, latinFont);
                        currentScript = ScriptType.Unknown;

                        string tag = input.Substring(i, closeIdx - i + 1);
                        sb.Append(tag);
                        i = closeIdx + 1;
                        continue;
                    }
                }

                // Retrieve UTF-32 Code Point (resolving surrogate pairs correctly)
                int codePoint = char.ConvertToUtf32(input, i);
                int charCount = char.IsSurrogatePair(input, i) ? 2 : 1;
                string character = input.Substring(i, charCount);

                ScriptType charScript = GetCodePointScriptType(codePoint);

                if (charScript == ScriptType.Unknown)
                {
                    // Neutral characters (spaces, punctuation) inherit current run's script
                    runBuffer.Append(character);
                }
                else
                {
                    if (currentScript == ScriptType.Unknown)
                    {
                        currentScript = charScript;
                        runBuffer.Append(character);
                    }
                    else if (charScript == currentScript)
                    {
                        runBuffer.Append(character);
                    }
                    else
                    {
                        // Script switched, flush previous run
                        FlushRun(sb, runBuffer, currentScript, tamilFont, hindiFont, latinFont);
                        currentScript = charScript;
                        runBuffer.Append(character);
                    }
                }

                i += charCount;
            }

            // Flush remaining run
            FlushRun(sb, runBuffer, currentScript, tamilFont, hindiFont, latinFont);

            return sb.ToString();
        }

        private static void FlushRun(StringBuilder sb, StringBuilder runBuffer, ScriptType script, string tamilFont, string hindiFont, string latinFont)
        {
            if (runBuffer.Length == 0) return;

            string text = runBuffer.ToString();
            runBuffer.Clear();

            // Do not wrap pure whitespace/punctuation runs
            if (script == ScriptType.Unknown)
            {
                sb.Append(text);
                return;
            }

            string fontName = latinFont;
            if (script == ScriptType.Tamil) fontName = tamilFont;
            else if (script == ScriptType.Hindi) fontName = hindiFont;

            sb.Append("<font=\"").Append(fontName).Append("\">").Append(text).Append("</font>");
        }

        private static ScriptType GetCodePointScriptType(int codePoint)
        {
            if (codePoint >= 0x0B80 && codePoint <= 0x0BFF) return ScriptType.Tamil;
            if (codePoint >= 0x0900 && codePoint <= 0x097F) return ScriptType.Hindi;
            if (codePoint >= 0x0980 && codePoint <= 0x09FF) return ScriptType.Bengali;
            if (codePoint >= 0x0C80 && codePoint <= 0x0CFF) return ScriptType.Kannada;
            if (codePoint >= 0x0D00 && codePoint <= 0x0D7F) return ScriptType.Malayalam;
            if (codePoint >= 0x0E00 && codePoint <= 0x0E7F) return ScriptType.Thai;
            
            if ((codePoint >= 0xAC00 && codePoint <= 0xD7AF) ||
                (codePoint >= 0x1100 && codePoint <= 0x11FF) ||
                (codePoint >= 0x3130 && codePoint <= 0x318F))
                return ScriptType.Korean;

            if ((codePoint >= 0x4E00 && codePoint <= 0x9FFF) ||
                (codePoint >= 0x3400 && codePoint <= 0x4DBF))
                return ScriptType.Chinese;

            // Basic Latin ranges
            if ((codePoint >= 0x0041 && codePoint <= 0x005A) || // A-Z
                (codePoint >= 0x0061 && codePoint <= 0x007A))   // a-z
                return ScriptType.Latin;

            return ScriptType.Unknown;
        }

        private static bool IsFontAssetHealthy(TMP_FontAsset font)
        {
            return font != null && 
                   font.atlasTextures != null && 
                   font.atlasTextures.Length > 0 && 
                   font.atlasTextures[0] != null && 
                   font.material != null;
        }
    }
}
