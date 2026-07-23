using NUnit.Framework;
using UnityEngine;
using TMPro;

namespace SmartFont.Universal.Tests
{
    [TestFixture]
    public class FontResolverTests
    {
        private FontDatabase database;
        private TMP_FontAsset latinFont;
        private TMP_FontAsset tamilFont;

        [SetUp]
        public void Setup()
        {
            database = ScriptableObject.CreateInstance<FontDatabase>();
            latinFont = ScriptableObject.CreateInstance<TMP_FontAsset>();
            tamilFont = ScriptableObject.CreateInstance<TMP_FontAsset>();

            database.SetFontForScript(ScriptType.Latin, latinFont);
            database.SetFontForScript(ScriptType.Tamil, tamilFont);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(database);
            Object.DestroyImmediate(latinFont);
            Object.DestroyImmediate(tamilFont);
        }

        [Test]
        public void FontDatabase_GetFontForScript_ReturnsMappedFont()
        {
            Assert.AreEqual(latinFont, database.GetFontForScript(ScriptType.Latin));
            Assert.AreEqual(tamilFont, database.GetFontForScript(ScriptType.Tamil));
            Assert.IsNull(database.GetFontForScript(ScriptType.Hindi));
        }

        [Test]
        public void FontResolver_ResolvesCorrectFont()
        {
            var go = new GameObject("TestText");
            var textComponent = go.AddComponent<TextMeshPro>();

            textComponent.text = "வணக்கம்"; // Tamil script
            FontResolver.ResolveAndApply(textComponent, database);

            Assert.AreEqual(tamilFont, textComponent.font);

            Object.DestroyImmediate(go);
        }
    }
}
