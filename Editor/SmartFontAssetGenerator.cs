using UnityEditor;
using UnityEngine;
using TMPro;
using System;
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
                // Resolve the physical path of the package
                string physicalPath = Path.GetFullPath(PackagePath);
                
                // If it is in the Library/PackageCache folder, it is logically immutable in Unity!
                if (physicalPath.Contains("Library/PackageCache") || physicalPath.Contains("Library\\PackageCache"))
                {
                    return false;
                }
                
                string testPath = $"{PackagePath}/{RuntimePath}/Resources/write_test.txt";
                string testPhysicalPath = Path.GetFullPath(testPath);
                string dir = Path.GetDirectoryName(testPhysicalPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllText(testPhysicalPath, "test");
                File.Delete(testPhysicalPath);
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
                string pkgDbPath = $"{PackagePath}/{RuntimePath}/Resources/SmartFontPackageDatabase.asset";
                database = AssetDatabase.LoadAssetAtPath<FontDatabase>(pkgDbPath);
            }
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
                targetFolder = $"{PackagePath}/{RuntimePath}/Resources";
                targetDbPath = $"{PackagePath}/{RuntimePath}/Resources/SmartFontDefaultDatabase.asset";
                
                if (!AssetDatabase.IsValidFolder($"{PackagePath}/{RuntimePath}/Resources"))
                {
                    AssetDatabase.CreateFolder($"{PackagePath}/{RuntimePath}", "Resources");
                }
                Debug.Log("[SmartFont] Package is mutable. Generating assets directly inside the package folder.");
            }
            else
            {
                targetFolder = "Assets/SmartFont/Resources";
                targetDbPath = "Assets/SmartFont/Resources/SmartFontDefaultDatabase.asset";

                if (!Directory.Exists(Path.GetFullPath("Assets/SmartFont/Resources")))
                {
                    Directory.CreateDirectory(Path.GetFullPath("Assets/SmartFont/Resources"));
                }
                Debug.Log("[SmartFont] Package is read-only (immutable). Generating assets inside the project's 'Assets/SmartFont' folder instead.");
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

                string cleanName = Path.GetFileNameWithoutExtension(ttfName)
                    .Replace(" ", "")
                    .Replace("-", "");
                string assetName = $"{cleanName}SDF";
                string assetPath = $"{targetFolder}/{assetName}.asset";
                TMP_FontAsset existingAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
                
                // Health check: check if the asset is already healthy
                bool isValid = existingAsset != null &&
                               existingAsset.sourceFontFile != null &&
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
                Texture2D tex = null;
                if (fontAsset.atlasTextures != null && fontAsset.atlasTextures.Length > 0)
                {
                    tex = fontAsset.atlasTextures[0];
                    if (tex != null)
                    {
                        if (tex.width == 0 || tex.height == 0 || tex.width != 1024 || tex.height != 1024)
                        {
                            tex.Resize(1024, 1024);
                            tex.Apply(false);
                        }
                        tex.name = $"{assetName}Atlas0";
                        tex.hideFlags = HideFlags.None;
                    }
                }

                // Ensure default material is properly formatted
                if (fontAsset.material != null)
                {
                    fontAsset.material.name = $"{assetName}Material";
                    fontAsset.material.hideFlags = HideFlags.None;
                }

                // 1. Extract texture and material references from memory before writing
                tex = (fontAsset.atlasTextures != null && fontAsset.atlasTextures.Length > 0) ? fontAsset.atlasTextures[0] : null;
                Material mat = fontAsset.material;

                // 2. Create the main asset container on disk
                AssetDatabase.CreateAsset(fontAsset, assetPath);

                // 3. Add the texture and material as sub-assets using the safe path-based signature (never crashes!)
                if (tex != null)
                {
                    AssetDatabase.AddObjectToAsset(tex, assetPath);
                    EditorUtility.SetDirty(tex);
                }

                if (mat != null)
                {
                    AssetDatabase.AddObjectToAsset(mat, assetPath);
                    EditorUtility.SetDirty(mat);
                }

                // 3.5 Explicitly bind the texture to the material as a sub-asset reference
                if (mat != null && tex != null)
                {
                    mat.SetTexture("_MainTex", tex);
                    EditorUtility.SetDirty(mat);
                }

                // 4. Force Unity's serialization system to register the references on disk using SerializedObject
                SerializedObject serializedFontAsset = new SerializedObject(fontAsset);
                
                SerializedProperty sourceFontFileProp = serializedFontAsset.FindProperty("m_SourceFontFile");
                if (sourceFontFileProp != null && ttfFont != null)
                {
                    sourceFontFileProp.objectReferenceValue = ttfFont;
                }

                SerializedProperty atlasTexturesProp = serializedFontAsset.FindProperty("m_AtlasTextures");
                if (atlasTexturesProp != null && tex != null)
                {
                    atlasTexturesProp.ClearArray();
                    atlasTexturesProp.InsertArrayElementAtIndex(0);
                    atlasTexturesProp.GetArrayElementAtIndex(0).objectReferenceValue = tex;
                }

                SerializedProperty materialProp = serializedFontAsset.FindProperty("m_Material");
                if (materialProp == null)
                {
                    materialProp = serializedFontAsset.FindProperty("material");
                }
                if (materialProp != null && mat != null)
                {
                    materialProp.objectReferenceValue = mat;
                }

                // Serialize creation settings
                SerializedProperty creationSettingsProp = serializedFontAsset.FindProperty("m_CreationSettings");
                if (creationSettingsProp != null)
                {
                    var sourceFontFileNameProp = creationSettingsProp.FindPropertyRelative("sourceFontFileName");
                    if (sourceFontFileNameProp != null) sourceFontFileNameProp.stringValue = ttfFont.name;

                    string guid;
                    long localId;
                    if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(ttfFont, out guid, out localId))
                    {
                        var sourceFontFileGUIDProp = creationSettingsProp.FindPropertyRelative("sourceFontFileGUID");
                        if (sourceFontFileGUIDProp != null) sourceFontFileGUIDProp.stringValue = guid;
                    }
                    
                    var pointSizeProp = creationSettingsProp.FindPropertyRelative("pointSize");
                    if (pointSizeProp != null) pointSizeProp.intValue = 90;

                    var paddingProp = creationSettingsProp.FindPropertyRelative("padding");
                    if (paddingProp != null) paddingProp.intValue = 9;

                    var atlasWidthProp = creationSettingsProp.FindPropertyRelative("atlasWidth");
                    if (atlasWidthProp != null) atlasWidthProp.intValue = 1024;

                    var atlasHeightProp = creationSettingsProp.FindPropertyRelative("atlasHeight");
                    if (atlasHeightProp != null) atlasHeightProp.intValue = 1024;

                    var renderModeProp = creationSettingsProp.FindPropertyRelative("renderMode");
                    if (renderModeProp != null) renderModeProp.intValue = (int)UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA;
                    var includeFeaturesProp = creationSettingsProp.FindPropertyRelative("includeFontFeatures");
                    if (includeFeaturesProp != null) includeFeaturesProp.boolValue = true;
                }

                // Clear dynamic data on build to keep the build sizes optimized and clean in git
                SerializedProperty clearDynamicDataProp = serializedFontAsset.FindProperty("m_ClearDynamicDataOnBuild");
                if (clearDynamicDataProp != null)
                {
                    clearDynamicDataProp.boolValue = true;
                }

                serializedFontAsset.ApplyModifiedProperties();
                
                // Call internal initialization methods to ensure the asset is fully loaded and lookup tables are populated
                InitializeFontAsset(fontAsset);

                // Populate the font asset with the script's characters so glyph and character tables are filled
                try
                {
                    PopulateFontAssetCharacters(fontAsset, ttfFont, script);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[SmartFont] PopulateFontAssetCharacters failed for {assetName}: {e.Message}");
                }

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
            Debug.Log("[SmartFont] Deleting packages-lock.json to unlock git dependencies...");
            try
            {
                string lockFilePath = Path.GetFullPath("Packages/packages-lock.json");
                if (File.Exists(lockFilePath))
                {
                    File.Delete(lockFilePath);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[SmartFont] Note: Could not delete packages-lock.json: {ex.Message}");
            }

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

        private static void InitializeFontAsset(TMP_FontAsset fontAsset)
        {
            if (fontAsset == null) return;
            
            // Invoke internal ReadFontAssetDefinition method
            var readMethod = typeof(TMP_FontAsset).GetMethod("ReadFontAssetDefinition", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (readMethod != null)
            {
                try { readMethod.Invoke(fontAsset, null); } catch (System.Exception e) { Debug.LogWarning($"[SmartFont] ReadFontAssetDefinition failed: {e.Message}"); }
            }
            
            // Invoke internal InitializeLookupTables method
            var initLookupMethod = typeof(TMP_FontAsset).GetMethod("InitializeLookupTables", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (initLookupMethod != null)
            {
                try { initLookupMethod.Invoke(fontAsset, null); } catch (System.Exception e) { Debug.LogWarning($"[SmartFont] InitializeLookupTables failed: {e.Message}"); }
            }

            // NOTE: Do NOT clear font asset data here. Clearing will remove populated
            // glyph and character tables which are required for correct rendering of
            // complex scripts (Indic, Thai, etc.). Populating characters is handled
            // after asset creation.
        }

        private static void PopulateFontAssetCharacters(TMP_FontAsset fontAsset, Font sourceFont, ScriptType script)
        {
            if (fontAsset == null || sourceFont == null) return;

            (int start, int end) range = (0, 0);
            switch (script)
            {
                case ScriptType.Tamil: range = (0x0B80, 0x0BFF); break;
                case ScriptType.Hindi: range = (0x0900, 0x097F); break;
                case ScriptType.Bengali: range = (0x0980, 0x09FF); break;
                case ScriptType.Kannada: range = (0x0C80, 0x0CFF); break;
                case ScriptType.Malayalam: range = (0x0D00, 0x0D7F); break;
                case ScriptType.Thai: range = (0x0E00, 0x0E7F); break;
                case ScriptType.Chinese: range = (0x4E00, 0x9FFF); break;
                case ScriptType.Korean: range = (0xAC00, 0xD7AF); break;
                default: range = (0x0020, 0x007E); break; // Basic Latin
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int code = range.start; code <= range.end; code++)
            {
                // Only add BMP characters (most target scripts are in BMP)
                if (code > 0xFFFF) continue;
                char ch = (char)code;
                try
                {
                    if (sourceFont.HasCharacter(ch))
                    {
                        sb.Append(ch);
                    }
                }
                catch { }
                // Keep batches reasonably sized
                if (sb.Length > 512)
                {
                    fontAsset.TryAddCharacters(sb.ToString());
                    sb.Clear();
                }
            }

            if (sb.Length > 0)
            {
                fontAsset.TryAddCharacters(sb.ToString());
            }
        }

        [MenuItem("Window/SmartFont/Generate Script Coverage Report")]
        public static void GenerateCoverageReport()
        {
            Debug.Log("[SmartFont] Generating Script Coverage Report...");
            FontDatabase database = FontResolver.GetDefaultDatabase();
            if (database == null)
            {
                Debug.LogError("[SmartFont] No active FontDatabase found. Cannot generate coverage report.");
                return;
            }

            var ranges = new Dictionary<ScriptType, (int start, int end)>
            {
                { ScriptType.Latin, (0x0041, 0x007A) },
                { ScriptType.Tamil, (0x0B80, 0x0BFF) },
                { ScriptType.Hindi, (0x0900, 0x097F) },
                { ScriptType.Bengali, (0x0980, 0x09FF) },
                { ScriptType.Kannada, (0x0C80, 0x0CFF) },
                { ScriptType.Malayalam, (0x0D00, 0x0D7F) },
                { ScriptType.Thai, (0x0E00, 0x0E7F) }
            };

            foreach (var kvp in ranges)
            {
                ScriptType script = kvp.Key;
                var range = kvp.Value;
                TMP_FontAsset fontAsset = database.GetFontForScript(script);

                if (fontAsset == null || fontAsset.sourceFontFile == null)
                {
                    Debug.LogWarning($"[SmartFont] {script}: Font or Source Font File is missing.");
                    continue;
                }

                int total = 0;
                int supported = 0;

                for (int code = range.start; code <= range.end; code++)
                {
                    total++;
                    if (fontAsset.sourceFontFile.HasCharacter((char)code))
                    {
                        supported++;
                    }
                }

                double pct = (double)supported / total * 100.0;
                Debug.Log($"[SmartFont] {script} Coverage: {pct:F1}% ({supported}/{total} characters supported by source font '{fontAsset.sourceFontFile.name}')");
            }
        }

        [MenuItem("Window/SmartFont/Repopulate Existing Font Assets")]
        public static void RepopulateExistingAssets()
        {
            Debug.Log("[SmartFont] Repopulating existing SmartFont TMP assets from FontDatabase...");

            // Try load default database from Resources
            FontDatabase database = Resources.Load<FontDatabase>("SmartFontDefaultDatabase");
            if (database == null)
            {
                database = Resources.Load<FontDatabase>("SmartFontPackageDatabase");
            }

            if (database == null || database.IsEmpty)
            {
                Debug.LogWarning("[SmartFont] No FontDatabase found in Resources. Cannot repopulate assets.");
                return;
            }

            int processed = 0;
            // For each mapping in the database, populate the mapped TMP_FontAsset
            foreach (ScriptType script in Enum.GetValues(typeof(ScriptType)))
            {
                TMP_FontAsset fontAsset = database.GetFontForScript(script);
                if (fontAsset == null) continue;

                Font source = fontAsset.sourceFontFile;
                if (source == null)
                {
                    Debug.LogWarning($"[SmartFont] Font asset '{fontAsset.name}' for script {script} has no source font file. Skipping.");
                    continue;
                }

                try
                {
                    PopulateFontAssetCharacters(fontAsset, source, script);
                    EditorUtility.SetDirty(fontAsset);
                    string path = AssetDatabase.GetAssetPath(fontAsset);
                    if (!string.IsNullOrEmpty(path))
                    {
                        AssetDatabase.SaveAssets();
                        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    }
                    processed++;
                    Debug.Log($"[SmartFont] Repopulated '{fontAsset.name}' for script {script}.");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[SmartFont] Failed to repopulate '{fontAsset.name}': {e.Message}");
                }
            }

            Debug.Log($"[SmartFont] Repopulation complete. Processed {processed} font assets.");
        }

        [MenuItem("Window/SmartFont/Toggle Verbose Diagnostics")]
        public static void ToggleVerboseDiagnostics()
        {
            bool current = EditorPrefs.GetBool("SmartFont_VerboseDiagnostics", false);
            EditorPrefs.SetBool("SmartFont_VerboseDiagnostics", !current);
            Debug.Log($"[SmartFont] Verbose Diagnostics is now {(!current ? "DISABLED" : "ENABLED")}.");
        }
    }
}
