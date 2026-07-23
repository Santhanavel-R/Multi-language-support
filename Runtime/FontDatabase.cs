using System;
using UnityEngine;
using TMPro;

namespace MultiLanguageSupporter
{
    [CreateAssetMenu(fileName = "FontDatabase", menuName = "SmartFont/Font Database", order = 1)]
    public class FontDatabase : ScriptableObject
    {
        [Serializable]
        public struct FontMapping
        {
            public ScriptType Script;
            public TMP_FontAsset FontAsset;
        }

        [SerializeField]
        private FontMapping[] mappings = new FontMapping[0];

        public bool IsEmpty => mappings == null || mappings.Length == 0;

        public TMP_FontAsset GetFontForScript(ScriptType script)
        {
            if (mappings == null) return null;
            for (int i = 0; i < mappings.Length; i++)
            {
                if (mappings[i].Script == script)
                {
                    return mappings[i].FontAsset;
                }
            }
            return null;
        }

        public void SetFontForScript(ScriptType script, TMP_FontAsset fontAsset)
        {
            if (mappings == null) mappings = new FontMapping[0];
            for (int i = 0; i < mappings.Length; i++)
            {
                if (mappings[i].Script == script)
                {
                    mappings[i] = new FontMapping { Script = script, FontAsset = fontAsset };
                    return;
                }
            }
            var list = new System.Collections.Generic.List<FontMapping>(mappings);
            list.Add(new FontMapping { Script = script, FontAsset = fontAsset });
            mappings = list.ToArray();
        }
    }
}
