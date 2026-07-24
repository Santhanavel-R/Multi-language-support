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
            // Disabled automatic generation on assembly reload to prevent startup crash loop
            // EditorApplication.delayCall += AutoGenerateIfNeeded;
        }

        private static bool IsPackageMutable()
        {
            try
            {
                string testPath = $"{PackagePath}/{RuntimePath}/Resources/write_test.txt";
                string physicalPath = Path.GetFullPath(testPath);
                string dir = Path.GetDirectoryName(physicalPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllText(physicalPath, "test");
                File.Delete(physicalPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void AutoGenerateIfNeeded()
        {
            string dbPath = $"{PackagePath}/{RuntimePath}/Resources/SmartFontDefaultDatabase.asset";
            FontDatabase database = AssetDatabase.LoadAssetAtPath<FontDatabase>(dbPath);
            if (database == null || database.IsEmpty)
            {
                string localDbPath = "Assets/SmartFont/Resources/SmartFontDefaultDatabase.asset";
                FontDatabase localDb = AssetDatabase.LoadAssetAtPath<FontDatabase>(localDbPath);
                if (localDb == null || localDb.IsEmpty)
                {
                    Debug.Log("[SmartFont] Default database is missing or empty. Please run Window > SmartFont > Generate Package Assets manually.");
                }
            }
        }
        
        [MenuItem("Window/SmartFont/Generate Package Assets")]
        public static void Generate()
        {
            Debug.Log("[SmartFont] Starting package asset generation...");

            bool usePackage = IsPackageMutable();
            string targetFolder;
            string targetDbPath;

            if (usePackage)
            {
                targetFolder = $"{PackagePath}/{RuntimePath}/Resources/Fonts";
                targetDbPath = $"{PackagePath}/{RuntimePath}/Resources/SmartFontDefaultDatabase.asset";
                
                if (!AssetDatabase.IsValidFolder($"{PackagePath}/{RuntimePath}/Resources"))
                {
                    AssetDatabase.CreateFolder($"{PackagePath}/{RuntimePath}", "Resources");
                }
                if (!AssetDatabase.IsValidFolder($"{PackagePath}/{RuntimePath}/Resources/Fonts"))
                {
                    AssetDatabase.CreateFolder($"{PackagePath}/{RuntimePath}/Resources", "Fonts");
                }
                Debug.Log("[SmartFont] Package is mutable. Generating assets directly inside the package folder.");
            }
            else
            {
                targetFolder = "Assets/SmartFont/Resources/Fonts";
                targetDbPath = "Assets/SmartFont/Resources/SmartFontDefaultDatabase.asset";

                if (!Directory.Exists(Path.GetFullPath("Assets/SmartFont/Resources/Fonts")))
                {
                    Directory.CreateDirectory(Path.GetFullPath("Assets/SmartFont/Resources/Fonts"));
                }
                Debug.Log("[SmartFont] Package is read-only (immutable). Generating assets inside the project's 'Assets/SmartFont' folder instead.");
            }

            AssetDatabase.Refresh();

            // Source TTF/OTF files
            var fontFiles = new Dictionary<ScriptType, string>
            {
                { ScriptType.Latin, "NotoSans-Regular.ttf" },
                { ScriptType.Tamil, "Sai-Sai.ttf" },
                { ScriptType.Hindi, "Kruti Dev 010.ttf" },
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

                string assetPath = $"{targetFolder}/{Path.GetFileNameWithoutExtension(ttfName)} SDF.asset";
                TMP_FontAsset existingAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
                
                // Health check: check if the asset is already healthy
                bool isValid = existingAsset != null &&
                               existingAsset.atlasTextures != null &&
                               existingAsset.atlasTextures.Length > 0 &&
                               existingAsset.atlasTextures[0] != null &&
                               existingAsset.atlasTextures[0].width == 1024 &&
                               existingAsset.material != null;

                if (isValid)
                {
                    Debug.Log($"[SmartFont] Healthy dynamic font asset already exists for {script} at {assetPath}, skipping generation.");
                    generatedAssets[script] = existingAsset;
                    // Force reimport to ensure the Unity Project window refreshes and shows the foldout arrow
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                    continue;
                }

                if (existingAsset != null)
                {
                    AssetDatabase.DeleteAsset(assetPath);
                }

                Debug.Log($"[SmartFont] Creating new dynamic TMP Font Asset for {script} from {ttfName}...");
                TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
                    ttfFont,
                    90, // samplingPointSize
                    9,  // atlasPadding
                    UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
                    1024, // atlasWidth
                    1024, // atlasHeight
                    AtlasPopulationMode.Dynamic,
                    true // enableMultiAtlasSupport
                );

                // Ensure atlas texture is properly formatted
                if (fontAsset.atlasTextures != null && fontAsset.atlasTextures.Length > 0)
                {
                    Texture2D tex = fontAsset.atlasTextures[0];
                    if (tex != null)
                    {
                        if (tex.width == 0 || tex.height == 0 || tex.width != 1024 || tex.height != 1024)
                        {
                            tex.Resize(1024, 1024);
                            tex.Apply(false);
                        }
                        tex.name = $"{Path.GetFileNameWithoutExtension(ttfName)} Atlas 0";
                        tex.hideFlags = HideFlags.None;
                    }
                }

                // Ensure default material is properly formatted
                if (fontAsset.material != null)
                {
                    fontAsset.material.name = $"{Path.GetFileNameWithoutExtension(ttfName)} Material";
                    fontAsset.material.hideFlags = HideFlags.None;
                }

                // Create main asset container on disk. Because fontAsset references the texture and material,
                // and their hideFlags are set to None, Unity's CreateAsset will automatically embed them
                // as visible sub-assets in the same file without needing AddObjectToAsset.
                // This avoids any native serialization double-bind crashes and keeps references 100% correct!
                AssetDatabase.CreateAsset(fontAsset, assetPath);
                
                EditorUtility.SetDirty(fontAsset);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate); // Refresh UI foldout arrow
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
            string dbPath = targetDbPath;
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

        [MenuItem("Window/SmartFont/Clear Package Cache & Resolve")]
        public static void ClearCacheAndResolve()
        {
            Debug.Log("[SmartFont] Clearing PackageCache for com.smartfont.universal...");
            
            try
            {
                string cachePath = Path.GetFullPath("Library/PackageCache");
                if (Directory.Exists(cachePath))
                {
                    string[] dirs = Directory.GetDirectories(cachePath, "com.smartfont.universal@*");
                    foreach (var dir in dirs)
                    {
                        Debug.Log($"[SmartFont] Deleting cache directory: {dir}");
                        var dirInfo = new DirectoryInfo(dir);
                        foreach (var file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                        {
                            file.Attributes = FileAttributes.Normal;
                        }
                        Directory.Delete(dir, true);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[SmartFont] Note: Could not delete cache folder directly (might be locked by Unity): {ex.Message}");
            }

            Debug.Log("[SmartFont] Requesting Unity Package Manager to resolve dependencies...");
            UnityEditor.PackageManager.Client.Resolve();
            Debug.Log("[SmartFont] Resolve request sent. Unity will now fetch the latest commit of the package!");
        }
    }
}
