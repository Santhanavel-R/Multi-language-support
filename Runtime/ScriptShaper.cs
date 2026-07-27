using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using TMPro;

namespace MultiLanguageSupporter
{
    public static class ScriptShaper
    {
        public struct Token
        {
            public string Content;
            public bool IsTag;
            public bool IsFontTag;
            public bool IsClosingFontTag;
        }

        public class ScriptRun
        {
            public ScriptType Script;
            public StringBuilder Content = new StringBuilder();

            public ScriptRun(ScriptType script)
            {
                Script = script;
            }
        }

        public static string Shape(string input)
        {
            return Shape(input, null);
        }

        public static string Shape(string input, FontDatabase database)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Step 1: Unicode Validator (NFC)
            string normalized = input.Normalize(NormalizationForm.FormC);

            if (database == null)
            {
                database = FontResolver.GetDefaultDatabase();
            }

            // Resolve font names
            string tamilFont = ResolveFontName(database, ScriptType.Tamil, "NotoSansTamilRegularSDF");
            string hindiFont = ResolveFontName(database, ScriptType.Hindi, "NotoSansDevanagariRegularSDF");
            string latinFont = ResolveFontName(database, ScriptType.Latin, "NotoSansRegularSDF");
            string bengaliFont = ResolveFontName(database, ScriptType.Bengali, "NotoSansBengaliRegularSDF");
            string kannadaFont = ResolveFontName(database, ScriptType.Kannada, "NotoSansKannadaRegularSDF");
            string malayalamFont = ResolveFontName(database, ScriptType.Malayalam, "NotoSansMalayalamRegularSDF");
            string thaiFont = ResolveFontName(database, ScriptType.Thai, "NotoSansThaiRegularSDF");
            string chineseFont = ResolveFontName(database, ScriptType.Chinese, "ZCOOLXiaoWeiRegularSDF");
            string koreanFont = ResolveFontName(database, ScriptType.Korean, "SunflowerMediumSDF");

            // Step 2: Rich Text Tokenizer
            List<Token> tokens = Tokenize(normalized);

            // Determine dominant script of raw text parts
            StringBuilder rawTextBuilder = new StringBuilder();
            foreach (var token in tokens)
            {
                if (!token.IsTag)
                {
                    rawTextBuilder.Append(token.Content);
                }
            }
            ScriptType dominant = ScriptDetector.DetectDominantScript(rawTextBuilder.ToString());
            string dominantFontName = GetFontNameForScript(dominant, latinFont, tamilFont, hindiFont, bengaliFont, kannadaFont, malayalamFont, thaiFont, chineseFont, koreanFont);

            // Step 3-8: Process each token, building and generating the output
            StringBuilder result = new StringBuilder();
            bool insideExistingFontBlock = false;

            foreach (var token in tokens)
            {
                if (token.IsTag)
                {
                    if (token.IsFontTag) insideExistingFontBlock = true;
                    else if (token.IsClosingFontTag) insideExistingFontBlock = false;
                    result.Append(token.Content);
                }
                else
                {
                    // Run Grapheme Enumerator, Script Detector, Run Builder, Run Optimizer, Font Resolver & Rich Text Generator
                    string shapedChunk = ProcessTextChunk(token.Content, tamilFont, hindiFont, latinFont, bengaliFont, kannadaFont, malayalamFont, thaiFont, chineseFont, koreanFont, dominantFontName, insideExistingFontBlock);
                    result.Append(shapedChunk);
                }
            }

            return result.ToString();
        }

        private static string ResolveFontName(FontDatabase database, ScriptType script, string defaultName)
        {
            if (database != null)
            {
                var font = database.GetFontForScript(script);
                if (IsFontAssetHealthy(font)) return font.name;
            }
            return defaultName;
        }

        private static string GetFontNameForScript(ScriptType script, string latin, string tamil, string hindi, string bengali, string kannada, string malayalam, string thai, string chinese, string korean)
        {
            switch (script)
            {
                case ScriptType.Tamil: return tamil;
                case ScriptType.Hindi: return hindi;
                case ScriptType.Bengali: return bengali;
                case ScriptType.Kannada: return kannada;
                case ScriptType.Malayalam: return malayalam;
                case ScriptType.Thai: return thai;
                case ScriptType.Chinese: return chinese;
                case ScriptType.Korean: return korean;
                default: return latin;
            }
        }

        private static List<Token> Tokenize(string text)
        {
            List<Token> tokens = new List<Token>();
            int i = 0;
            int len = text.Length;
            StringBuilder currentText = new StringBuilder();

            while (i < len)
            {
                if (text[i] == '<')
                {
                    int closeIdx = text.IndexOf('>', i);
                    if (closeIdx != -1)
                    {
                        if (currentText.Length > 0)
                        {
                            tokens.Add(new Token { Content = currentText.ToString(), IsTag = false });
                            currentText.Clear();
                        }

                        string tag = text.Substring(i, closeIdx - i + 1);
                        string lowerTag = tag.ToLowerInvariant();
                        bool isFont = lowerTag.StartsWith("<font");
                        bool isClosingFont = lowerTag.StartsWith("</font");

                        tokens.Add(new Token 
                        { 
                            Content = tag, 
                            IsTag = true, 
                            IsFontTag = isFont, 
                            IsClosingFontTag = isClosingFont 
                        });
                        i = closeIdx + 1;
                        continue;
                    }
                    else
                    {
                        currentText.Append('<');
                        i++;
                    }
                }
                else
                {
                    currentText.Append(text[i]);
                    i++;
                }
            }

            if (currentText.Length > 0)
            {
                tokens.Add(new Token { Content = currentText.ToString(), IsTag = false });
            }

            return tokens;
        }

        private static string ProcessTextChunk(
            string text, 
            string tamilFont, 
            string hindiFont, 
            string latinFont,
            string bengaliFont,
            string kannadaFont,
            string malayalamFont,
            string thaiFont,
            string chineseFont,
            string koreanFont,
            string dominantFontName, 
            bool insideExistingFontBlock)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // Step 3-5: Grapheme Enumerator, Script Detector, and Run Builder
            List<ScriptRun> runs = new List<ScriptRun>();
            TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(text);
            
            while (enumerator.MoveNext())
            {
                string grapheme = enumerator.GetTextElement();
                int codePoint = char.ConvertToUtf32(grapheme, 0);
                ScriptType script = ScriptDetector.GetCodePointScriptType(codePoint);

                if (script == ScriptType.Unknown || ScriptDetector.IsNeutralCodePoint(codePoint))
                {
                    if (runs.Count == 0)
                    {
                        runs.Add(new ScriptRun(ScriptType.Unknown));
                    }
                    runs[runs.Count - 1].Content.Append(grapheme);
                }
                else
                {
                    if (runs.Count == 0 || runs[runs.Count - 1].Script != script)
                    {
                        runs.Add(new ScriptRun(script));
                    }
                    runs[runs.Count - 1].Content.Append(grapheme);
                }
            }

            // Step 6: Run Optimizer (Merge neutral/unknown runs)
            List<ScriptRun> optimizedRuns = new List<ScriptRun>();
            for (int k = 0; k < runs.Count; k++)
            {
                var run = runs[k];
                if (run.Script == ScriptType.Unknown)
                {
                    // Merge into preceding run if possible
                    if (optimizedRuns.Count > 0)
                    {
                        optimizedRuns[optimizedRuns.Count - 1].Content.Append(run.Content.ToString());
                    }
                    // Or merge into succeeding run if first run is Unknown
                    else if (k + 1 < runs.Count)
                    {
                        runs[k + 1].Content.Insert(0, run.Content.ToString());
                    }
                    else
                    {
                        optimizedRuns.Add(run); // Keep as Unknown/Default
                    }
                }
                else
                {
                    optimizedRuns.Add(run);
                }
            }

            // Step 7-8: Font Resolver and Rich Text Generator
            StringBuilder sb = new StringBuilder();
            foreach (var run in optimizedRuns)
            {
                string runText = run.Content.ToString();
                if (insideExistingFontBlock || run.Script == ScriptType.Unknown)
                {
                    sb.Append(runText);
                    continue;
                }

                string fontName = GetFontNameForScript(run.Script, latinFont, tamilFont, hindiFont, bengaliFont, kannadaFont, malayalamFont, thaiFont, chineseFont, koreanFont);
                if (fontName == dominantFontName)
                {
                    sb.Append(runText);
                }
                else
                {
                    sb.Append("<font=\"").Append(fontName).Append("\">").Append(runText).Append("</font>");
                }
            }

            return sb.ToString();
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
