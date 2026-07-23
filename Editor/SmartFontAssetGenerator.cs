using UnityEditor;
using UnityEngine;
using TMPro;
using System.IO;
using System.Collections.Generic;

namespace MultiLanguageSupporter.Editor
{
    [InitializeOnLoad]
    public static class SmartFontAssetGenerator
    {
        private const string PackagePath = "Packages/com.smartfont.universal";
        private const string RuntimePath = "Runtime";

        static SmartFontAssetGenerator()
        {
            // Delay the call so that the AssetDatabase is fully loaded and ready
            EditorApplication.delayCall += AutoGenerateIfNeeded;
        }

        private static void AutoGenerateIfNeeded()
        {
            string dbPath = $"{PackagePath}/{RuntimePath}/Resources/SmartFontDefaultDatabase.asset";
            FontDatabase database = AssetDatabase.LoadAssetAtPath<FontDatabase>(dbPath);
            if (database == null || database.IsEmpty)
            {
                Debug.Log("[SmartFont] Default database is missing or empty. Starting automatic generation of package assets...");
                Generate();
            }
        }
        
        [MenuItem("Window/SmartFont/Generate Package Assets")]
        public static void Generate()
        {
            Debug.Log("[SmartFont] Starting package asset generation...");

            // Create directories using AssetDatabase
            if (!AssetDatabase.IsValidFolder($"{PackagePath}/{RuntimePath}/Resources"))
            {
                AssetDatabase.CreateFolder($"{PackagePath}/{RuntimePath}", "Resources");
            }
            if (!AssetDatabase.IsValidFolder($"{PackagePath}/{RuntimePath}/Resources/Fonts"))
            {
                AssetDatabase.CreateFolder($"{PackagePath}/{RuntimePath}/Resources", "Fonts");
            }

            AssetDatabase.Refresh();

            // Source TTF/OTF files
            var fontFiles = new Dictionary<ScriptType, string>
            {
                { ScriptType.Latin, "NotoSans-Regular.ttf" },
                { ScriptType.Tamil, "NotoSansTamil-Regular.ttf" },
                { ScriptType.Hindi, "NotoSansDevanagari-Regular.ttf" },
                { ScriptType.Bengali, "NotoSansBengali-Regular.ttf" },
                { ScriptType.Kannada, "NotoSansKannada-Regular.ttf" },
                { ScriptType.Malayalam, "NotoSansMalayalam-Regular.ttf" },
                { ScriptType.Thai, "NotoSansThai-Regular.ttf" },
                { ScriptType.Chinese, "ZCOOLXiaoWei-Regular.ttf" },
                { ScriptType.Korean, "Sunflower-Medium.ttf" }
            };

            // Map to store generated TMP Font Assets
            var generatedAssets = new Dictionary<ScriptType, TMP_FontAsset>();

            foreach (var kvp in fontFiles)
            {
                ScriptType script = kvp.Key;
                string ttfName = kvp.Value;
                
                string ttfPath = $"{PackagePath}/{RuntimePath}/Fonts/{ttfName}";
                Font ttfFont = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);

                if (ttfFont == null)
                {
                    Debug.LogError($"[SmartFont] Failed to load source TTF font at: {ttfPath}");
                    continue;
                }

                string assetPath = $"{PackagePath}/{RuntimePath}/Resources/Fonts/{Path.GetFileNameWithoutExtension(ttfName)} SDF.asset";
                // Always delete existing asset first to ensure we overwrite and save sub-assets correctly!
                if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath) != null)
                {
                    AssetDatabase.DeleteAsset(assetPath);
                }

                Debug.Log($"[SmartFont] Creating new dynamic TMP Font Asset for {script} from {ttfName}...");
                TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(ttfFont);
                AssetDatabase.CreateAsset(fontAsset, assetPath);
                
                // Attach atlas textures as sub-assets so they are saved to disk!
                if (fontAsset.atlasTextures != null)
                {
                    for (int i = 0; i < fontAsset.atlasTextures.Length; i++)
                    {
                        Texture2D tex = fontAsset.atlasTextures[i];
                        if (tex != null)
                        {
                            tex.name = $"{Path.GetFileNameWithoutExtension(ttfName)} Atlas {i}";
                            AssetDatabase.AddObjectToAsset(tex, fontAsset);
                        }
                    }
                }

                // Attach default material as sub-asset so it is saved to disk!
                if (fontAsset.material != null)
                {
                    fontAsset.material.name = $"{Path.GetFileNameWithoutExtension(ttfName)} Material";
                    AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
                }
                
                AssetDatabase.SaveAssets();
                generatedAssets[script] = fontAsset;
            }

            // Setup fallback chains in Editor
            foreach (var kvp in generatedAssets)
            {
                TMP_FontAsset primaryFont = kvp.Value;
                if (primaryFont == null) continue;

                if (primaryFont.fallbackFontAssetTable == null)
                {
                    primaryFont.fallbackFontAssetTable = new List<TMP_FontAsset>();
                }
                else
                {
                    primaryFont.fallbackFontAssetTable.Clear();
                }

                foreach (var otherKvp in generatedAssets)
                {
                    if (otherKvp.Key == kvp.Key) continue; // Don't add itself as a fallback
                    if (otherKvp.Value != null)
                    {
                        primaryFont.fallbackFontAssetTable.Add(otherKvp.Value);
                    }
                }

                EditorUtility.SetDirty(primaryFont);
            }

            // Create or update FontDatabase
            string dbPath = $"{PackagePath}/{RuntimePath}/Resources/SmartFontDefaultDatabase.asset";
            FontDatabase database = AssetDatabase.LoadAssetAtPath<FontDatabase>(dbPath);

            if (database == null)
            {
                Debug.Log($"[SmartFont] Creating new SmartFontDefaultDatabase at {dbPath}...");
                database = ScriptableObject.CreateInstance<FontDatabase>();
                AssetDatabase.CreateAsset(database, dbPath);
                AssetDatabase.SaveAssets();
            }

            // Configure mappings
            foreach (var kvp in generatedAssets)
            {
                database.SetFontForScript(kvp.Key, kvp.Value);
            }
            
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[SmartFont] Package asset generation completed successfully!");
        }
    }
}
