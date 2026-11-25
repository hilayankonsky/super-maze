// Assets/Editor/HomeKitInstaller.cs — TEMPORARY editor tool for integrating the SuperMaze_Home_Kit art
// into the Home scene and capturing verification screenshots. Presentation only: it keeps the existing
// buttons and their OnClick listeners, and never touches services, saving or game rules.
// It runs only when a trigger file is written to Temp/claude_homekit_trigger.txt. Remove after approval.
using System;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
static partial class HomeKitInstaller
{
    const string TriggerPath = "Temp/claude_homekit_trigger.txt";
    const string ResultPath = "Temp/claude_homekit_result.txt";
    const string HomeScenePath = "Assets/Scenes/Home.unity";
    const string Kit = "Assets/Materials/HomeUI/ModularV1/";

    static double nextPoll;
    static readonly StringBuilder log = new StringBuilder();

    static HomeKitInstaller()
    {
        EditorApplication.update += Poll;
        KitPlayTest.Init();
    }

    static void Poll()
    {
        if (EditorApplication.timeSinceStartup < nextPoll) return;
        nextPoll = EditorApplication.timeSinceStartup + 1.0;
        if (!File.Exists(TriggerPath)) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;

        string[] parts = File.ReadAllText(TriggerPath).Trim().Split('|');
        File.Delete(TriggerPath);
        log.Clear();
        try
        {
            switch (parts[0])
            {
                case "import": Import(); break;
                case "dump": Dump(); break;
                case "console": DumpConsole(); break;
                case "apply": Apply(); break;
                case "icon": SetAppIcon(); break;
                case "capture": CaptureAll(parts[1]); break;
                case "playtest": KitPlayTest.Start(parts[1]); Log("playtest started"); break;
                default: Log("unknown command " + parts[0]); break;
            }
        }
        catch (Exception e)
        {
            Log("EXCEPTION: " + e);
        }
        Log(ConsoleCounts());
        File.WriteAllText(ResultPath, log.ToString());
    }

    static void Log(string s) => log.AppendLine(s);

    public static string ConsoleCounts()
    {
        var t = typeof(EditorWindow).Assembly.GetType("UnityEditor.LogEntries");
        var m = t?.GetMethod("GetCountsByType", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        if (m == null) return "console counts unavailable";
        var args = new object[] { 0, 0, 0 };
        m.Invoke(null, args);
        return "console: errors=" + args[0] + " warnings=" + args[1] + " logs=" + args[2];
    }

    // Lists the current Console entries (internal editor API via reflection; read-only).
    static void DumpConsole()
    {
        var asm = typeof(EditorWindow).Assembly;
        var le = asm.GetType("UnityEditor.LogEntries");
        var entryType = asm.GetType("UnityEditor.LogEntry");
        int count = (int)le.GetMethod("StartGettingEntries").Invoke(null, null);
        try
        {
            var entry = Activator.CreateInstance(entryType);
            var get = le.GetMethod("GetEntryInternal");
            for (int i = 0; i < count; i++)
            {
                get.Invoke(null, new object[] { i, entry });
                string msg = (string)entryType.GetField("message").GetValue(entry);
                int mode = (int)entryType.GetField("mode").GetValue(entry);
                Log("[" + i + " mode=" + mode + "] " + msg.Split('\n')[0]);
            }
        }
        finally { le.GetMethod("EndGettingEntries").Invoke(null, null); }
    }

    // ------------------------------------------------------------------ import settings

    // 9-slice borders measured from the kit files (pixels where the rounded ends become flat).
    static Vector4 BorderFor(string path)
    {
        if (path.EndsWith("UI/CreamBar.png")) return new Vector4(60, 60, 60, 60);
        if (path.EndsWith("UI/NumberPill.png")) return new Vector4(70, 70, 70, 70);
        if (path.EndsWith("UI/StarPlate.png")) return new Vector4(72, 72, 72, 72);
        if (path.EndsWith("UI/PressedYellow.png")) return new Vector4(80, 80, 80, 80);
        return Vector4.zero;
    }

    static readonly string[] ExtraSprites =
    {
        "Assets/Materials/Logo/SuperMazeLogo.png",
        "Assets/Materials/SettingsUI/SettingsHeaderArch.png",
        "Assets/Materials/SettingsUI/CloseButtonX.png",
        "Assets/Materials/SettingsUI/VibrationIconNavy.png",
        "Assets/Materials/SettingsUI/SoundIconNavy.png",
    };
    static readonly string[] AppIcons =
    {
        "Assets/Materials/Logo/AppIcon.png",
        "Assets/Materials/Logo/AppIconBackground.png", "Assets/Materials/Logo/AppIconForeground.png",
    };

    static void Import()
    {
        AssetDatabase.Refresh();
        var paths = Directory.GetFiles(Kit, "*.png", SearchOption.AllDirectories).Select(p => p.Replace('\\', '/')).OrderBy(p => p).ToList();
        foreach (var p in paths.Concat(ExtraSprites)) ImportSprite(p);
        foreach (var p in AppIcons)
        {
            // App icons stay plain textures; Unity resizes them per launcher size at build time.
            var ti = AssetImporter.GetAtPath(p) as TextureImporter;
            if (ti == null) { Log("missing importer " + p); continue; }
            ti.textureType = TextureImporterType.Default;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.npotScale = TextureImporterNPOTScale.None;
            ti.maxTextureSize = 1024;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.SaveAndReimport();
            Log(p + " -> icon texture");
        }
    }

    static void ImportSprite(string p)
    {
        {
            var ti = AssetImporter.GetAtPath(p) as TextureImporter;
            if (ti == null) { Log("missing importer " + p); return; }
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.alphaSource = TextureImporterAlphaSource.FromInput;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.maxTextureSize = 2048; // every file is <= 1800 px (PathMap), so this never downsizes
            ti.textureCompression = TextureImporterCompression.CompressedHQ;
            var settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; // keep full canvas (required for Path)
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            ti.SetTextureSettings(settings);
            ti.spriteBorder = BorderFor(p);
            ti.SaveAndReimport();
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(p);
            Log(p + " -> " + (s == null ? "NO SPRITE" : s.rect.width + "x" + s.rect.height + " border=" + s.border));
        }
    }

    // ------------------------------------------------------------------ dump

    static void Dump()
    {
        if (!PrepareScene()) return;
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            DumpNode(root.transform, 0);
    }

    static void DumpNode(Transform t, int depth)
    {
        var sb = new StringBuilder();
        sb.Append(new string(' ', depth * 2)).Append(t.name).Append(t.gameObject.activeSelf ? "" : " [inactive]");
        if (t is RectTransform rt)
            sb.Append($"  a=({rt.anchorMin.x:0.##},{rt.anchorMin.y:0.##})-({rt.anchorMax.x:0.##},{rt.anchorMax.y:0.##}) pos={rt.anchoredPosition} size={rt.sizeDelta} scale={rt.localScale.x:0.##}");
        foreach (var c in t.GetComponents<Component>())
        {
            if (c == null) { sb.Append(" [MISSING]"); continue; }
            if (c is Transform) continue;
            sb.Append(" <").Append(c.GetType().Name);
            if (c is Image img) sb.Append(":").Append(img.sprite ? img.sprite.name : "none").Append(img.raycastTarget ? ",rc" : "");
            if (c is TMP_Text tx) sb.Append(":'").Append(tx.text).Append("'");
            if (c is Button b)
            {
                for (int i = 0; i < b.onClick.GetPersistentEventCount(); i++)
                    sb.Append(" ").Append(b.onClick.GetPersistentTarget(i) ? b.onClick.GetPersistentTarget(i).GetType().Name : "null").Append(".").Append(b.onClick.GetPersistentMethodName(i));
                sb.Append(" tr=").Append(b.transition).Append(" tg=").Append(b.targetGraphic ? b.targetGraphic.name : "none");
            }
            sb.Append(">");
        }
        Log(sb.ToString());
        foreach (Transform ch in t) DumpNode(ch, depth + 1);
    }

    // ------------------------------------------------------------------ scene helpers

    static bool PrepareScene()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var sc = SceneManager.GetSceneAt(i);
            if (sc.isDirty)
            {
                Log("ABORT: scene '" + sc.path + "' has unsaved changes. Save it first.");
                return false;
            }
        }
        if (SceneManager.GetActiveScene().path != HomeScenePath || SceneManager.sceneCount > 1)
            EditorSceneManager.OpenScene(HomeScenePath, OpenSceneMode.Single);
        return true;
    }
}
