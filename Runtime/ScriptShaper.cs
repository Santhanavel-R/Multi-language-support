using System.Text;

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

            // Shape languages that need left-vowel reordering
            input = ShapeTamil(input, tamilFontName);
            input = ShapeDevanagari(input, hindiFontName, latinFontName);
            input = ShapeBengali(input);

            return input;
        }

        private static string ShapeTamil(string input, string tamilFontName)
        {
            if (string.IsNullOrEmpty(input)) return input;

            bool hasTamil = false;
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c >= 0x0B80 && c <= 0x0BFF)
                {
                    hasTamil = true;
                    break;
                }
            }

            if (!hasTamil)
            {
                return input;
            }

            StringBuilder sb = new StringBuilder();
            StringBuilder tamilGroup = new StringBuilder();
            bool insideFontBlock = false;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (TryReadTag(input, i, out string tag, out int tagEndIndex))
                {
                    if (tamilGroup.Length > 0)
                    {
                        string converted = TamilEncoder.TamilEncoding.ConvertFromUnicode(tamilGroup.ToString(), TamilEncoder.TamilFontEncoding.TSCII);
                        sb.Append("<font=\"").Append(tamilFontName).Append("\">").Append(converted).Append("</font>");
                        tamilGroup.Clear();
                    }
                    sb.Append(tag);
                    string lowerTag = tag.ToLowerInvariant();
                    if (lowerTag.StartsWith("<font")) insideFontBlock = true;
                    else if (lowerTag.StartsWith("</font")) insideFontBlock = false;
                    i = tagEndIndex;
                    continue;
                }

                if (insideFontBlock)
                {
                    sb.Append(c);
                    continue;
                }

                if (c >= 0x0B80 && c <= 0x0BFF)
                {
                    tamilGroup.Append(c);
                }
                else
                {
                    if (tamilGroup.Length > 0)
                    {
                        string converted = TamilEncoder.TamilEncoding.ConvertFromUnicode(tamilGroup.ToString(), TamilEncoder.TamilFontEncoding.TSCII);
                        sb.Append("<font=\"").Append(tamilFontName).Append("\">").Append(converted).Append("</font>");
                        tamilGroup.Clear();
                    }
                    sb.Append(c);
                }
            }

            if (tamilGroup.Length > 0)
            {
                string converted = TamilEncoder.TamilEncoding.ConvertFromUnicode(tamilGroup.ToString(), TamilEncoder.TamilFontEncoding.TSCII);
                sb.Append("<font=\"").Append(tamilFontName).Append("\">").Append(converted).Append("</font>");
            }

            return sb.ToString();
        }

        private static string ShapeDevanagari(string input, string hindiFontName, string latinFontName)
        {
            if (string.IsNullOrEmpty(input)) return input;

            bool hasDevanagari = false;
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c >= 0x0900 && c <= 0x097F)
                {
                    hasDevanagari = true;
                    break;
                }
            }

            if (!hasDevanagari)
            {
                return input;
            }

            StringBuilder sb = new StringBuilder();
            bool insideFontBlock = false;
            StringBuilder currentHindi = new StringBuilder();
            StringBuilder currentLatin = new StringBuilder();

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (TryReadTag(input, i, out string tag, out int tagEndIndex))
                {
                    FlushDevAndLatin(sb, currentHindi, currentLatin, hindiFontName, latinFontName);
                    sb.Append(tag);
                    string lowerTag = tag.ToLowerInvariant();
                    if (lowerTag.StartsWith("<font")) insideFontBlock = true;
                    else if (lowerTag.StartsWith("</font")) insideFontBlock = false;
                    i = tagEndIndex;
                    continue;
                }

                if (insideFontBlock)
                {
                    sb.Append(c);
                    continue;
                }

                bool isLatinAlphaNum = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9');
                
                if (isLatinAlphaNum)
                {
                    if (currentHindi.Length > 0)
                    {
                        FlushDevAndLatin(sb, currentHindi, currentLatin, hindiFontName, latinFontName);
                    }
                    currentLatin.Append(c);
                }
                else
                {
                    if (currentLatin.Length > 0 && (c == ' ' || c == '.' || c == ',' || c == '!' || c == '?' || c == '-' || c == '(' || c == ')' || c == '\'' || c == '"'))
                    {
                        currentLatin.Append(c);
                    }
                    else
                    {
                        if (currentLatin.Length > 0)
                        {
                            FlushDevAndLatin(sb, currentHindi, currentLatin, hindiFontName, latinFontName);
                        }
                        currentHindi.Append(c);
                    }
                }
            }

            FlushDevAndLatin(sb, currentHindi, currentLatin, hindiFontName, latinFontName);
            return sb.ToString();
        }

        private static void FlushDevAndLatin(StringBuilder sb, StringBuilder hindi, StringBuilder latin, string hindiFontName, string latinFontName)
        {
            if (hindi.Length > 0)
            {
                string converted = UnicodeToKrutidev.Convert(hindi.ToString());
                sb.Append("<font=\"").Append(hindiFontName).Append("\">").Append(converted).Append("</font>");
                hindi.Clear();
            }
            if (latin.Length > 0)
            {
                string latStr = latin.ToString();
                
                string trimmed = latStr.TrimEnd(' ', '.', ',', '!', '?', '-', '(', ')', '\'', '"');
                string trailing = latStr.Substring(trimmed.Length);
                
                string cleanLatin = trimmed.TrimStart(' ', '.', ',', '!', '?', '-', '(', ')', '\'', '"');
                string leading = trimmed.Substring(0, trimmed.Length - cleanLatin.Length);

                if (cleanLatin.Length > 0)
                {
                    if (leading.Length > 0)
                    {
                        sb.Append("<font=\"").Append(hindiFontName).Append("\">").Append(UnicodeToKrutidev.Convert(leading)).Append("</font>");
                    }
                    sb.Append("<font=\"").Append(latinFontName).Append("\">").Append(cleanLatin).Append("</font>");
                    if (trailing.Length > 0)
                    {
                        sb.Append("<font=\"").Append(hindiFontName).Append("\">").Append(UnicodeToKrutidev.Convert(trailing)).Append("</font>");
                    }
                }
                else
                {
                    sb.Append("<font=\"").Append(hindiFontName).Append("\">").Append(UnicodeToKrutidev.Convert(latStr)).Append("</font>");
                }
                latin.Clear();
            }
        }

        private static string ShapeBengali(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            bool hasBengali = false;
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c >= 0x0980 && c <= 0x09FF)
                {
                    hasBengali = true;
                    break;
                }
            }

            if (!hasBengali)
            {
                return input;
            }

            StringBuilder sb = new StringBuilder();
            bool insideFontBlock = false;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (TryReadTag(input, i, out string tag, out int tagEndIndex))
                {
                    sb.Append(tag);
                    string lowerTag = tag.ToLowerInvariant();
                    if (lowerTag.StartsWith("<font")) insideFontBlock = true;
                    else if (lowerTag.StartsWith("</font")) insideFontBlock = false;
                    i = tagEndIndex;
                    continue;
                }

                if (insideFontBlock)
                {
                    sb.Append(c);
                    continue;
                }

                if (IsBengaliConsonant(c))
                {
                    int clusterEnd = i;
                    while (clusterEnd + 1 < input.Length)
                    {
                        if (input[clusterEnd + 1] == '\u09CD')
                        {
                            clusterEnd += 1;
                            if (clusterEnd + 1 < input.Length && IsBengaliConsonant(input[clusterEnd + 1]))
                            {
                                clusterEnd += 1;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (clusterEnd + 1 < input.Length)
                    {
                        char next = input[clusterEnd + 1];
                        if (next == '\u09BF')
                        {
                            sb.Append('\u09BF');
                            sb.Append(input.Substring(i, clusterEnd - i + 1));
                            i = clusterEnd + 1;
                            continue;
                        }
                        if (next == '\u09C7')
                        {
                            sb.Append('\u09C7');
                            sb.Append(input.Substring(i, clusterEnd - i + 1));
                            i = clusterEnd + 1;
                            continue;
                        }
                        if (next == '\u09C8')
                        {
                            sb.Append('\u09C8');
                            sb.Append(input.Substring(i, clusterEnd - i + 1));
                            i = clusterEnd + 1;
                            continue;
                        }
                        if (next == '\u09CB')
                        {
                            sb.Append('\u09C7');
                            sb.Append(input.Substring(i, clusterEnd - i + 1));
                            sb.Append('\u09BE');
                            i = clusterEnd + 1;
                            continue;
                        }
                        if (next == '\u09CC')
                        {
                            sb.Append('\u09C7');
                            sb.Append(input.Substring(i, clusterEnd - i + 1));
                            sb.Append('\u09D7');
                            i = clusterEnd + 1;
                            continue;
                        }
                    }
                }
                sb.Append(c);
            }
            return sb.ToString();
        }

        private static bool IsBengaliConsonant(char c)
        {
            return c >= '\u0995' && c <= '\u09B9';
        }

        private static bool TryReadTag(string input, int index, out string tag, out int tagEndIndex)
        {
            tag = null;
            tagEndIndex = index;
            if (input[index] != '<') return false;

            int closeIndex = input.IndexOf('>', index);
            if (closeIndex == -1) return false;

            tag = input.Substring(index, closeIndex - index + 1);
            tagEndIndex = closeIndex;
            return true;
        }

        private static bool IsFontAssetHealthy(TMPro.TMP_FontAsset font)
        {
            return font != null && 
                   font.atlasTextures != null && 
                   font.atlasTextures.Length > 0 && 
                   font.atlasTextures[0] != null && 
                   font.material != null;
        }
    }
}
