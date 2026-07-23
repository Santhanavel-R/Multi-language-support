using NUnit.Framework;

namespace SmartFont.Universal.Tests
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
        public void DetectDominantScript_Hindi_ReturnsHindi()
        {
            Assert.AreEqual(ScriptType.Hindi, ScriptDetector.DetectDominantScript("नमस्ते"));
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
    }
}
