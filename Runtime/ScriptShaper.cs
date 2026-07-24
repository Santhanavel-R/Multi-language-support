using System.Text;

namespace MultiLanguageSupporter
{
    public static class ScriptShaper
    {
        public static string Shape(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Shape languages that need left-vowel reordering
            input = ShapeTamil(input);
            input = ShapeDevanagari(input);
            input = ShapeBengali(input);

            return input;
        }

        private static string ShapeTamil(string input)
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

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c >= 0x0B80 && c <= 0x0BFF)
                {
                    tamilGroup.Append(c);
                }
                else
                {
                    if (tamilGroup.Length > 0)
                    {
                        string converted = TamilEncoder.TamilEncoding.ConvertFromUnicode(tamilGroup.ToString(), TamilEncoder.TamilFontEncoding.TSCII);
                        sb.Append("<font=\"SaiSaiSDF\">").Append(converted).Append("</font>");
                        tamilGroup.Clear();
                    }
                    sb.Append(c);
                }
            }

            if (tamilGroup.Length > 0)
            {
                string converted = TamilEncoder.TamilEncoding.ConvertFromUnicode(tamilGroup.ToString(), TamilEncoder.TamilFontEncoding.TSCII);
                sb.Append("<font=\"Sai-Sai SDF\">\").Append(converted).Append("</font>");
            }

            return sb.ToString();
        }

        private static string ShapeDevanagari(string input)
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
            bool insideTag = false;
            StringBuilder currentHindi = new StringBuilder();
            StringBuilder currentLatin = new StringBuilder();

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (c == '<')
                {
                    FlushDevAndLatin(sb, currentHindi, currentLatin);
                    insideTag = true;
                    sb.Append(c);
                }
                else if (c == '>')
                {
                    insideTag = false;
                    sb.Append(c);
                }
                else if (insideTag)
                {
                    sb.Append(c);
                }
                else
                {
                    bool isLatinAlphaNum = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9');
                    
                    if (isLatinAlphaNum)
                    {
                        if (currentHindi.Length > 0)
                        {
                            FlushDevAndLatin(sb, currentHindi, currentLatin);
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
                                FlushDevAndLatin(sb, currentHindi, currentLatin);
                            }
                            currentHindi.Append(c);
                        }
                    }
                }
            }

            FlushDevAndLatin(sb, currentHindi, currentLatin);
            return sb.ToString();
        }

        private static void FlushDevAndLatin(StringBuilder sb, StringBuilder hindi, StringBuilder latin)
        {
            if (hindi.Length > 0)
            {
                string converted = UnicodeToKrutidev.Convert(hindi.ToString());
                sb.Append("<font=\"Kruti Dev 010 SDF\">").Append(converted).Append("</font>");
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
                        sb.Append("<font=\"Kruti Dev 010 SDF\">").Append(UnicodeToKrutidev.Convert(leading)).Append("</font>");
                    }
                    sb.Append("<font=\"NotoSans-Regular SDF\">").Append(cleanLatin).Append("</font>");
                    if (trailing.Length > 0)
                    {
                        sb.Append("<font=\"Kruti Dev 010 SDF\">").Append(UnicodeToKrutidev.Convert(trailing)).Append("</font>");
                    }
                }
                else
                {
                    sb.Append("<font=\"Kruti Dev 010 SDF\">").Append(UnicodeToKrutidev.Convert(latStr)).Append("</font>");
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
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (IsBengaliConsonant(c))
                {
                    // Find the end of the consonant cluster (joined by U+09CD halant)
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
                        if (next == '\u09BF') // ি (short i)
                        {
                            sb.Append('\u09BF');
                            sb.Append(input.Substring(i, clusterEnd - i + 1));
                            i = clusterEnd + 1;
                            continue;
                        }
                        if (next == '\u09C7') // ে (e)
                        {
                            sb.Append('\u09C7');
                            sb.Append(input.Substring(i, clusterEnd - i + 1));
                            i = clusterEnd + 1;
                            continue;
                        }
                        if (next == '\u09C8') // ৈ (ai)
                        {
                            sb.Append('\u09C8');
                            sb.Append(input.Substring(i, clusterEnd - i + 1));
                            i = clusterEnd + 1;
                            continue;
                        }
                        if (next == '\u09CB') // ো (o) -> ে + consonant + া
                        {
                            sb.Append('\u09C7');
                            sb.Append(input.Substring(i, clusterEnd - i + 1));
                            sb.Append('\u09BE');
                            i = clusterEnd + 1;
                            continue;
                        }
                        if (next == '\u09CC') // ৌ (au) -> ে + consonant + ৗ
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
    }
}
