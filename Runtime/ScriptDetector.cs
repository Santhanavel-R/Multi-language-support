using System;
using System.Collections.Generic;
using System.Globalization;

namespace MultiLanguageSupporter
{
    public enum ScriptType
    {
        Unknown,
        Latin,       // English, Malay, Indonesian, Filipino, Vietnamese
        Cyrillic,
        Greek,
        Arabic,      // Arabic, Urdu
        Hebrew,
        Thai,
        Lao,
        Khmer,
        Burmese,     // Myanmar
        Hindi,       // Devanagari (Hindi, Marathi, Nepali)
        Bengali,
        Gurmukhi,    // Punjabi
        Gujarati,
        Odia,
        Tamil,
        Telugu,
        Kannada,
        Malayalam,
        Sinhala,
        Chinese,     // CJK Unified Ideographs
        Japanese,    // Hiragana, Katakana, Kanji
        Korean,      // Hangul
        Emoji
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
            foreach (ScriptType type in Enum.GetValues(typeof(ScriptType)))
            {
                counts[type] = 0;
            }

            int validClusters = 0;
            bool hasNonLatin = false;

            TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(text);
            while (enumerator.MoveNext())
            {
                string grapheme = enumerator.GetTextElement();
                
                // Skip formatting/tags if we are evaluating raw or partially formatted text
                if (grapheme.StartsWith("<") && grapheme.EndsWith(">"))
                {
                    continue;
                }

                // Check first code point in the grapheme cluster
                int codePoint = char.ConvertToUtf32(grapheme, 0);
                
                // Skip whitespaces, digits, common punctuation, etc.
                if (IsNeutralCodePoint(codePoint))
                {
                    continue;
                }

                validClusters++;
                ScriptType type = GetCodePointScriptType(codePoint);
                counts[type]++;

                if (type != ScriptType.Latin && type != ScriptType.Unknown)
                {
                    hasNonLatin = true;
                }
            }

            if (validClusters == 0)
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
                int nonLatinCount = validClusters - latinCount;
                if (nonLatinCount > 0 && nonLatinCount >= latinCount)
                {
                    return dominant;
                }
            }

            return dominant;
        }

        public static bool IsNeutralCodePoint(int codePoint)
        {
            // ASCII Control, Whitespace, Punctuation, Digits
            if (codePoint <= 0x0020) return true; // Spaces & Controls
            if (codePoint >= 0x0021 && codePoint <= 0x002F) return true; // ASCII Punctuation / Math symbols
            if (codePoint >= 0x0030 && codePoint <= 0x0039) return true; // Digits
            if (codePoint >= 0x003A && codePoint <= 0x0040) return true; // ASCII Punctuation
            if (codePoint >= 0x005B && codePoint <= 0x0060) return true; // ASCII Punctuation
            if (codePoint >= 0x007B && codePoint <= 0x007E) return true; // ASCII Punctuation
            
            // Common general punctuation
            if (codePoint >= 0x2000 && codePoint <= 0x206F) return true; 

            // CJK Symbols and Punctuation
            if (codePoint >= 0x3000 && codePoint <= 0x303F) return true;

            return false;
        }

        public static ScriptType GetCodePointScriptType(int codePoint)
        {
            // Emojis / Miscellaneous Symbols
            if ((codePoint >= 0x1F300 && codePoint <= 0x1F9FF) ||
                (codePoint >= 0x1F600 && codePoint <= 0x1F64F) ||
                (codePoint >= 0x1F680 && codePoint <= 0x1F6FF) ||
                (codePoint >= 0x2600 && codePoint <= 0x26FF) ||
                (codePoint >= 0x2700 && codePoint <= 0x27BF) ||
                (codePoint >= 0x1F1E6 && codePoint <= 0x1F1FF)) // Regional Indicators / Flags
            {
                return ScriptType.Emoji;
            }

            // Devanagari (Hindi, Marathi, Nepali): U+0900 to U+097F
            if (codePoint >= 0x0900 && codePoint <= 0x097F)
                return ScriptType.Hindi;

            // Bengali: U+0980 to U+09FF
            if (codePoint >= 0x0980 && codePoint <= 0x09FF)
                return ScriptType.Bengali;

            // Gurmukhi (Punjabi): U+0A00 to U+0A7F
            if (codePoint >= 0x0A00 && codePoint <= 0x0A7F)
                return ScriptType.Gurmukhi;

            // Gujarati: U+0A80 to U+0AFF
            if (codePoint >= 0x0A80 && codePoint <= 0x0AFF)
                return ScriptType.Gujarati;

            // Odia: U+0B00 to U+0B7F
            if (codePoint >= 0x0B00 && codePoint <= 0x0B7F)
                return ScriptType.Odia;

            // Tamil: U+0B80 to U+0BFF
            if (codePoint >= 0x0B80 && codePoint <= 0x0BFF)
                return ScriptType.Tamil;

            // Telugu: U+0C00 to U+0C7F
            if (codePoint >= 0x0C00 && codePoint <= 0x0C7F)
                return ScriptType.Telugu;

            // Kannada: U+0C80 to U+0CFF
            if (codePoint >= 0x0C80 && codePoint <= 0x0CFF)
                return ScriptType.Kannada;

            // Malayalam: U+0D00 to U+0D7F
            if (codePoint >= 0x0D00 && codePoint <= 0x0D7F)
                return ScriptType.Malayalam;

            // Sinhala: U+0D80 to U+0DFF
            if (codePoint >= 0x0D80 && codePoint <= 0x0DFF)
                return ScriptType.Sinhala;

            // Thai: U+0E00 to U+0E7F
            if (codePoint >= 0x0E00 && codePoint <= 0x0E7F)
                return ScriptType.Thai;

            // Lao: U+0E80 to U+0EFF
            if (codePoint >= 0x0E80 && codePoint <= 0x0EFF)
                return ScriptType.Lao;

            // Tibetan: U+0F00 to U+0FFF
            if (codePoint >= 0x0F00 && codePoint <= 0x0FFF)
                return ScriptType.Unknown;

            // Myanmar: U+1000 to U+109F
            if (codePoint >= 0x1000 && codePoint <= 0x109F)
                return ScriptType.Burmese;

            // Khmer: U+1780 to U+17FF
            if (codePoint >= 0x1780 && codePoint <= 0x17FF)
                return ScriptType.Khmer;

            // Hebrew: U+0590 to U+05FF
            if (codePoint >= 0x0590 && codePoint <= 0x05FF)
                return ScriptType.Hebrew;

            // Arabic: U+0600 to U+06FF, U+0750 to U+077F, U+08A0 to U+08FF
            if ((codePoint >= 0x0600 && codePoint <= 0x06FF) ||
                (codePoint >= 0x0750 && codePoint <= 0x077F) ||
                (codePoint >= 0x08A0 && codePoint <= 0x08FF))
                return ScriptType.Arabic;

            // Cyrillic: U+0400 to U+04FF, U+0500 to U+052F
            if ((codePoint >= 0x0400 && codePoint <= 0x04FF) ||
                (codePoint >= 0x0500 && codePoint <= 0x052F))
                return ScriptType.Cyrillic;

            // Greek: U+0370 to U+03FF
            if (codePoint >= 0x0370 && codePoint <= 0x03FF)
                return ScriptType.Greek;

            // Japanese: Hiragana U+3040..U+309F, Katakana U+30A0..U+30FF
            if ((codePoint >= 0x3040 && codePoint <= 0x309F) ||
                (codePoint >= 0x30A0 && codePoint <= 0x30FF))
                return ScriptType.Japanese;

            // Korean: Hangul Syllables U+AC00..U+D7AF, Hangul Jamo U+1100..U+11FF, Hangul Compatibility Jamo U+3130..U+318F
            if ((codePoint >= 0xAC00 && codePoint <= 0xD7AF) ||
                (codePoint >= 0x1100 && codePoint <= 0x11FF) ||
                (codePoint >= 0x3130 && codePoint <= 0x318F))
                return ScriptType.Korean;

            // CJK/Chinese: CJK Unified Ideographs U+4E00..U+9FFF, CJK Extension A U+3400..U+4DBF, CJK Extension B/etc. above U+20000
            if ((codePoint >= 0x4E00 && codePoint <= 0x9FFF) ||
                (codePoint >= 0x3400 && codePoint <= 0x4DBF) ||
                (codePoint >= 0x20000 && codePoint <= 0x2A6DF))
                return ScriptType.Chinese;

            // Latin: Basic Latin, Latin-1, Latin Extended-A, B, and Vietnamese (Latin Additional: 1EA0-1EF9)
            if ((codePoint >= 0x0041 && codePoint <= 0x005A) || // A-Z
                (codePoint >= 0x0061 && codePoint <= 0x007A) || // a-z
                (codePoint >= 0x00C0 && codePoint <= 0x00FF) || // Latin-1
                (codePoint >= 0x0100 && codePoint <= 0x017F) || // Extended-A
                (codePoint >= 0x0180 && codePoint <= 0x024F) || // Extended-B
                (codePoint >= 0x1EA0 && codePoint <= 0x1EF9))   // Vietnamese
                return ScriptType.Latin;

            return ScriptType.Unknown;
        }
    }
}
