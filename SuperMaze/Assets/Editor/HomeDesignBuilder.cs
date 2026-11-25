// Assets/Editor/HomeDesignBuilder.cs — TEMPORARY editor tool used to apply the Home/Settings
// visual redesign and capture verification screenshots. Visual changes only: it never touches
// OnClick listeners, services, saving or game logic. Remove after the redesign is approved.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
static class HomeDesignBuilder
{
    const string TriggerPath = "Temp/claude_home_design_trigger.txt";
    const string ResultPath = "Temp/claude_home_design_result.txt";
    const string HomeScenePath = "Assets/Scenes/Home.unity";
    const string HomeUI = "Assets/Materials/HomeUI/";
    const string SettingsUI = "Assets/Materials/SettingsUI/";
    const string FontPath = "Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Roboto-Bold SDF.asset";

    static readonly Color Navy = new Color32(0x1E, 0x2F, 0x5E, 0xFF);
    static readonly Color White = Color.white;

    static readonly Dictionary<string, Vector4> SlicedBorders = new Dictionary<string, Vector4>
    {
        { HomeUI + "CreamBar.png", new Vector4(80, 80, 80, 80) },
        { HomeUI + "NavPressedHighlight.png", new Vector4(72, 66, 72, 66) },
        { HomeUI + "YellowButton.png", new Vector4(76, 76, 76, 76) },
        { HomeUI + "NumberPill.png", new Vector4(56, 54, 56, 54) },
        { SettingsUI + "SettingsShellBlue.png", new Vector4(130, 130, 130, 130) },
    };

    static double nextPoll;
    static StringBuilder log = new StringBuilder();

    static HomeDesignBuilder()
    {
        EditorApplication.update += Poll;
        PlayTest.Init();
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
                case "refresh":
                    AssetDatabase.Refresh();
                    Log("refreshed");
                    break;
                case "build":
                    Build(parts.Length > 1 ? parts[1] : null);
                    break;
                case "capture":
                    CaptureAll(parts[1]);
                    break;
                case "audit":
                    Audit();
                    break;
                case "playtest":
                    PlayTest.Start(parts[1]);
                    Log("playtest started");
                    break;
            }
        }
        catch (Exception e)
        {
            Log("EXCEPTION: " + e);
        }
        File.WriteAllText(ResultPath, log.ToString());
    }

    static void Log(string s) { log.AppendLine(s); }

    public static string ConsoleCounts()
    {
        var t = typeof(EditorWindow).Assembly.GetType("UnityEditor.LogEntries");
        var m = t?.GetMethod("GetCountsByType", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        if (m == null) return "console counts unavailable";
        var args = new object[] { 0, 0, 0 };
        m.Invoke(null, args);
        return "console: errors=" + args[0] + " warnings=" + args[1] + " logs=" + args[2];
    }

    static void Audit()
    {
        Log("script compilation failed: " + EditorUtility.scriptCompilationFailed);
        Log(ConsoleCounts());
        if (!PrepareScene(out string previous)) return;
        foreach (var path in new[] { "Assets/Scenes/Bootstrap.unity", "Assets/Scenes/Home.unity", "Assets/Scenes/Level.unity" })
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            int missingScripts = 0, brokenRefs = 0;
            foreach (var root in scene.GetRootGameObjects())
            foreach (var tr in root.GetComponentsInChildren<Transform>(true))
            {
                var go = tr.gameObject;
                int m = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
                if (m > 0) { missingScripts += m; Log("  MISSING SCRIPT on " + GetPath(tr)); }
                foreach (var comp in go.GetComponents<Component>())
                {
                    if (comp == null) continue;
                    var so = new SerializedObject(comp);
                    var it = so.GetIterator();
                    bool ours = comp is MonoBehaviour && (comp.GetType().Assembly.GetName().Name == "Game" || comp.GetType().Assembly.GetName().Name == "Assembly-CSharp");
                    while (it.NextVisible(true))
                    {
                        if (it.propertyType != SerializedPropertyType.ObjectReference) continue;
                        if (ours && it.objectReferenceValue == null && it.depth == 0 && it.name != "m_Script")
                            Log("  unassigned " + GetPath(tr) + " " + comp.GetType().Name + "." + it.propertyPath);
                    }
                }
            }
            Log(path + ": missing scripts=" + missingScripts + ", broken references=" + brokenRefs);
        }
        if (!string.IsNullOrEmpty(previous)) EditorSceneManager.OpenScene(previous, OpenSceneMode.Single);
    }

    static string GetPath(Transform t) => t.parent == null ? t.name : GetPath(t.parent) + "/" + t.name;

    // ------------------------------------------------------------------ importers

    static void ConfigureImporters()
    {
        var paths = Directory.GetFiles(HomeUI, "*.png").Select(p => p.Replace('\\', '/')).ToList();
        foreach (var n in new[] { "SettingsShellBlue", "SettingsHeaderBlue", "CloseButtonRed", "ToggleTrackOnBlue", "ToggleTrackOffBlue", "ToggleKnobCream" })
            paths.Add(SettingsUI + n + ".png");

        foreach (var p in paths)
        {
            var ti = AssetImporter.GetAtPath(p) as TextureImporter;
            if (ti == null) { Log("missing importer " + p); continue; }
            bool changed = false;
            if (ti.textureType != TextureImporterType.Sprite) { ti.textureType = TextureImporterType.Sprite; changed = true; }
            if (ti.spriteImportMode != SpriteImportMode.Single) { ti.spriteImportMode = SpriteImportMode.Single; changed = true; }
            if (ti.mipmapEnabled) { ti.mipmapEnabled = false; changed = true; }
            if (!ti.alphaIsTransparency) { ti.alphaIsTransparency = true; changed = true; }
            if (ti.wrapMode != TextureWrapMode.Clamp) { ti.wrapMode = TextureWrapMode.Clamp; changed = true; }
            if (ti.maxTextureSize != 2048) { ti.maxTextureSize = 2048; changed = true; }
            Vector4 border = SlicedBorders.TryGetValue(p, out var b) ? b : Vector4.zero;
            if (ti.spriteBorder != border) { ti.spriteBorder = border; changed = true; }
            if (changed) ti.SaveAndReimport();
        }
    }

    static Sprite Spr(string path)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s == null) Log("MISSING SPRITE " + path);
        return s;
    }

    static Sprite H(string name) => Spr(HomeUI + name + ".png");
    static Sprite St(string name) => Spr(SettingsUI + name + ".png");

    // ------------------------------------------------------------------ helpers

    static RectTransform Child(Transform parent, string name)
    {
        var t = parent.Find(name);
        if (t == null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5;
            go.transform.SetParent(parent, false);
            t = go.transform;
        }
        return (RectTransform)t;
    }

    static RectTransform Req(Transform root, string path)
    {
        var t = root.Find(path);
        if (t == null) throw new Exception("Not found: " + path);
        return (RectTransform)t;
    }

    static RectTransform Either(Transform root, params string[] paths)
    {
        foreach (var p in paths)
        {
            var t = root.Find(p);
            if (t != null) return (RectTransform)t;
        }
        throw new Exception("Not found: " + string.Join(" / ", paths));
    }

    static void SetRect(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = pivot;
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        rt.localRotation = Quaternion.identity; rt.localScale = Vector3.one;
    }

    static void Center(RectTransform rt, Vector2 pos, Vector2 size) =>
        SetRect(rt, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), pos, size);

    static void Stretch(RectTransform rt, float l = 0, float b = 0, float r = 0, float t = 0)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.pivot = new Vector2(.5f, .5f);
        rt.offsetMin = new Vector2(l, b); rt.offsetMax = new Vector2(-r, -t);
        rt.localRotation = Quaternion.identity; rt.localScale = Vector3.one;
    }

    // place by top-left coordinates inside a fixed-size container
    static void Place(RectTransform rt, float x, float y, float w, float h) =>
        SetRect(rt, new Vector2(0, 1), new Vector2(0, 1), new Vector2(.5f, .5f), new Vector2(x + w / 2, -(y + h / 2)), new Vector2(w, h));

    static Image Img(Component c, Sprite s, bool preserve = true, bool raycast = false, Color? color = null)
    {
        var img = c.GetComponent<Image>();
        if (img == null) img = c.gameObject.AddComponent<Image>();
        img.sprite = s;
        img.type = Image.Type.Simple;
        img.preserveAspect = preserve;
        img.raycastTarget = raycast;
        img.color = color ?? White;
        img.enabled = true;
        return img;
    }

    static Image Sliced(Component c, Sprite s, float multiplier, bool raycast = false)
    {
        var img = Img(c, s, false, raycast);
        img.type = Image.Type.Sliced;
        img.fillCenter = true;
        img.pixelsPerUnitMultiplier = multiplier;
        return img;
    }

    static TMP_FontAsset font;

    static TextMeshProUGUI Txt(Component c, string text, float size, Color color, TextAlignmentOptions align = TextAlignmentOptions.Center, bool raycast = false)
    {
        var t = c.GetComponent<TextMeshProUGUI>();
        if (t == null) t = c.gameObject.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        if (text != null) t.text = text;
        t.fontSize = size;
        t.enableAutoSizing = false;
        t.color = color;
        t.alignment = align;
        t.fontStyle = FontStyles.Normal;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Overflow;
        t.raycastTarget = raycast;
        t.characterSpacing = 0;
        t.margin = Vector4.zero;
        return t;
    }

    static void AutoSize(TextMeshProUGUI t, float min, float max)
    {
        t.enableAutoSizing = true; t.fontSizeMin = min; t.fontSizeMax = max;
    }

    // ------------------------------------------------------------------ build

    static bool PrepareScene(out string previousScene)
    {
        previousScene = SceneManager.GetActiveScene().path;
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

    static void Build(string captureDir)
    {
        if (!PrepareScene(out string previous)) return;
        AssetDatabase.Refresh();
        ConfigureImporters();
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (font == null) Log("MISSING FONT");

        var canvas = GameObject.Find("Canvas").transform;
        var safe = Req(canvas, "SafeArea");

        BuildBackground(canvas, safe);
        BuildDecor(safe);
        BuildMap(safe);
        BuildCheckout(safe);
        BuildTopBar(safe);
        BuildHudButtons(safe);
        BuildBottomBar(safe);
        BuildSettings(safe);
        OrderSiblings(safe);

        var scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Log("saved " + scene.path);

        if (!string.IsNullOrEmpty(captureDir)) CaptureAll(captureDir);

        if (!string.IsNullOrEmpty(previous) && previous != HomeScenePath)
            EditorSceneManager.OpenScene(previous, OpenSceneMode.Single);
    }

    static void BuildBackground(Transform canvas, RectTransform safe)
    {
        var bg = Child(canvas, "Background");
        bg.SetSiblingIndex(0);
        Stretch(bg);
        Img(bg, H("FloorGradient"), false);

        var back = Child(canvas, "BackDecor");
        back.SetSiblingIndex(1);
        Stretch(back);

        var vl = Child(back, "VinesLeft");
        SetRect(vl, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(-30, 12), new Vector2(330, 330));
        Img(vl, H("Vines"));
        var vr = Child(back, "VinesRight");
        SetRect(vr, new Vector2(1, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(30, 12), new Vector2(330, 330));
        vr.localScale = new Vector3(-1, 1, 1);
        Img(vr, H("Vines"));

        // SafeArea keeps its Image component; it just becomes transparent so the floor shows.
        var safeImg = safe.GetComponent<Image>();
        if (safeImg != null) safeImg.color = new Color(1, 1, 1, 0);
    }

    static void BuildDecor(RectTransform safe)
    {
        var decor = Child(safe, "Decor");
        Stretch(decor);

        var sl = Child(decor, "ShelfLeft");
        SetRect(sl, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(-12, -168), new Vector2(250, 880));
        Img(sl, H("ShelfProduce"));
        ShelfSign(sl, "PRODUCE", 10f, new Vector2(18, -168));

        var sr = Child(decor, "ShelfRight");
        SetRect(sr, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(12, -168), new Vector2(250, 880));
        Img(sr, H("ShelfBakery"));
        ShelfSign(sr, "BAKERY", -10f, new Vector2(-18, -168));

        var fp = Child(decor, "FlowerPot");
        SetRect(fp, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(-12, 470), new Vector2(135, 228));
        Img(fp, H("FlowerPot"));

        var pr = Child(decor, "PlantRight");
        SetRect(pr, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(8, 585), new Vector2(130, 156));
        Img(pr, H("PottedPlant"));
    }

    static void ShelfSign(RectTransform shelf, string label, float angle, Vector2 pos)
    {
        var sign = Child(shelf, "Sign");
        SetRect(sign, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(.5f, .5f), pos, new Vector2(212, 66));
        sign.localRotation = Quaternion.Euler(0, 0, angle);
        Img(sign, H("ShelfSign"));
        var t = Child(sign, "Label");
        Stretch(t, 12, 6, 12, 8);
        var txt = Txt(t, label, 36, Navy);
        AutoSize(txt, 20, 36);
        txt.characterSpacing = 2;
    }

    static readonly Vector2[] LevelPositions =
    {
        new Vector2(-168, -362), new Vector2(140, -225), new Vector2(-177, -107),
        new Vector2(147, 40), new Vector2(-138, 180), new Vector2(16, 370),
    };

    static void BuildMap(RectTransform safe)
    {
        var area = Child(safe, "MapArea");
        Stretch(area, 0, 540, 0, 160);

        var map = Either(safe, "LevelMap", "MapArea/LevelMap");
        map.SetParent(area, false);
        Center(map, Vector2.zero, new Vector2(940, 1000));
        if (map.GetComponent<UniformScaleFitter>() == null) map.gameObject.AddComponent<UniformScaleFitter>();

        var path = Child(map, "MapPath");
        path.SetSiblingIndex(0);
        Stretch(path);
        Img(path, H("MapPath"), false);

        for (int i = 1; i <= 5; i++)
        {
            var p = map.Find("Path" + i);
            if (p != null) p.gameObject.SetActive(false);
        }

        for (int i = 1; i <= 6; i++)
        {
            var btn = Req(map, "Level" + i + "Button");
            Center(btn, LevelPositions[i - 1], new Vector2(240, 240));
            Img(btn, H("LevelNode" + i), true, true);

            var pill = Child(btn, "NumberPill");
            pill.SetSiblingIndex(0);
            Center(pill, new Vector2(0, -62), new Vector2(96, 62));
            Sliced(pill, H("NumberPill"), 112f / 62f);

            var text = Req(btn, "Level" + i + "Text");
            text.SetAsLastSibling();
            Center(text, new Vector2(0, -60), new Vector2(96, 62));
            Txt(text, null, 44, Navy);
        }
    }

    static void BuildCheckout(RectTransform safe)
    {
        var co = Child(safe, "Checkout");
        SetRect(co, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(0, 145), new Vector2(940, 420));

        var plant = Child(co, "CounterPlant"); Place(plant, 110, 42, 160, 192); Img(plant, H("PottedPlant"));
        var counter = Child(co, "Counter"); Place(counter, 100, 170, 665, 225); Img(counter, H("CheckoutCounter"));
        var bag = Child(co, "ShoppingBag"); Place(bag, 602, 64, 128, 157); Img(bag, H("ShoppingBag"));
        var receipt = Child(co, "ReceiptScreen"); Place(receipt, 290, 30, 270, 200); Img(receipt, H("ReceiptScreen"));
        var rt = Child(receipt, "Title");
        SetRect(rt, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(.5f, .5f), new Vector2(0, -38), new Vector2(200, 34));
        Txt(rt, "RECEIPT", 26, Navy).characterSpacing = 2;
        var term = Child(co, "CardTerminal"); Place(term, 560, 148, 50, 84); Img(term, H("CardTerminal"));
        var basket = Child(co, "BreadBasket"); Place(basket, 765, 170, 160, 216); Img(basket, H("BreadBasket"));

        var sign = Child(co, "CheckoutSign"); Place(sign, 302, 290, 290, 64);
        Sliced(sign, H("NumberPill"), 112f / 64f);
        var st = Child(sign, "Label"); Stretch(st);
        Txt(st, "CHECKOUT", 36, Navy).characterSpacing = 3;
    }

    static readonly Color DividerColor = new Color(0.118f, 0.184f, 0.369f, 0.55f);

    static void BuildTopBar(RectTransform safe)
    {
        var bar = Child(safe, "TopBar");
        SetRect(bar, new Vector2(0, 1), new Vector2(1, 1), new Vector2(.5f, 1), new Vector2(0, -26), new Vector2(-80, 120));
        Sliced(bar, H("CreamBar"), 168f / 120f, true);

        RectTransform AtX(string name, float fx, Vector2 size, float y = 0)
        {
            var r = Child(bar, name);
            SetRect(r, new Vector2(fx, .5f), new Vector2(fx, .5f), new Vector2(.5f, .5f), new Vector2(0, y), size);
            return r;
        }

        // Logo slot: the team's logo art is not in the project yet, so the Image stays disabled.
        var logo = AtX("Logo", .125f, new Vector2(140, 140), 12);
        var logoImg = logo.GetComponent<Image>();
        if (logoImg == null) logoImg = logo.gameObject.AddComponent<Image>();
        logoImg.preserveAspect = true; logoImg.raycastTarget = false;
        logoImg.enabled = logoImg.sprite != null;

        Img(AtX("Divider1", .25f, new Vector2(4, 64)), null, false, false, DividerColor);
        Img(AtX("CoinIcon", .315f, new Vector2(74, 74)), H("CoinIcon"));
        var coins = Either(safe, "CoinsText", "TopBar/CoinsText");
        if (coins.parent != bar) coins.SetParent(bar, false);
        coins.anchorMin = new Vector2(.365f, .12f); coins.anchorMax = new Vector2(.535f, .88f);
        coins.pivot = new Vector2(0, .5f); coins.offsetMin = Vector2.zero; coins.offsetMax = Vector2.zero;
        coins.localScale = Vector3.one; coins.localRotation = Quaternion.identity;
        var ct = Txt(coins, null, 46, Navy, TextAlignmentOptions.MidlineLeft);
        AutoSize(ct, 22, 46);

        Img(AtX("Divider2", .55f, new Vector2(4, 64)), null, false, false, DividerColor);
        Img(AtX("HeartIcon", .615f, new Vector2(74, 70)), H("HeartIcon"));
        var lives = Either(safe, "LivesText", "TopBar/LivesText");
        if (lives.parent != bar) lives.SetParent(bar, false);
        lives.anchorMin = new Vector2(.665f, .12f); lives.anchorMax = new Vector2(.815f, .88f);
        lives.pivot = new Vector2(0, .5f); lives.offsetMin = Vector2.zero; lives.offsetMax = Vector2.zero;
        lives.localScale = Vector3.one; lives.localRotation = Quaternion.identity;
        var lt = Txt(lives, "3/3", 46, Navy, TextAlignmentOptions.MidlineLeft);
        AutoSize(lt, 22, 46);

        Img(AtX("Divider3", .83f, new Vector2(4, 64)), null, false, false, DividerColor);

        var settings = Either(safe, "BottomBar/SettingsButton", "TopBar/SettingsButton");
        settings.SetParent(bar, false);
        SetRect(settings, new Vector2(.915f, .5f), new Vector2(.915f, .5f), new Vector2(.5f, .5f), Vector2.zero, new Vector2(98, 98));
        Img(settings, H("GearButton"), true, true);
        var stext = settings.Find("SettingsText");
        if (stext != null) stext.gameObject.SetActive(false);
    }

    static void BuildHudButtons(RectTransform safe)
    {
        var gift = Req(safe, "DailyGiftButton");
        SetRect(gift, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(36, -168), new Vector2(300, 86));
        Sliced(gift, H("YellowButton"), 160f / 86f, true);
        var gt = Req(gift, "Dailygift");
        Stretch(gt, 22, 8, 22, 10);
        AutoSize(Txt(gt, null, 32, Navy), 18, 32);

        var plus = Req(safe, "+10");
        SetRect(plus, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(350, -168), new Vector2(112, 86));
        Sliced(plus, H("YellowButton"), 160f / 86f, true);
        var pt = Req(plus, "+10Text");
        Stretch(pt, 8, 8, 8, 10);
        AutoSize(Txt(pt, null, 34, Navy), 18, 34);
    }

    static void BuildBottomBar(RectTransform safe)
    {
        var bar = Req(safe, "BottomBar");
        SetRect(bar, new Vector2(0, 0), new Vector2(1, 0), new Vector2(.5f, 0), new Vector2(0, 12), new Vector2(-28, 124));
        Sliced(bar, H("CreamBar"), 168f / 124f, true);

        NavButton(bar, "HomeButton", "HomeText", "HOME", H("HomeIcon"), new Vector2(0, 0), new Vector2(.5f, 1), new Vector2(12, 10), new Vector2(-16, -10));
        NavButton(bar, "ShopButton", "ShopText", "SHOP", H("ShopIcon"), new Vector2(.5f, 0), new Vector2(1, 1), new Vector2(16, 10), new Vector2(-12, -10));

        var div = Child(bar, "Divider");
        Center(div, Vector2.zero, new Vector2(5, 62));
        Img(div, null, false, false, Navy);
    }

    static void NavButton(RectTransform bar, string name, string textName, string label, Sprite icon,
        Vector2 aMin, Vector2 aMax, Vector2 offMin, Vector2 offMax)
    {
        var b = Req(bar, name);
        b.anchorMin = aMin; b.anchorMax = aMax; b.pivot = new Vector2(.5f, .5f);
        b.offsetMin = offMin; b.offsetMax = offMax; b.localScale = Vector3.one; b.localRotation = Quaternion.identity;
        var img = Sliced(b, H("NavPressedHighlight"), 140f / 104f, true);

        // Yellow capsule is visible only while the button is held (PressHighlight, presentation only).
        var button = b.GetComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.targetGraphic = img;
        var ph = b.GetComponent<PressHighlight>();
        if (ph == null) ph = b.gameObject.AddComponent<PressHighlight>();
        var pso = new SerializedObject(ph);
        pso.FindProperty("highlight").objectReferenceValue = img;
        pso.ApplyModifiedPropertiesWithoutUndo();

        var ic = Child(b, "Icon");
        Center(ic, new Vector2(-78, 0), new Vector2(68, 68));
        Img(ic, icon);
        var t = Req(b, textName);
        t.SetAsLastSibling();
        Center(t, new Vector2(52, 0), new Vector2(190, 80));
        Txt(t, label, 44, Navy).characterSpacing = 1;
    }

    static void BuildSettings(RectTransform safe)
    {
        var panel = Req(safe, "SettingsPanel");
        var popup = Req(panel, "Popup");
        Center(popup, new Vector2(0, -10), new Vector2(840, 953));
        Sliced(popup, St("SettingsShellBlue"), 1.35f, true);

        var header = Req(popup, "HeaderGold");
        SetRect(header, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(.5f, .5f), new Vector2(0, -8), new Vector2(594, 172));
        Img(header, St("SettingsHeaderBlue"));
        var title = Req(header, "Title");
        Center(title, new Vector2(0, 4), new Vector2(500, 110));
        Txt(title, "SETTINGS", 72, Navy).characterSpacing = 4;

        var close = Req(popup, "CloseButton");
        Center(close, new Vector2(332, 421), new Vector2(122, 122));
        Img(close, St("CloseButtonRed"), true, true);
        var navy = close.Find("Navy");
        if (navy != null)
        {
            var nimg = navy.GetComponent<Image>();
            if (nimg != null) nimg.enabled = false;
            Stretch((RectTransform)navy);
            var lbl = navy.Find("Label");
            if (lbl != null) { Stretch((RectTransform)lbl, 0, 6, 0, 0); Txt(lbl, "X", 62, White); }
        }

        SettingsRow(popup, "VibrationRow", "VibrationIcon", "VibrationToggle", 211);
        SettingsRow(popup, "SoundRow", "SoundIcon", "SoundToggle", -49);

        var div = Req(popup, "Divider");
        Center(div, new Vector2(0, 80), new Vector2(690, 4));
        Img(div, null, false, false, new Color32(0xE6, 0xCF, 0xA2, 0xFF));

        var done = Req(popup, "DoneButton");
        Center(done, new Vector2(0, -281), new Vector2(620, 152));
        Sliced(done, H("YellowButton"), 160f / 152f, true);
        var dl = Req(done, "Label");
        Stretch(dl, 0, 10, 0, 0);
        Txt(dl, "Done", 78, Navy);

        var controller = safe.GetComponent<SettingsPanelController>();
        var so = new SerializedObject(controller);
        so.FindProperty("enabledTrackSprite").objectReferenceValue = St("ToggleTrackOnBlue");
        so.FindProperty("disabledTrackSprite").objectReferenceValue = St("ToggleTrackOffBlue");
        so.FindProperty("knobTravel").floatValue = 66f;
        so.FindProperty("stateLabelOffset").floatValue = 33f;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SettingsRow(RectTransform popup, string rowName, string iconName, string toggleName, float y)
    {
        var row = Req(popup, rowName);
        Center(row, new Vector2(0, y), new Vector2(698, 130));

        var icon = Req(row, iconName);
        SetRect(icon, new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(.5f, .5f), new Vector2(72, 0), new Vector2(150, 150));
        var iimg = icon.GetComponent<Image>(); iimg.preserveAspect = true; iimg.raycastTarget = false;

        var label = Req(row, "Label");
        SetRect(label, new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(166, 0), new Vector2(290, 110));
        Txt(label, null, 58, Navy, TextAlignmentOptions.MidlineLeft);

        var toggle = Req(row, toggleName);
        SetRect(toggle, new Vector2(1, .5f), new Vector2(1, .5f), new Vector2(1, .5f), Vector2.zero, new Vector2(242, 108));
        var timg = toggle.GetComponent<Image>();
        timg.sprite = St("ToggleTrackOnBlue"); timg.type = Image.Type.Simple; timg.preserveAspect = true; timg.color = White;

        var state = Req(toggle, "StateLabel");
        Center(state, new Vector2(-33, 2), new Vector2(110, 60));
        Txt(state, null, 40, White);

        var knob = Req(toggle, "Knob");
        Center(knob, new Vector2(66, 0), new Vector2(84, 84));
        Img(knob, St("ToggleKnobCream"));
        foreach (var fx in knob.GetComponents<Shadow>()) fx.enabled = false; // Outline derives from Shadow
    }

    static void OrderSiblings(RectTransform safe)
    {
        string[] order = { "Decor", "MapArea", "Checkout", "TopBar", "DailyGiftButton", "+10", "BottomBar", "LevelPopup", "HomeMusic", "SettingsPanel" };
        int i = 0;
        foreach (var n in order)
        {
            var t = safe.Find(n);
            if (t != null) t.SetSiblingIndex(i++);
        }
    }

    // ------------------------------------------------------------------ capture

    struct Shot
    {
        public string name; public int w, h; public float inTop, inBottom; public bool settings, pressHome, soundOff;
    }

    static void CaptureAll(string dir)
    {
        Directory.CreateDirectory(dir);
        if (SceneManager.GetActiveScene().path != HomeScenePath)
        {
            if (!PrepareScene(out _)) return;
        }
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        var shots = new[]
        {
            new Shot { name = "home_1080x1920_16x9", w = 1080, h = 1920 },
            new Shot { name = "home_1080x2340_19.5x9", w = 1080, h = 2340 },
            new Shot { name = "home_1080x2400_20x9_notch", w = 1080, h = 2400, inTop = 0.045f, inBottom = 0.03f },
            new Shot { name = "home_1200x1920_16x10", w = 1200, h = 1920 },
            new Shot { name = "home_1536x2048_4x3", w = 1536, h = 2048 },
            new Shot { name = "home_pressed_home_1080x2340", w = 1080, h = 2340, pressHome = true },
            new Shot { name = "settings_1080x2340", w = 1080, h = 2340, settings = true },
            new Shot { name = "settings_off_1080x2340", w = 1080, h = 2340, settings = true, soundOff = true },
            new Shot { name = "settings_1080x1920", w = 1080, h = 1920, settings = true },
        };
        foreach (var s in shots)
        {
            Capture(Path.Combine(dir, s.name + ".png"), s);
            EditorSceneManager.OpenScene(HomeScenePath, OpenSceneMode.Single); // discard capture-only changes
        }
        Log("captured " + shots.Length + " shots to " + dir);
    }

    static void Capture(string file, Shot s)
    {
        var canvasGo = GameObject.Find("Canvas");
        var safe = (RectTransform)canvasGo.transform.Find("SafeArea");
        var settings = safe.Find("SettingsPanel");
        var levelPopup = safe.Find("LevelPopup");
        levelPopup.gameObject.SetActive(false);
        settings.gameObject.SetActive(s.settings);
        if (s.settings)
        {
            settings.GetComponent<CanvasGroup>().alpha = 1;
            // mirror what SettingsPanelController.UpdateToggleVisual shows for the saved state
            ApplyToggleVisual(settings, "Popup/VibrationRow/VibrationToggle", true);
            ApplyToggleVisual(settings, "Popup/SoundRow/SoundToggle", !s.soundOff);
        }
        foreach (var n in new[] { "BottomBar/HomeButton", "BottomBar/ShopButton" })
        {
            var b = safe.Find(n).GetComponent<Button>();
            bool pressed = s.pressHome && n.EndsWith("HomeButton");
            b.targetGraphic.canvasRenderer.SetAlpha(pressed ? 1f : 0f);
        }
        CaptureCanvas(file, s.w, s.h, s.inTop, s.inBottom);
    }

    static void ApplyToggleVisual(Transform settings, string path, bool on)
    {
        var t = settings.Find(path);
        t.GetComponent<Image>().sprite = St(on ? "ToggleTrackOnBlue" : "ToggleTrackOffBlue");
        ((RectTransform)t.Find("Knob")).anchoredPosition = new Vector2(on ? 66 : -66, 0);
        var label = t.Find("StateLabel").GetComponent<TMP_Text>();
        label.rectTransform.anchoredPosition = new Vector2(on ? -33 : 33, label.rectTransform.anchoredPosition.y);
        label.text = on ? "ON" : "OFF";
    }

    public static void CaptureCanvas(string file, int w, int h, float inTop, float inBottom)
    {
        var canvasGo = GameObject.Find("Canvas");
        var canvas = canvasGo.GetComponent<Canvas>();
        var crt = (RectTransform)canvasGo.transform;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        var safe = (RectTransform)canvasGo.transform.Find("SafeArea");

        float s = Mathf.Sqrt(w * (float)h / (scaler.referenceResolution.x * scaler.referenceResolution.y));
        var prevMode = canvas.renderMode;
        bool prevScaler = scaler.enabled;
        var prevSafeMin = safe.anchorMin; var prevSafeMax = safe.anchorMax;

        scaler.enabled = false;
        canvas.renderMode = RenderMode.WorldSpace;
        crt.localScale = Vector3.one;
        crt.rotation = Quaternion.identity;
        crt.pivot = new Vector2(.5f, .5f);
        crt.sizeDelta = new Vector2(w / s, h / s);
        crt.position = Vector3.zero;
        safe.anchorMin = new Vector2(0, inBottom); safe.anchorMax = new Vector2(1, 1 - inTop);
        safe.offsetMin = Vector2.zero; safe.offsetMax = Vector2.zero;

        Canvas.ForceUpdateCanvases();
        foreach (var f in canvasGo.GetComponentsInChildren<UniformScaleFitter>(true)) f.Fit();
        Canvas.ForceUpdateCanvases();
        foreach (var t in canvasGo.GetComponentsInChildren<TMP_Text>(false)) t.ForceMeshUpdate(true);
        Canvas.ForceUpdateCanvases();

        var camGo = new GameObject("__CaptureCam") { hideFlags = HideFlags.HideAndDontSave };
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = h / s / 2f;
        cam.transform.position = new Vector3(0, 0, -500);
        cam.nearClipPlane = 0.1f; cam.farClipPlane = 2000f;
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.black;
        cam.cullingMask = ~0;
        var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        cam.aspect = w / (float)h;

        var req = new RenderPipeline.StandardRequest();
        if (RenderPipelineManager.currentPipeline != null && RenderPipeline.SupportsRenderRequest(cam, req))
        {
            req.destination = rt;
            RenderPipeline.SubmitRenderRequest(cam, req);
        }
        else
        {
            cam.Render();
        }

        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();
        RenderTexture.active = prev;
        File.WriteAllBytes(file, tex.EncodeToPNG());

        cam.targetTexture = null;
        UnityEngine.Object.DestroyImmediate(tex);
        UnityEngine.Object.DestroyImmediate(rt);
        UnityEngine.Object.DestroyImmediate(camGo);

        canvas.renderMode = prevMode;
        scaler.enabled = prevScaler;
        safe.anchorMin = prevSafeMin; safe.anchorMax = prevSafeMax;
        safe.offsetMin = Vector2.zero; safe.offsetMax = Vector2.zero;
    }
}

// ---------------------------------------------------------------------- play mode verification

static class PlayTest
{
    const string StepKey = "claude.playtest.step";
    const string DirKey = "claude.playtest.dir";
    const string LogFile = "Temp/claude_playtest_log.txt";

    static double waitUntil;
    static int step;
    static bool origVibration, origSound;
    static bool registered;

    public static void Init()
    {
        if (SessionState.GetInt(StepKey, 0) == 0) return;
        Register();
    }

    static void Register()
    {
        if (registered) return;
        registered = true;
        Application.logMessageReceived += OnLog;
        EditorApplication.update += Tick;
        EditorApplication.playModeStateChanged += OnPlayMode;
    }

    public static void Start(string dir)
    {
        File.WriteAllText(LogFile, "PLAYTEST " + DateTime.Now + "\n");
        SessionState.SetInt(StepKey, 1);
        SessionState.SetString(DirKey, dir);
        Register();
        EditorApplication.EnterPlaymode();
    }

    static void W(string s) => File.AppendAllText(LogFile, s + "\n");

    static void OnLog(string msg, string stack, LogType type)
    {
        if (SessionState.GetInt(StepKey, 0) == 0) return;
        if (type == LogType.Log) return;
        W("[" + type + "] " + msg + (type == LogType.Exception || type == LogType.Error ? "\n" + stack : ""));
    }

    static void OnPlayMode(PlayModeStateChange s)
    {
        if (s == PlayModeStateChange.EnteredEditMode && SessionState.GetInt(StepKey, 0) != 0)
        {
            W("DONE (back in edit mode)");
            SessionState.SetInt(StepKey, 0);
        }
    }

    static void Check(bool ok, string what) => W((ok ? "PASS " : "FAIL ") + what);

    static Transform Safe => GameObject.Find("Canvas")?.transform.Find("SafeArea");

    static void Wait(double sec) { waitUntil = EditorApplication.timeSinceStartup + sec; }

    static void Tick()
    {
        int st = SessionState.GetInt(StepKey, 0);
        if (st == 0 || !EditorApplication.isPlaying) return;
        if (EditorApplication.timeSinceStartup < waitUntil) return;
        try { Run(st); }
        catch (Exception e) { W("EXCEPTION in step " + st + ": " + e); SessionState.SetInt(StepKey, 99); EditorApplication.ExitPlaymode(); }
    }

    static void Next(int s, double wait = 0.4) { SessionState.SetInt(StepKey, s); Wait(wait); }

    static void Pointer(GameObject go, bool enter, bool down, bool up, bool exit)
    {
        var ped = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left, pointerId = -1 };
        if (enter) ExecuteEvents.Execute(go, ped, ExecuteEvents.pointerEnterHandler);
        if (down) { ped.pressPosition = Vector2.zero; ped.pointerPress = go; ExecuteEvents.Execute(go, ped, ExecuteEvents.pointerDownHandler); }
        if (up) ExecuteEvents.Execute(go, ped, ExecuteEvents.pointerUpHandler);
        if (exit) ExecuteEvents.Execute(go, ped, ExecuteEvents.pointerExitHandler);
    }

    static float Alpha(string path) => Safe.Find(path).GetComponent<Button>().targetGraphic.canvasRenderer.GetAlpha();

    static string Dir => SessionState.GetString(DirKey, "");

    static void Shot(string name)
    {
        var canvas = GameObject.Find("Canvas");
        var scaler = canvas.GetComponent<CanvasScaler>();
        var rt = (RectTransform)canvas.transform;
        HomeDesignBuilder.CaptureCanvas(Path.Combine(Dir, name + ".png"), 1080, 2340, 0, 0);
        canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
    }

    static void Run(int st)
    {
        var scene = SceneManager.GetActiveScene().name;
        switch (st)
        {
            case 1:
                if (scene != "Home") { Wait(0.3); return; }
                W("Home loaded via Bootstrap. Loaded scenes: " + SceneManager.sceneCount);
                Check(GameObject.FindObjectsByType<Bootstrap>(FindObjectsSortMode.None).Length <= 1, "single Bootstrap instance");
                Next(2, 1.0);
                break;
            case 2:
            {
                var coinsLabel = Safe.Find("TopBar/CoinsText").GetComponent<TMP_Text>().text;
                int coins = Services.Save.GetInt("coins");
                Check(coinsLabel.Contains(coins.ToString()), "coin label '" + coinsLabel + "' shows saved coins " + coins);
                var gift = Safe.Find("DailyGiftButton/Dailygift").GetComponent<TMP_Text>().text;
                W("daily gift label: '" + gift + "'");
                {
                    int coinsBefore = Services.Save.GetInt("coins");
                    int dayBefore = Services.Save.GetInt("daily_last_day", -1);
                    int today = (int)(DateTime.UtcNow - DateTime.UnixEpoch).TotalDays;
                    var giftLabel = Safe.Find("DailyGiftButton/Dailygift").GetComponent<TMP_Text>();
                    var coinLabel = Safe.Find("TopBar/CoinsText").GetComponent<TMP_Text>();
                    Safe.Find("DailyGiftButton").GetComponent<Button>().onClick.Invoke();
                    if (dayBefore != today)
                    {
                        int reward = Services.Config.Economy.dailyReward;
                        Check(gift == "Daily gift: +" + reward, "daily gift label shows config reward " + reward);
                        Check(Services.Save.GetInt("coins") == coinsBefore + reward, "daily gift adds " + reward + " coins");
                        Check(coinLabel.text.Contains((coinsBefore + reward).ToString()), "coin label updates after gift: '" + coinLabel.text + "'");
                        Check(giftLabel.text == "Come back tomorrow", "gift label switches to 'Come back tomorrow'");
                        Services.Save.SetInt("coins", coinsBefore);
                        Services.Save.SetInt("daily_last_day", dayBefore);
                        Check(Services.Save.GetInt("coins") == coinsBefore && Services.Save.GetInt("daily_last_day", -1) == dayBefore, "coins and daily-gift date restored to original values");
                        Check(coinLabel.text.Contains(coinsBefore.ToString()), "coin label follows restore: '" + coinLabel.text + "'");
                    }
                    else
                    {
                        Check(Services.Save.GetInt("coins") == coinsBefore, "already claimed today: second claim adds nothing");
                    }
                }
                W("lives label: '" + Safe.Find("TopBar/LivesText").GetComponent<TMP_Text>().text + "'");
                Check(Alpha("BottomBar/HomeButton") < 0.01f && Alpha("BottomBar/ShopButton") < 0.01f, "Home/Shop highlight hidden at rest");
                origVibration = Services.Settings.VibrationOn; origSound = Services.Settings.SoundOn;
                SessionState.SetInt("claude.orig.vib", origVibration ? 1 : 0);
                SessionState.SetInt("claude.orig.snd", origSound ? 1 : 0);
                W("saved settings: vibration=" + origVibration + " sound=" + origSound);
                Shot("play_home");
                Safe.Find("TopBar/SettingsButton").GetComponent<Button>().onClick.Invoke();
                Next(3);
                break;
            }
            case 3:
            {
                var panel = Safe.Find("SettingsPanel");
                Check(panel.gameObject.activeSelf, "settings opens from top-bar gear");
                var vt = panel.Find("Popup/VibrationRow/VibrationToggle").GetComponent<Toggle>();
                var stg = panel.Find("Popup/SoundRow/SoundToggle").GetComponent<Toggle>();
                Check(vt.isOn == Services.Settings.VibrationOn && stg.isOn == Services.Settings.SoundOn, "toggles show saved values");
                Check(vt.transform.Find("StateLabel").GetComponent<TMP_Text>().text == (vt.isOn ? "ON" : "OFF"), "vibration state label matches");
                Shot("play_settings");
                stg.isOn = !stg.isOn;
                Check(Services.Settings.SoundOn == stg.isOn, "sound toggle saves new value " + stg.isOn);
                Check(stg.transform.Find("StateLabel").GetComponent<TMP_Text>().text == (stg.isOn ? "ON" : "OFF"), "sound state label updates");
                Shot("play_settings_sound_toggled");
                stg.isOn = SessionState.GetInt("claude.orig.snd", 1) == 1;
                Check(Services.Settings.SoundOn == stg.isOn, "sound restored to original");
                vt.isOn = !vt.isOn;
                Check(Services.Settings.VibrationOn == vt.isOn, "vibration toggle saves new value " + vt.isOn);
                vt.isOn = SessionState.GetInt("claude.orig.vib", 1) == 1;
                Check(Services.Settings.VibrationOn == vt.isOn, "vibration restored to original");
                panel.Find("Popup/DoneButton").GetComponent<Button>().onClick.Invoke();
                Next(4, 2.0);
                break;
            }
            case 4:
            {
                var sp = Safe.Find("SettingsPanel");
                W("diag: active=" + sp.gameObject.activeSelf + " alpha=" + sp.GetComponent<CanvasGroup>().alpha
                  + " frame=" + Time.frameCount + " unscaled=" + Time.unscaledTime + " ctrlEnabled=" + Safe.GetComponent<SettingsPanelController>().isActiveAndEnabled);
            }
                Check(!Safe.Find("SettingsPanel").gameObject.activeSelf, "Done closes settings");
                Safe.Find("TopBar/SettingsButton").GetComponent<Button>().onClick.Invoke();
                Next(5, 1.0);
                break;
            case 5:
                Safe.Find("SettingsPanel/Popup/CloseButton").GetComponent<Button>().onClick.Invoke();
                Next(6, 2.0);
                break;
            case 6:
                Check(!Safe.Find("SettingsPanel").gameObject.activeSelf, "X closes settings");
                Pointer(Safe.Find("BottomBar/HomeButton").gameObject, true, true, false, false);
                Next(7, 0.25);
                break;
            case 7:
                Check(Alpha("BottomBar/HomeButton") > 0.99f, "Home highlight visible while pressed");
                Shot("play_home_pressed");
                Pointer(Safe.Find("BottomBar/HomeButton").gameObject, false, false, true, true);
                Next(8, 0.25);
                break;
            case 8:
                Check(Alpha("BottomBar/HomeButton") < 0.01f, "Home highlight hidden after release");
                Pointer(Safe.Find("BottomBar/ShopButton").gameObject, true, true, false, false);
                Next(9, 0.25);
                break;
            case 9:
                Check(Alpha("BottomBar/ShopButton") > 0.99f, "Shop highlight visible while pressed");
                Pointer(Safe.Find("BottomBar/ShopButton").gameObject, false, false, false, true); // finger slides off
                Next(10, 0.25);
                break;
            case 10:
                Check(Alpha("BottomBar/ShopButton") < 0.01f, "Shop highlight hidden when press is cancelled (pointer exit)");
                Pointer(Safe.Find("BottomBar/ShopButton").gameObject, false, false, true, false);
                Safe.Find("MapArea/LevelMap/Level1Button").GetComponent<Button>().onClick.Invoke();
                Next(11);
                break;
            case 11:
            {
                Check(Alpha("BottomBar/ShopButton") < 0.01f, "Shop highlight still hidden after release");
                var popup = Safe.Find("LevelPopup");
                Check(popup.gameObject.activeSelf, "Level 1 opens level popup");
                Check(popup.Find("PopupWindow/LevelTitle").GetComponent<TMP_Text>().text == "Level 1", "popup title is 'Level 1'");
                popup.Find("PopupWindow/CloseButton").GetComponent<Button>().onClick.Invoke();
                Next(12);
                break;
            }
            case 12:
                Check(!Safe.Find("LevelPopup").gameObject.activeSelf, "level popup closes");
                Safe.Find("MapArea/LevelMap/Level4Button").GetComponent<Button>().onClick.Invoke();
                Next(13);
                break;
            case 13:
                Check(Safe.Find("LevelPopup/PopupWindow/LevelTitle").GetComponent<TMP_Text>().text == "Level 4", "Level 4 opens popup titled 'Level 4'");
                Safe.Find("LevelPopup/PopupWindow/PopupPlayButton").GetComponent<Button>().onClick.Invoke();
                Next(14, 1.0);
                break;
            case 14:
            {
                if (scene != "Level") { Check(false, "Play loads Level (got " + scene + ")"); Next(99, 0); EditorApplication.ExitPlaymode(); return; }
                Check(true, "Play loads Level scene");
                var back = GameObject.FindObjectsByType<Button>(FindObjectsSortMode.None)
                    .FirstOrDefault(b => Enumerable.Range(0, b.onClick.GetPersistentEventCount()).Any(i => b.onClick.GetPersistentMethodName(i) == "Back"));
                Check(back != null, "Level has a Back button");
                back?.onClick.Invoke();
                Next(15, 1.0);
                break;
            }
            case 15:
                Check(scene == "Home", "Back returns to Home");
                Check(GameObject.FindObjectsByType<Bootstrap>(FindObjectsSortMode.None).Length <= 1, "Bootstrap not duplicated after navigation");
                Next(99, 0.5);
                EditorApplication.ExitPlaymode();
                break;
        }
    }
}
