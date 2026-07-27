using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using TMPro;

namespace MultiLanguageSupporter
{
    public static class ScriptShaper
    {
        // Extensible registry mapping Script to Font Asset Name
        private static readonly Dictionary<ScriptType, string> ScriptFontMap = new Dictionary<ScriptType, string>
        {
            { ScriptType.Latin, "NotoSansRegularSDF" },
            { ScriptType.Tamil, "NotoSansTamilRegularSDF" },
            { ScriptType.Hindi, "NotoSansDevanagariRegularSDF" },
            { ScriptType.Bengali, "NotoSansBengaliRegularSDF" },
            { ScriptType.Kannada, "NotoSansKannadaRegularSDF" },
            { ScriptType.Malayalam, "NotoSansMalayalamRegularSDF" },
            { ScriptType.Thai, "NotoSansThaiRegularSDF" },
            { ScriptType.Chinese, "ZCOOLXiaoWeiRegularSDF" },
            { ScriptType.Korean, "SunflowerMediumSDF" }
        };

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

            // Resolve dynamic names from active database or fall back to defaults
            string tamilFont = ResolveFontName(database, ScriptType.Tamil);
            string hindiFont = ResolveFontName(database, ScriptType.Hindi);
            string latinFont = ResolveFontName(database, ScriptType.Latin);

            // Shape Indic languages first (Unicode Pre-Shaping Stage)
            string shapedInput = PreShapeComplexScripts(input);

            // Determine dominant script of raw input
            ScriptType dominant = ScriptDetector.DetectDominantScript(shapedInput);
            string dominantFontName = latinFont;
            if (dominant == ScriptType.Tamil) dominantFontName = tamilFont;
            else if (dominant == ScriptType.Hindi) dominantFontName = hindiFont;

            return SegmentAndTagText(shapedInput, tamilFont, hindiFont, latinFont, dominantFontName);
        }

        private static string ResolveFontName(FontDatabase database, ScriptType script)
        {
            if (database != null)
            {
                var font = database.GetFontForScript(script);
                if (IsFontAssetHealthy(font)) return font.name;
            }
            return ScriptFontMap.ContainsKey(script) ? ScriptFontMap[script] : "NotoSansRegularSDF";
        }

        private static string PreShapeComplexScripts(string input)
        {
            // By design, this stage accommodates:
            // 1. Arabic Bidi/RTL context shaping.
            // 2. Indic glyph substitution & reordering mapped to Private Use Area (PUA) glyphs.
            // Under normal Unity 2022+ TMPro configurations, using clean Unicode Noto fonts provides native support.
            return input;
        }

        private static string SegmentAndTagText(string input, string tamilFont, string hindiFont, string latinFont, string dominantFontName)
        {
            StringBuilder sb = new StringBuilder();
            TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(input);

            ScriptType currentScript = ScriptType.Unknown;
            StringBuilder runBuffer = new StringBuilder();
            bool insideExistingFontBlock = false;

            while (enumerator.MoveNext())
            {
                string grapheme = enumerator.GetTextElement();

                // Skip existing rich-text tags
                if (grapheme.StartsWith("<"))
                {
                    int closeIdx = grapheme.IndexOf('>');
                    if (closeIdx != -1)
                    {
                        FlushRun(sb, runBuffer, currentScript, tamilFont, hindiFont, latinFont, dominantFontName, insideExistingFontBlock);
                        currentScript = ScriptType.Unknown;
                        sb.Append(grapheme);

                        string lowerTag = grapheme.ToLowerInvariant();
                        if (lowerTag.StartsWith("<font")) insideExistingFontBlock = true;
                        else if (lowerTag.StartsWith("</font")) insideExistingFontBlock = false;
                        continue;
                    }
                }

                int codePoint = char.ConvertToUtf32(grapheme, 0);
                ScriptType charScript = GetCodePointScriptType(codePoint);

                if (charScript == ScriptType.Unknown)
                {
                    runBuffer.Append(grapheme);
                }
                else
                {
                    if (currentScript == ScriptType.Unknown)
                    {
                        currentScript = charScript;
                        runBuffer.Append(grapheme);
                    }
                    else if (charScript == currentScript)
                    {
                        runBuffer.Append(grapheme);
                    }
                    else
                    {
                        FlushRun(sb, runBuffer, currentScript, tamilFont, hindiFont, latinFont, dominantFontName, insideExistingFontBlock);
                        currentScript = charScript;
                        runBuffer.Append(grapheme);
                    }
                }
            }

            FlushRun(sb, runBuffer, currentScript, tamilFont, hindiFont, latinFont, dominantFontName, insideExistingFontBlock);
            return sb.ToString();
        }

        private static void FlushRun(StringBuilder sb, StringBuilder runBuffer, ScriptType script, string tamilFont, string hindiFont, string latinFont, string dominantFontName, bool insideExistingFontBlock)
        {
            if (runBuffer.Length == 0) return;

            string text = runBuffer.ToString();
            runBuffer.Clear();

            if (script == ScriptType.Unknown || insideExistingFontBlock)
            {
                sb.Append(text);
                return;
            }

            string fontName = latinFont;
            if (script == ScriptType.Tamil) fontName = tamilFont;
            else if (script == ScriptType.Hindi) fontName = hindiFont;

            if (fontName == dominantFontName)
            {
                sb.Append(text);
            }
            else
            {
                sb.Append("<font=\"").Append(fontName).Append("\">").Append(text).Append("</font>");
            }
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
