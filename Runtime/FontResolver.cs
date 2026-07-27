using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace MultiLanguageSupporter
{
    public static class FontResolver
    {
        private static FontDatabase defaultDatabase;

        public static void SetDefaultDatabase(FontDatabase database)
        {
            defaultDatabase = database;
        }

        public static FontDatabase GetDefaultDatabase()
        {
            if (defaultDatabase == null)
            {
                // Try loading from Resources (project generated)
                defaultDatabase = Resources.Load<FontDatabase>("SmartFontDefaultDatabase");
                if (defaultDatabase == null)
                {
                    // Fall back to package pre-built
                    defaultDatabase = Resources.Load<FontDatabase>("SmartFontPackageDatabase");
                }
            }
            return defaultDatabase;
        }

        public static void ResolveAndApply(TMP_Text textComponent, FontDatabase database = null, string originalText = null)
        {
            if (textComponent == null) return;

            if (database == null)
            {
                database = GetDefaultDatabase();
            }

            string text = !string.IsNullOrEmpty(originalText) ? originalText : textComponent.text;
            if (string.IsNullOrEmpty(text)) return;

            ScriptType dominant = ScriptDetector.DetectDominantScript(text);
            
            // Priority 1: Preferred Font (from database)
            TMP_FontAsset resolvedFont = null;
            if (database != null)
            {
                resolvedFont = database.GetFontForScript(dominant);
            }

            // Priority 2: Script Fallback (check preferred font's fallback list)
            if (resolvedFont != null && !IsFontAssetHealthy(resolvedFont))
            {
                resolvedFont = GetHealthyFallback(resolvedFont);
            }

            // Priority 3: Global Fallback (package database)
            if (resolvedFont == null || !IsFontAssetHealthy(resolvedFont))
            {
                var pkgDb = Resources.Load<FontDatabase>("SmartFontPackageDatabase");
                if (pkgDb != null)
                {
                    resolvedFont = pkgDb.GetFontForScript(dominant);
                    if (resolvedFont != null && !IsFontAssetHealthy(resolvedFont))
                    {
                        resolvedFont = GetHealthyFallback(resolvedFont);
                    }
                }
            }

            // Priority 4: TMP Default Font (if healthy)
            if (resolvedFont == null || !IsFontAssetHealthy(resolvedFont))
            {
                resolvedFont = textComponent.font;
            }

            // Apply font if healthy
            if (resolvedFont != null && IsFontAssetHealthy(resolvedFont))
            {
                textComponent.font = resolvedFont;
            }
            else
            {
                Debug.LogWarning($"[SmartFont] Failed to resolve a healthy font for script {dominant}. Keeping default font.");
            }
        }

        private static TMP_FontAsset GetHealthyFallback(TMP_FontAsset font)
        {
            if (font == null) return null;

            if (font.fallbackFontAssetTable != null)
            {
                foreach (var fallback in font.fallbackFontAssetTable)
                {
                    if (IsFontAssetHealthy(fallback))
                    {
                        return fallback;
                    }
                }
            }
            return null;
        }

        public static bool IsFontAssetHealthy(TMP_FontAsset font)
        {
            if (font == null) return false;

            // 1. Source TTF exists (Only required if dynamic)
            if (font.atlasPopulationMode == AtlasPopulationMode.Dynamic && font.sourceFontFile == null)
            {
                return false;
            }

            // 2. Atlas texture exists
            if (font.atlasTextures == null || font.atlasTextures.Length == 0 || font.atlasTextures[0] == null)
            {
                return false;
            }

            // 3. Material exists
            if (font.material == null)
            {
                return false;
            }

            // 4. Character Table & Glyph Table & Lookup Tables
            if (font.characterTable == null || font.glyphTable == null)
            {
                return false;
            }

            return true;
        }
    }
}
