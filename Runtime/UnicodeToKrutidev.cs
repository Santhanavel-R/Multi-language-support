using System;
using System.Text;

namespace MultiLanguageSupporter
{
    public static class UnicodeToKrutidev
    {
        private static readonly string[] array_one = new string[] {
            "‘",   "’",   "“",   "”",   "(",    ")",   "{",    "}",   "=", "।",  "?",  "-",  "µ", "॰", ",", ".", "् ", 
            "०",  "१",  "२",  "३",     "४",   "५",  "६",   "७",   "८",   "९", "x", 
            
            "फ़्",  "क़",  "ख़",  "ग़", "ज़्", "ज़",  "ड़",  "ढ़",   "फ़",  "य़",  "ऱ",  "ऩ",  
            "त्त्",   "त्त",     "क्त",  "दृ",  "कृ",
            
            "ह्न",  "ह्य",  "हृ",  "ह्म",  "ह्र",  "ह्",   "द्द",  "क्ष्", "क्ष", "त्र्", "त्र","ज्ञ",
            "छ्य",  "ट्य",  "ठ्य",  "ड्य",  "ढ्य", "द्य","द्व",
            "श्र",  "ट्र",    "ड्र",    "ढ्र",    "छ्र",   "क्र",  "फ्र",  "द्र",   "प्र",   "ग्र", "रु",  "रू",
            "्र",
            
            "ओ",  "औ",  "आ",   "अ",   "ई",   "इ",  "उ",   "ऊ",  "ऐ",  "ए", "ऋ",
            
            "क्",  "क",  "क्क",  "ख्",   "ख",    "ग्",   "ग",  "घ्",  "घ",    "ङ",
            "चै",   "च्",   "च",   "छ",  "ज्", "ज",   "झ्",  "झ",   "ञ",
            
            "ट्ट",   "ट्ठ",   "ट",   "ठ",   "ड्ड",   "ड्ढ",  "ड",   "ढ",  "ण्", "ण",  
            "त्",  "त",  "थ्", "थ",  "द्ध",  "द", "ध्", "ध",  "न्",  "न",  
            
            "प्",  "प",  "फ्", "फ",  "ब्",  "ब", "भ्",  "भ",  "म्",  "म",
            "य्",  "य",  "र",  "ल्", "ल",  "ळ",  "व्",  "व", 
            "श्", "श",  "ष्", "ष",  "स्",   "स",   "ह",     
            
            "ऑ",   "ॉ",  "ो",   "ौ",   "ा",   "ी",   "ु",   "ू",   "ृ",   "े",   "ै",
            "ं",   "ँ",   "ः",   "ॅ",    "ऽ",  "् ", "्"
        };

        private static readonly string[] array_two = new string[] {
            "^", "*",  "Þ", "ß", "¼", "½", "¿", "À", "¾", "A", "\\", "&", "&", "Œ", "]","-","~ ", 
            "å",  "ƒ",  "„",   "…",   "†",   "‡",   "ˆ",   "‰",   "Š",   "‹","Û",
            
            "¶",   "d",    "[k",  "x",  "T",  "t",   "M+", "<+", "Q",  ";",    "j",   "u",
            "Ù",   "Ùk",   "Dr",    "–",   "—",       
            
            "à",   "á",    "â",   "ã",   "ºz",  "º",   "í", "{", "{k",  "«", "=","K", 
            "Nî",   "Vî",    "Bî",   "Mî",   "<î", "|","}",
            "J",   "Vª",   "Mª",  "<ªª",  "Nª",   "Ø",  "Ý",   "æ", "ç", "xz", "#", ":",
            "z",
            
            "vks",  "vkS",  "vk",    "v",   "bZ",  "b",  "m",  "Å",  ",s",  ",",   "_",
            
            "D",  "d",    "ô",     "[",     "[k",    "X",   "x",  "?",    "?k",   "³", 
            "pkS",  "P",    "p",  "N",   "T",    "t",   "÷",  ">",   "¥",
            
            "ê",      "ë",      "V",  "B",   "ì",       "ï",     "M",  "<",  ".", ".k",   
            "R",  "r",   "F", "Fk",  ")",    "n", "/",  "/k",  "U", "u",   
            
            "I",  "i",   "¶", "Q",   "C",  "c",  "H",  "Hk", "E",   "e",
            "¸",   ";",    "j",  "Y",   "y",  "G",  "O",  "o",
            "'", "'k",  "\"", "\"k", "L",   "l",   "g",      
            
            "v‚",    "‚",    "ks",   "kS",   "k",     "h",    "q",   "w",   "`",    "s",    "S",
            "a",    "¡",    "%",     "W",   "·",   "~ ", "~"
        };

        public static string Convert(string unicodeText)
        {
            if (string.IsNullOrEmpty(unicodeText)) return unicodeText;

            // Convert straight quotes to curly quotes before mapping so they map to correct Krutidev symbols instead of random letters
            string modified = ConvertStraightQuotesToCurly(unicodeText);

            // Specialty replacements before mapping: normalize precomposed characters (like U+095C, U+095D) to composite sequences
            modified = modified.Replace("\u0958", "क\u093C"); // क़
            modified = modified.Replace("\u0959", "ख\u093C"); // ख़
            modified = modified.Replace("\u095A", "ग\u093C"); // ग़
            modified = modified.Replace("\u095B", "ज\u093C"); // ज़
            modified = modified.Replace("\u095C", "ड\u093C"); // ड़
            modified = modified.Replace("\u095D", "ढ\u093C"); // ढ़
            modified = modified.Replace("\u095E", "फ\u093C"); // फ़
            modified = modified.Replace("\u095F", "य\u093C"); // य़
            modified = modified.Replace("\u0931", "र\u093C"); // ऱ
            modified = modified.Replace("\u0929", "न\u093C"); // ऩ
            
            // Replace short-i matra
            modified = modified.Replace("ि", "f");

            // Replace Unicode characters with Krutidev ASCII mapping using a single-pass loop to prevent multi-pass corruption
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < modified.Length; )
            {
                bool matched = false;
                for (int idx = 0; idx < array_one.Length; idx++)
                {
                    string target = array_one[idx];
                    if (i + target.Length <= modified.Length && modified.Substring(i, target.Length) == target)
                    {
                        sb.Append(array_two[idx]);
                        i += target.Length;
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                {
                    sb.Append(modified[i]);
                    i++;
                }
            }
            modified = sb.ToString();

            // Move "f" to the correct position (before the consonant or consonant cluster)
            modified = "  " + modified + "  ";
            int positionOfF = modified.IndexOf("f");
            while (positionOfF != -1)
            {
                int shiftIndex = positionOfF - 1;
                while (shiftIndex > 2)
                {
                    if (modified[shiftIndex] == '+')
                    {
                        shiftIndex--;
                        continue;
                    }

                    char prevChar = modified[shiftIndex - 1];
                    // If the previous character is a half-consonant joiner (like halant or a half-character in Krutidev)
                    if (prevChar == 'D' || prevChar == 'P' || prevChar == '[' || prevChar == 'X' || prevChar == '?' ||
                        prevChar == 'T' || prevChar == 'R' || prevChar == 'F' || prevChar == 'I' || prevChar == 'C' ||
                        prevChar == 'H' || prevChar == 'E' || prevChar == 'Y' || prevChar == 'L' || prevChar == 'O' ||
                        prevChar == '\'' || prevChar == '"' || prevChar == '}' || prevChar == '{' || prevChar == '|')
                    {
                        shiftIndex--;
                    }
                    else
                    {
                        break;
                    }
                }

                // Perform the shift of "f" to the left of the cluster
                modified = modified.Substring(0, shiftIndex) + "f" + modified.Substring(shiftIndex, positionOfF - shiftIndex) + modified.Substring(positionOfF + 1);
                positionOfF = modified.IndexOf("f", positionOfF + 1);
            }
            modified = modified.Trim();

            // Move "half R" (repha) to correct position and replace
            modified = "  " + modified + "  ";
            int positionOfR = modified.IndexOf("j~");
            string[] setOfMatras = new string[] { "‚", "ks", "kS", "k", "h", "q", "w", "`", "s", "S", "a", "¡", "%", "W", "·", "~ ", "~" };
            while (positionOfR != -1)
            {
                modified = modified.Remove(positionOfR, 2);
                
                int shiftIndex = positionOfR;
                while (shiftIndex < modified.Length - 2)
                {
                    string nextChar = modified[shiftIndex].ToString();
                    bool isMatra = false;
                    foreach (var matra in setOfMatras)
                    {
                        if (nextChar == matra)
                        {
                            isMatra = true;
                            break;
                        }
                    }
                    if (isMatra)
                    {
                        shiftIndex++;
                    }
                    else
                    {
                        break;
                    }
                }

                modified = modified.Substring(0, shiftIndex) + "Z" + modified.Substring(shiftIndex);
                positionOfR = modified.IndexOf("j~");
            }
            modified = modified.Trim();

            return modified;
        }

        private static string ConvertStraightQuotesToCurly(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            StringBuilder sb = new StringBuilder();
            bool insideSingle = false;
            bool insideDouble = false;
            
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '\'')
                {
                    if (insideSingle)
                    {
                        sb.Append('’'); // closing curly single
                    }
                    else
                    {
                        sb.Append('‘'); // opening curly single
                    }
                    insideSingle = !insideSingle;
                }
                else if (c == '"')
                {
                    if (insideDouble)
                    {
                        sb.Append('”'); // closing curly double
                    }
                    else
                    {
                        sb.Append('“'); // opening curly double
                    }
                    insideDouble = !insideDouble;
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
