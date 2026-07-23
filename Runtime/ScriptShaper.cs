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
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (IsTamilConsonant(c))
                {
                    int clusterEnd = i;

                    if (clusterEnd + 1 < input.Length)
                    {
                        char next = input[clusterEnd + 1];
                        if (next == '\u0BC6') // ெ (short e)
                        {
                            sb.Append('\u0BC6');
                            sb.Append(c);
                            i = clusterEnd + 1;
                            continue;
                        }
                        if (next == '\u0BC7') // ே (long e)
                        {
                            sb.Append('\u0BC7');
                            sb.Append(c);
                            i = clusterEnd + 1;
                            continue;
                        }
                        if (next == '\u0BC8') // ை (ai)
                        {
                            sb.Append('\u0BC8');
                            sb.Append(c);
                            i = clusterEnd + 1;
                            continue;
                        }
                        if (next == '\u0BCA') // ொ (short o) -> ெ + consonant + ா
                        {
                            sb.Append('\u0BC6');
                            sb.Append(c);
                            sb.Append('\u0BBE');
                            i = clusterEnd + 1;
                            continue;
                        }
                        if (next == '\u0BCB') // ோ (long o) -> ே + consonant + ா
                        {
                            sb.Append('\u0BC7');
                            sb.Append(c);
                            sb.Append('\u0BBE');
                            i = clusterEnd + 1;
                            continue;
                        }
                        if (next == '\u0BCC') // ௌ (au) -> ெ + consonant + ள
                        {
                            sb.Append('\u0BC6');
                            sb.Append(c);
                            sb.Append('\u0BD7');
                            i = clusterEnd + 1;
                            continue;
                        }
                    }
                }
                sb.Append(c);
            }
            return sb.ToString();
        }

        private static bool IsTamilConsonant(char c)
        {
            return (c >= '\u0B95' && c <= '\u0BB9') || c == '\u0BD0';
        }

        private static string ShapeDevanagari(string input)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (IsDevanagariConsonant(c))
                {
                    // Find the end of the consonant cluster (joined by U+094D halant)
                    int clusterEnd = i;
                    while (clusterEnd + 1 < input.Length)
                    {
                        if (input[clusterEnd + 1] == '\u094D')
                        {
                            clusterEnd += 1;
                            if (clusterEnd + 1 < input.Length && IsDevanagariConsonant(input[clusterEnd + 1]))
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

                    if (clusterEnd + 1 < input.Length && input[clusterEnd + 1] == '\u093F') // ि (short i matra)
                    {
                        sb.Append('\u093F');
                        sb.Append(input.Substring(i, clusterEnd - i + 1));
                        i = clusterEnd + 1;
                        continue;
                    }
                }
                sb.Append(c);
            }
            return sb.ToString();
        }

        private static bool IsDevanagariConsonant(char c)
        {
            return c >= '\u0915' && c <= '\u0939';
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
