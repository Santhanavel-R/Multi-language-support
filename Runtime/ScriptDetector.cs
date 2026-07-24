using System.Collections.Generic;

namespace MultiLanguageSupporter
{
    public enum ScriptType
    {
        Unknown,
        Latin,       // English, Malay, Indonesian, Filipino
        Tamil,
        Hindi,       // Devanagari script
        Bengali,
        Kannada,
        Malayalam,
        Thai,
        Korean,      // Hangul
        Chinese      // CJK Unified Ideographs
    }

    public static class ScriptDetector
    {
        public static ScriptType DetectDominantScript(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return ScriptType.Latin;
            }

            var counts = new Dictionary<ScriptType, int>();

            counts[ScriptType.Latin] = 0;
            counts[ScriptType.Tamil] = 0;
            counts[ScriptType.Hindi] = 0;
            counts[ScriptType.Bengali] = 0;
            counts[ScriptType.Kannada] = 0;
            counts[ScriptType.Malayalam] = 0;
            counts[ScriptType.Thai] = 0;
            counts[ScriptType.Korean] = 0;
            counts[ScriptType.Chinese] = 0;
            counts[ScriptType.Unknown] = 0;

            int validChars = 0;
            bool hasNonLatin = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (char.IsWhiteSpace(c) || char.IsPunctuation(c) || char.IsControl(c) || char.IsDigit(c) || c == '<' || c == '>')
                {
                    if (c == '<' && i < text.Length - 1)
                    {
                        int closeIdx = text.IndexOf('>', i);
                        if (closeIdx != -1)
                        {
                            i = closeIdx;
                            continue;
                        }
                    }
                    continue;
                }

                validChars++;
                ScriptType type = GetCharScriptType(c);
                counts[type]++;

                if (type != ScriptType.Latin && type != ScriptType.Unknown)
                {
                    hasNonLatin = true;
                }
            }

            if (validChars == 0)
            {
                return ScriptType.Latin;
            }

            ScriptType dominant = ScriptType.Latin;
            int maxCount = -1;
            int latinCount = counts[ScriptType.Latin];

            foreach (var kvp in counts)
            {
                if (kvp.Key == ScriptType.Unknown) continue;

                if (kvp.Value > maxCount)
                {
                    maxCount = kvp.Value;
                    dominant = kvp.Key;
                }
            }

            if (hasNonLatin)
            {
                int nonLatinCount = validChars - latinCount;
                if (nonLatinCount > 0 && nonLatinCount >= latinCount)
                {
                    return dominant;
                }
            }

            if (maxCount == 0 && counts[ScriptType.Unknown] > 0)
            {
                return ScriptType.Unknown;
            }

            return dominant;
        }

        public static ScriptType GetCharScriptType(char c)
        {
            int code = (int)c;

            // Tamil: U+0B80 to U+0BFF
            if (code >= 0x0B80 && code <= 0x0BFF)
                return ScriptType.Tamil;

            // Hindi (Devanagari): U+0900 to U+097F
            if (code >= 0x0900 && code <= 0x097F)
                return ScriptType.Hindi;

            // Bengali: U+0980 to U+09FF
            if (code >= 0x0980 && code <= 0x09FF)
                return ScriptType.Bengali;

            // Kannada: U+0C80 to U+0CFF
            if (code >= 0x0C80 && code <= 0x0CFF)
                return ScriptType.Kannada;

            // Malayalam: U+0D00 to U+0D7F
            if (code >= 0x0D00 && code <= 0x0D7F)
                return ScriptType.Malayalam;

            // Thai: U+0E00 to U+0E7F
            if (code >= 0x0E00 && code <= 0x0E7F)
                return ScriptType.Thai;

            // Korean (Hangul): 
            // Hangul Syllables: U+AC00 to U+D7AF
            // Hangul Jamo: U+1100 to U+11FF
            // Hangul Compatibility Jamo: U+3130 to U+318F
            if ((code >= 0xAC00 && code <= 0x0D7AF) ||
                (code >= 0x1100 && code <= 0x11FF) ||
                (code >= 0x3130 && code <= 0x318F))
                return ScriptType.Korean;

            // Chinese (CJK Unified Ideographs): U+4E00 to U+9FFF
            // Extension A: U+3400 to U+4DBF
            if ((code >= 0x4E00 && code <= 0x9FFF) ||
                (code >= 0x3400 && code <= 0x4DBF))
                return ScriptType.Chinese;

            // Latin (includes basic ASCII, Latin-1 Supplement, Latin Extended A, B)
            if ((code >= 0x0041 && code <= 0x005A) || // A-Z
                (code >= 0x0061 && code <= 0x007A) || // a-z
                (code >= 0x00C0 && code <= 0x00FF) || // Latin-1 Supplement letters
                (code >= 0x0100 && code <= 0x017F) || // Latin Extended-A
                (code >= 0x0180 && code <= 0x024F))   // Latin Extended-B
                return ScriptType.Latin;

            return ScriptType.Unknown;
        }
    }
}
