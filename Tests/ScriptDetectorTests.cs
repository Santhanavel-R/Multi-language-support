using NUnit.Framework;
using UnityEngine;
using TMPro;

namespace MultiLanguageSupporter.Tests
{
    [TestFixture]
    public class ScriptDetectorTests
    {
        [Test]
        public void DetectDominantScript_Latin_ReturnsLatin()
        {
            Assert.AreEqual(ScriptType.Latin, ScriptDetector.DetectDominantScript("Hello World"));
            Assert.AreEqual(ScriptType.Latin, ScriptDetector.DetectDominantScript("Selamat pagi")); // Malay/Indonesian
        }

        [Test]
        public void DetectDominantScript_Tamil_ReturnsTamil()
        {
            Assert.AreEqual(ScriptType.Tamil, ScriptDetector.DetectDominantScript("வணக்கம்"));
        }

        [Test]
        public void ScriptShaper_DoesNotReshapeAlreadyShapedTamilText()
        {
            string input = "<font=\"Sai-Sai SDF\">வணக்கம்</font> Hello";
            string output = ScriptShaper.Shape(input);

            Assert.AreEqual(input, output);
        }

        [Test]
        public void DetectDominantScript_Hindi_ReturnsHindi()
        {
            Assert.AreEqual(ScriptType.Hindi, ScriptDetector.DetectDominantScript("नमस्ते"));
        }

        [Test]
        public void DetectDominantScript_Bengali_ReturnsBengali()
        {
            Assert.AreEqual(ScriptType.Bengali, ScriptDetector.DetectDominantScript("বাংলা"));
        }

        [Test]
        public void DetectDominantScript_LatinAndTamilTie_ReturnsTamil()
        {
            Assert.AreEqual(ScriptType.Tamil, ScriptDetector.DetectDominantScript("A வ"));
        }

        [Test]
        public void DetectDominantScript_MixedLatinAndHindi_ReturnsHindi()
        {
            Assert.AreEqual(ScriptType.Hindi, ScriptDetector.DetectDominantScript("Hello नमस्ते"));
        }

        [Test]
        public void DetectDominantScript_Kannada_ReturnsKannada()
        {
            Assert.AreEqual(ScriptType.Kannada, ScriptDetector.DetectDominantScript("ನಮಸ್ಕಾರ"));
        }

        [Test]
        public void DetectDominantScript_Malayalam_ReturnsMalayalam()
        {
            Assert.AreEqual(ScriptType.Malayalam, ScriptDetector.DetectDominantScript("ഹലോ"));
        }

        [Test]
        public void DetectDominantScript_Thai_ReturnsThai()
        {
            Assert.AreEqual(ScriptType.Thai, ScriptDetector.DetectDominantScript("สวัสดี"));
        }

        [Test]
        public void DetectDominantScript_Korean_ReturnsKorean()
        {
            Assert.AreEqual(ScriptType.Korean, ScriptDetector.DetectDominantScript("안녕하세요"));
        }

        [Test]
        public void DetectDominantScript_Chinese_ReturnsChinese()
        {
            Assert.AreEqual(ScriptType.Chinese, ScriptDetector.DetectDominantScript("你好"));
        }

        [Test]
        public void DetectDominantScript_MixedDominant_ReturnsTamil()
        {
            // Mixed text with more Tamil than English
            Assert.AreEqual(ScriptType.Tamil, ScriptDetector.DetectDominantScript("Hello வணக்கம்"));
        }

        [Test]
        public void MultiLanguageText_PreprocessesTamilTextBeforeApplying()
        {
            var go = new GameObject("MultiLanguageTextTest");
            var textComponent = go.AddComponent<TextMeshPro>();
            var multiLanguageText = go.AddComponent<MultiLanguageText>();

            multiLanguageText.Text = "வணக்கம்";

            Assert.IsTrue(textComponent.text.Contains("<font=\"Sai-Sai SDF\">"));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void DetectDominantScript_ShapedTamil_ReturnsTamil()
        {
            Assert.AreEqual(ScriptType.Tamil, ScriptDetector.DetectDominantScript("<font=\"Sai-Sai SDF\">tíªfF«</font>"));
        }

        [Test]
        public void DetectDominantScript_ShapedHindi_ReturnsHindi()
        {
            Assert.AreEqual(ScriptType.Hindi, ScriptDetector.DetectDominantScript("<font=\"Kruti Dev 010 SDF\">ueLrs</font>"));
        }

        [Test]
        public void DetectDominantScript_ShapedMixed_ReturnsLatin()
        {
            Assert.AreEqual(ScriptType.Latin, ScriptDetector.DetectDominantScript("Hello <font=\"Sai-Sai SDF\">tíªfF«</font>"));
        }
    }
}
