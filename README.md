# SmartFont Universal (Multi-Language Supporter)

A Unity package that automatically resolves TextMesh Pro (TMP) rendering issues for multilingual text by detecting Unicode scripts and applying correct font mappings. It comes completely self-contained with Noto Sans and other high-quality Unicode fonts.

---

## Supported Languages & Scripts

The package currently supports the following target languages out-of-the-box:

| Language | Script Type | Unicode Range | Bundled Font |
|---|---|---|---|
| **English** | Latin | Basic ASCII & Extended | Noto Sans |
| **Malay** | Latin | Basic ASCII & Extended | Noto Sans |
| **Indonesian** | Latin | Basic ASCII & Extended | Noto Sans |
| **Filipino** | Latin | Basic ASCII & Extended | Noto Sans |
| **Tamil** | Tamil | `U+0B80` - `U+0BFF` | Noto Sans Tamil |
| **Hindi** | Devanagari | `U+0900` - `U+097F` | Noto Sans Devanagari |
| **Bengali** | Bengali | `U+0980` - `U+09FF` | Noto Sans Bengali |
| **Kannada** | Kannada | `U+0C80` - `U+0CFF` | Noto Sans Kannada |
| **Malayalam** | Malayalam | `U+0D00` - `U+0D7F` | Noto Sans Malayalam |
| **Thai** | Thai | `U+0E00` - `U+0E7F` | Noto Sans Thai |
| **Korean** | Hangul | `U+AC00` - `U+D7AF` | Sunflower |
| **Chinese** | CJK Unified | `U+4E00` - `U+9FFF` | ZCOOL XiaoWei |

---

## Installation

Add the package via the Unity Package Manager (UPM):

1. Open Unity and go to **Window > Package Manager**.
2. Click the **+** icon in the top-left corner and select **Add package from git URL...**.
3. Paste the repository URL:
   ```txt
   https://github.com/Santhanavel-R/Multi-language-support.git
   ```
4. Click **Add**.

*Once added, the package will automatically compile and generate all required TMP Font Assets and the default Font Database inside the package folder (`Runtime/Resources/`) so it works with zero setup!*

---

## How To Use

### Method 1: The MultiLanguageText Component (Recommended)
Use this if you want to set your text content inside a dedicated component and have it automatically apply the correct font on the TMP Text component.

1. Attach the `MultiLanguageText` component to any GameObject containing a TextMesh Pro component (`TextMeshPro` or `TextMeshProUGUI`).
2. Type or paste your content in the **Text Content** field in the Inspector (supports multi-line text).
3. The component will automatically set the text and apply the correct font in the editor immediately (no need to play the game!).
4. To set text via C# code:
   ```csharp
   using MultiLanguageSupporter;
   using UnityEngine;

   public class Example : MonoBehaviour
   {
       private MultiLanguageText multiLanguageText;

       private void Start()
       {
           multiLanguageText = GetComponent<MultiLanguageText>();
           // Updates the text and instantly applies the correct font!
           multiLanguageText.Text = "வணக்கம்"; 
       }
   }
   ```

### Method 2: The SmartFontApplier Component
Use this if you want to modify text on the standard `TextMeshPro` component directly, and want a background listener to auto-resolve the font.

1. Attach the `SmartFontApplier` component to your TextMesh Pro GameObject.
2. When you modify `textComponent.text` (either via code or typing in the default TMP text input), the applier will detect the change and apply the correct font.

---

## Customizing / Regenerating Fonts
If you want to manually regenerate the TMP font assets or force-refresh the database:
1. In the top menu, go to **Window > SmartFont > Generate Package Assets**.
2. This will reload the raw `.ttf` files, rebuild the dynamic SDF font assets under `Runtime/Resources/Fonts/`, and update the `SmartFontDefaultDatabase.asset`.

---

## How to Add New Languages / Scripts in the Future

The package is designed to be easily extensible. To add support for a new language (e.g. **Arabic**):

1. **Add to Enum**: 
   Open `Runtime/ScriptDetector.cs` and add your script to the `ScriptType` enum:
   ```csharp
   public enum ScriptType
   {
       // ... existing scripts
       Arabic
   }
   ```

2. **Initialize Count**:
   In `ScriptDetector.DetectDominantScript`, initialize the script counter:
   ```csharp
   counts[ScriptType.Arabic] = 0;
   ```

3. **Add Unicode Range Check**:
   In `ScriptDetector.GetCharScriptType`, specify the Unicode block range for the script (e.g. `0x0600` to `0x06FF` for Arabic):
   ```csharp
   // Arabic: U+0600 to U+06FF
   if (code >= 0x0600 && code <= 0x06FF)
       return ScriptType.Arabic;
   ```

4. **Download & Place Font**:
   Download a `.ttf` or `.otf` font file supporting that language (e.g. from Google Fonts) and save it in the `Runtime/Fonts/` folder.

5. **Update Asset Generator**:
   Open `Editor/SmartFontAssetGenerator.cs` and add your font mapping to the `fontFiles` dictionary:
   ```csharp
   { ScriptType.Arabic, "MyArabicFont-Regular.ttf" }
   ```

6. **Generate Assets**:
   Save your code files. In Unity, select **Window > SmartFont > Generate Package Assets** to automatically compile and register the new font asset in the default database!
