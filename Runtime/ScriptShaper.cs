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

            // Delegate to the precompiled TamilEncoder library to convert Unicode to legacy TSCII encoding
            return TamilEncoder.TamilEncoding.ConvertFromUnicode(input, TamilEncoder.TamilFontEncoding.TSCII);
        }

        private static string ShapeDevanagari(string input)
        {
            return UnicodeToKrutidev.Convert(input);
        }

        private static string ShapeBengali(string input)
        {
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
