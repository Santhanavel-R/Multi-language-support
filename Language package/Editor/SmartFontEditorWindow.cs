using UnityEngine;
using UnityEditor;
using TMPro;
using System.Collections.Generic;
using SmartFont.Universal;

namespace SmartFont.Universal.Editor
{
    public class SmartFontEditorWindow : EditorWindow
    {
        private FontDatabase currentDatabase;
        private string textToScan = "";
        private ScriptType detectedScript = ScriptType.Unknown;

        [MenuItem("Window/SmartFont/SmartFont Editor & Validator")]
        public static void ShowWindow()
        {
            GetWindow<SmartFontEditorWindow>("SmartFont Editor");
        }

        private void OnGUI()
        {
            GUILayout.Label("SmartFont Universal Settings", EditorStyles.boldLabel);

            currentDatabase = (FontDatabase)EditorGUILayout.ObjectField("Font Database", currentDatabase, typeof(FontDatabase), false);

            if (currentDatabase == null)
            {
                EditorGUILayout.HelpBox("Please assign or create a FontDatabase ScriptableObject.", MessageType.Warning);
                if (GUILayout.Button("Create New Font Database"))
                {
                    CreateFontDatabaseAsset();
                }
                return;
            }

            EditorGUILayout.Space();
            GUILayout.Label("Configure Font Mappings", EditorStyles.boldLabel);

            foreach (ScriptType script in System.Enum.GetValues(typeof(ScriptType)))
            {
                if (script == ScriptType.Unknown) continue;

                TMP_FontAsset currentFont = currentDatabase.GetFontForScript(script);
                TMP_FontAsset newFont = (TMP_FontAsset)EditorGUILayout.ObjectField(script.ToString(), currentFont, typeof(TMP_FontAsset), false);

                if (newFont != currentFont)
                {
                    currentDatabase.SetFontForScript(script, newFont);
                    EditorUtility.SetDirty(currentDatabase);
                }
            }

            EditorGUILayout.Space();
            GUILayout.Label("Script Detector Sandbox", EditorStyles.boldLabel);
            textToScan = EditorGUILayout.TextField("Test Text", textToScan);

            if (GUILayout.Button("Detect Script"))
            {
                detectedScript = ScriptDetector.DetectDominantScript(textToScan);
            }

            EditorGUILayout.LabelField("Detected Script Type", detectedScript.ToString());

            EditorGUILayout.Space();
            GUILayout.Label("Scene Scanner & Fixer", EditorStyles.boldLabel);
            if (GUILayout.Button("Scan & Auto-Fix Active Scene TMP Components"))
            {
                ScanAndFixScene();
            }
        }

        private void CreateFontDatabaseAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create Font Database", "FontDatabase", "asset", "Save Font Database");
            if (string.IsNullOrEmpty(path)) return;

            FontDatabase asset = ScriptableObject.CreateInstance<FontDatabase>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            currentDatabase = asset;
        }

        private void ScanAndFixScene()
        {
            if (currentDatabase == null)
            {
                Debug.LogError("[SmartFont] No FontDatabase loaded to fix scene.");
                return;
            }

            TMP_Text[] textComponents = FindObjectsOfType<TMP_Text>();
            int fixedCount = 0;

            foreach (var comp in textComponents)
            {
                // Verify if it has applier, if not, add one
                SmartFontApplier applier = comp.GetComponent<SmartFontApplier>();
                if (applier == null)
                {
                    applier = comp.gameObject.AddComponent<SmartFontApplier>();
                    Undo.RegisterCreatedObjectUndo(applier, "Add SmartFont Applier");
                }
                
                applier.Resolve();
                fixedCount++;
            }

            Debug.Log($"[SmartFont] Scanned and updated {fixedCount} TMP text components in the scene.");
        }
    }
}
