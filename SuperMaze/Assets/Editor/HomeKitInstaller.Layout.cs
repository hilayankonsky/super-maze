// Assets/Editor/HomeKitInstaller.Layout.cs — TEMPORARY (see HomeKitInstaller.cs).
// Builds the scrollable Home map from the modular kit. Existing buttons are moved, never recreated,
// so their OnClick listeners and component references stay intact.
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

static partial class HomeKitInstaller
{
    static readonly Color Navy = new Color32(0x1E, 0x2F, 0x5E, 0xFF);

    // ---- map layout (canvas units at the 1080x1920 reference; x from the content centre, y from the content bottom) ----
    // Values measured from Reference/ApprovedHome.png (941x1672 -> x1.148 to 1080x1920).
    // Positions and sizes below match the measured 1080x1920 reference directly. The map is not globally
    // reduced to fit the upper levels; the ScrollRect provides the extra vertical room instead.
    static readonly Vector2 ReferenceResolution = new Vector2(1080, 1920); // portrait; Expand keeps at least this area on every phone
    const float ContentHeight = 2400f;             // leaves room for the path and future levels above Level 6
    const float CheckoutWidth = 840f;              // reference counter + basket spans almost the full width
    const float CheckoutBottom = 140f;             // just above the bottom bar (12 + 124)
    const float PathMapBottom = 600f;              // PathMap.png is 600x1800 px at 1 px per unit, centred, covering y 600-2400
    const float ShelfHeight = 1015f;               // larger shelves, cropped slightly by the screen edges like the reference
    const float ShelfBottom = 740f;
    const float ShelfOutside = 52f;

    // Exact number-pill centres measured from the approved reference.
    static readonly Vector2[] PillPos =
    {
        new Vector2(-171, 688), new Vector2(159, 868), new Vector2(-167, 1002),
        new Vector2(148, 1130), new Vector2(-108, 1343), new Vector2(20, 1548),
    };

    struct NodeArt
    {
        public string stand, food;
        public float standW, foodW, standCY, foodCY;
        public bool foodBehind;  // drawn behind the stand so a basket/crate rim hides its bottom
        public float pillW, pillH, plateW, plateH, plateCY;
        public float insetW, insetH, insetCY; // conservative RectMask2D inside an open basket/crate
    }
    // Sizes measured against the reference: the pill sits on the stand front and the food just above it.

    static readonly NodeArt[] Nodes =
    {
        new NodeArt { stand = "BenchBlue",   food = "Apple",        standW = 261, foodW = 114, standCY = 5,   foodCY = 97,  pillW = 101, pillH = 61, plateW = 182, plateH = 61, plateCY = -58 },
        new NodeArt { stand = "StoolYellow", food = "Cheese",       standW = 246, foodW = 147, standCY = 7,   foodCY = 89,  pillW = 104, pillH = 64, plateW = 178, plateH = 59, plateCY = -59 },
        new NodeArt { stand = "BasketBench", food = "Baguettes",    standW = 253, foodW = 173, standCY = -29, foodCY = 92,  pillW = 101, pillH = 63, plateW = 165, plateH = 55, plateCY = -56, foodBehind = true, insetW = 150, insetH = 46, insetCY = 29 },
        new NodeArt { stand = "MilkDisplay", food = "Milk",         standW = 212, foodW = 103, standCY = 34,  foodCY = 165, pillW = 103, pillH = 61, plateW = 167, plateH = 54, plateCY = -54 },
        new NodeArt { stand = "CrateCoral",  food = "Strawberries", standW = 262, foodW = 208, standCY = -5,  foodCY = 85,  pillW = 99,  pillH = 61, plateW = 166, plateH = 55, plateCY = -55, foodBehind = true, insetW = 164, insetH = 46, insetCY = 65 },
        new NodeArt { stand = "BenchBlue",   food = "Honey",        standW = 257, foodW = 146, standCY = 0,   foodCY = 78,  pillW = 97,  pillH = 55, plateW = 157, plateH = 54, plateCY = -53 },
    };

    // ------------------------------------------------------------------ apply

    static void Apply()
    {
        if (!PrepareScene()) return;
        var canvas = GameObject.Find("Canvas").transform;
        var safe = Req(canvas, "SafeArea");

        // Portrait canvas scaling: 1080x1920 reference, Expand. Every phone gets at least that area at the
        // same scale, so nothing is squeezed; taller phones simply show more of the map.
        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = ReferenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

        // Background floor (behind everything, including the safe-area margins).
        var bg = Req(canvas, "Background");
        Img(bg, K("Environment/Floor"), false);

        // The map reaches the lower edge of the top bar, matching the reference. The two extra course
        // controls sit compactly in the otherwise-unused Level-7 area instead of reserving an empty row.
        const float topBarBottom = 176f;
        var scrollGo = Child(safe, "MapScroll");
        Stretch(scrollGo);
        scrollGo.offsetMax = new Vector2(0, -topBarBottom);
        var viewport = Child(scrollGo, "Viewport");
        Stretch(viewport);
        Img(viewport, null, false, true, new Color(1, 1, 1, 0)); // invisible drag surface
        if (viewport.GetComponent<RectMask2D>() == null) viewport.gameObject.AddComponent<RectMask2D>();
        var content = Child(viewport, "Content");
        content.anchorMin = new Vector2(0, 0); content.anchorMax = new Vector2(1, 0); content.pivot = new Vector2(.5f, 0);
        content.anchoredPosition = Vector2.zero; content.sizeDelta = new Vector2(0, ContentHeight);
        content.localScale = Vector3.one; content.localRotation = Quaternion.identity;

        var scroll = scrollGo.GetComponent<ScrollRect>();
        if (scroll == null) scroll = scrollGo.gameObject.AddComponent<ScrollRect>();
        scroll.content = content; scroll.viewport = viewport;
        scroll.horizontal = false; scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Elastic; scroll.elasticity = 0.1f;
        scroll.inertia = true; scroll.decelerationRate = 0.135f; scroll.scrollSensitivity = 40f;
        scroll.verticalScrollbar = null; scroll.horizontalScrollbar = null;
        scroll.verticalNormalizedPosition = 0f;

        // Shelves: angled (perspective) versions so each one is narrow at the screen edge with its sign fully readable.
        var shelfL = Child(content, "ShelfProduce");
        var shelfLSprite = K("Environment/ShelfProduceAngled");
        SetRect(shelfL, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(-ShelfOutside, ShelfBottom),
            new Vector2(ShelfHeight * shelfLSprite.rect.width / shelfLSprite.rect.height, ShelfHeight));
        Img(shelfL, shelfLSprite);
        var shelfR = Child(content, "ShelfBakery");
        var shelfRSprite = K("Environment/ShelfBakeryAngled");
        SetRect(shelfR, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(ShelfOutside, ShelfBottom),
            new Vector2(ShelfHeight * shelfRSprite.rect.width / shelfRSprite.rect.height, ShelfHeight));
        Img(shelfR, shelfRSprite);

        // Path: one generated image that follows the reference route under every level (replaces the tiles).
        var path = Child(content, "Path");
        SetRect(path, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(.5f, 0), Vector2.zero, new Vector2(1080f, ContentHeight));
        for (int i = path.childCount - 1; i >= 0; i--)
        {
            var old = path.GetChild(i);
            if (old.name.StartsWith("PathStart") || old.name.StartsWith("PathLoop")) { Log("removed tile " + old.name); UnityEngine.Object.DestroyImmediate(old.gameObject); }
        }
        var pathMap = Child(path, "PathMap");
        var pathSprite = K("Path/PathMap");
        SetRect(pathMap, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(0, PathMapBottom),
            new Vector2(pathSprite.rect.width, pathSprite.rect.height));
        Img(pathMap, pathSprite, false);

        // Checkout at the start of the map (scrolls away with the content).
        var checkout = Child(content, "Checkout");
        var coSprite = K("Environment/Checkout");
        SetRect(checkout, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(47, CheckoutBottom),
            new Vector2(CheckoutWidth, CheckoutWidth * coSprite.rect.height / coSprite.rect.width));
        Img(checkout, coSprite);

        // Level nodes: move the existing buttons (keeps OnClick -> LevelPopupController.Open(n)).
        var levels = Child(content, "Levels");
        SetRect(levels, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(.5f, 0), Vector2.zero, new Vector2(1080f, ContentHeight));
        for (int i = 1; i <= 6; i++)
        {
            var btn = Either(safe, "MapArea/LevelMap/Level" + i + "Button", "MapScroll/Viewport/Content/Levels/Level" + i + "Button");
            btn.SetParent(levels, false);
            BuildNode(btn, i, Nodes[i - 1]);
        }

        // Fixed HUD art.
        BuildTopBar(safe);
        BuildCompactRewards(safe);
        BuildBottomBar(safe);
        BuildLeaves(safe);
        BuildSettings(safe);

        // Remove the old code-drawn decoration (pure Images, nothing references them).
        foreach (var n in new[] { "MapArea", "Decor", "Checkout" })
        {
            var t = safe.Find(n);
            if (t != null) { Log("removed SafeArea/" + n); UnityEngine.Object.DestroyImmediate(t.gameObject); }
        }
        var back = canvas.Find("BackDecor");
        if (back != null) { Log("removed Canvas/BackDecor"); UnityEngine.Object.DestroyImmediate(back.gameObject); }

        string[] order = { "MapScroll", "TopLeaves", "TopBar", "DailyGiftButton", "+10", "BottomBar", "LevelPopup", "HomeMusic", "SettingsPanel" };
        int idx = 0;
        foreach (var n in order) { var t = safe.Find(n); if (t != null) t.SetSiblingIndex(idx++); }
        content.Find("ShelfProduce").SetSiblingIndex(0);
        content.Find("ShelfBakery").SetSiblingIndex(1);
        path.SetSiblingIndex(2);
        checkout.SetSiblingIndex(3);
        levels.SetSiblingIndex(4);

        var scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Log("saved " + scene.path + "; content height " + ContentHeight);
        for (int i = 1; i <= 6; i++) Log("Level" + i + " pill " + PillPos[i - 1]);
    }

    static void BuildNode(RectTransform btn, int level, NodeArt art)
    {
        var standSprite = K("Stands/" + art.stand);
        var foodSprite = K("Food/" + art.food);
        float standH = art.standW * standSprite.rect.height / standSprite.rect.width;
        float foodH = art.foodW * foodSprite.rect.height / foodSprite.rect.width;

        // Layout relative to the number pill (pill centre = 0).
        const float pillCY = 0f;
        float standCY = art.standCY;
        float foodCY = art.foodCY;
        float plateCY = art.plateCY;

        float top = Mathf.Max(foodCY + foodH / 2f, standCY + standH / 2f);
        float bottom = Mathf.Min(plateCY - art.plateH / 2f, standCY - standH / 2f);
        float hitW = level >= 5 ? 128f : Mathf.Min(art.standW, 220f);
        float hitH = top - bottom;
        float centerY = (top + bottom) / 2f;
        Vector2 bend = PillPos[level - 1];

        // The button itself is the transparent hit area.
        // Anchored to the bottom-centre of Levels, like every other map element (y measured from the content bottom).
        SetRect(btn, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(.5f, .5f), new Vector2(bend.x, bend.y + centerY), new Vector2(hitW, hitH));
        var hit = Img(btn, null, false, true, new Color(1, 1, 1, 0));

        float Rel(float y) => y - centerY;

        var food = Child(btn, "Food");
        Center(food, new Vector2(0, Rel(foodCY)), new Vector2(art.foodW, foodH));
        Img(food, foodSprite);

        var stand = Child(btn, "Stand");
        Center(stand, new Vector2(0, Rel(standCY)), new Vector2(art.standW, standH));
        var standImg = Img(stand, standSprite);

        // Food drawn behind the stand only for the basket / crate, so the rim hides its bottom.
        food.SetSiblingIndex(art.foodBehind ? 0 : 1);
        stand.SetSiblingIndex(art.foodBehind ? 1 : 0);

        var oldInset = btn.Find("FoodInteriorMask");
        if (art.insetW > 0f)
        {
            var inset = oldInset as RectTransform ?? Child(btn, "FoodInteriorMask");
            Center(inset, new Vector2(0, Rel(art.insetCY)), new Vector2(art.insetW, art.insetH));
            if (inset.GetComponent<RectMask2D>() == null) inset.gameObject.AddComponent<RectMask2D>();
            var fill = Child(inset, "FoodFill");
            Center(fill, new Vector2(0, foodCY - art.insetCY), new Vector2(art.foodW, foodH));
            Img(fill, foodSprite);
            inset.SetSiblingIndex(2);
        }
        else if (oldInset != null)
        {
            UnityEngine.Object.DestroyImmediate(oldInset.gameObject);
        }

        var pill = Req(btn, "NumberPill");
        pill.SetSiblingIndex(3);
        Center(pill, new Vector2(0, Rel(pillCY)), new Vector2(art.pillW, art.pillH));
        Sliced(pill, K("UI/NumberPill"), 163f / art.pillH);

        var text = Req(btn, "Level" + level + "Text");
        text.SetSiblingIndex(4);
        Center(text, new Vector2(0, Rel(pillCY) + 1), new Vector2(art.pillW, art.pillH));
        var tmp = text.GetComponent<TextMeshProUGUI>();
        tmp.fontSize = 46; tmp.enableAutoSizing = false; tmp.color = Navy; tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false; tmp.textWrappingMode = TextWrappingModes.NoWrap; tmp.overflowMode = TextOverflowModes.Overflow;

        var plate = Child(btn, "StarPlate");
        plate.SetSiblingIndex(5);
        Center(plate, new Vector2(0, Rel(plateCY)), new Vector2(art.plateW, art.plateH));
        Sliced(plate, K("UI/StarPlate"), 161f / art.plateH);
        var starImgs = new Image[3];
        for (int s = 0; s < 3; s++)
        {
            var st = Child(plate, "Star" + (s + 1));
            float starSize = Mathf.Min(44f, (art.plateW - 24f) / 3f);
            Center(st, new Vector2((s - 1) * starSize, 1f), new Vector2(starSize, starSize));
            starImgs[s] = Img(st, K("UI/StarEmpty"));
        }

        var button = btn.GetComponent<Button>();
        button.transition = Selectable.Transition.ColorTint;
        button.targetGraphic = standImg; // press tint shows on the stand, not on the invisible hit area

        var view = btn.GetComponent<LevelNodeView>();
        if (view == null) view = btn.gameObject.AddComponent<LevelNodeView>();
        var so = new SerializedObject(view);
        so.FindProperty("levelId").intValue = level;
        so.FindProperty("numberLabel").objectReferenceValue = tmp;
        var arr = so.FindProperty("stars");
        arr.arraySize = 3;
        for (int s = 0; s < 3; s++) arr.GetArrayElementAtIndex(s).objectReferenceValue = starImgs[s];
        so.FindProperty("starFull").objectReferenceValue = K("UI/StarFull");
        so.FindProperty("starEmpty").objectReferenceValue = K("UI/StarEmpty");
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void BuildTopBar(RectTransform safe)
    {
        var bar = Req(safe, "TopBar");
        SetRect(bar, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(.5f, .5f), new Vector2(0, -107), new Vector2(972, 138));
        Sliced(bar, K("UI/CreamBar"), 131f / 138f, true);

        var logo = Req(bar, "Logo");
        Img(logo, Asset<Sprite>("Assets/Materials/Logo/SuperMazeLogo.png"));
        Center(logo, new Vector2(-359, 16), new Vector2(156, 160));

        SetBarDivider(Req(bar, "Divider1"), -241, 8, 64);
        SetBarDivider(Req(bar, "Divider2"), 44, 5, 56);
        SetBarDivider(Req(bar, "Divider3"), 313, 5, 56);

        var coinIcon = Req(bar, "CoinIcon");
        Center(coinIcon, new Vector2(-171, 5), new Vector2(78, 78));
        Img(coinIcon, K("UI/Coin"));
        var coinsText = Req(bar, "CoinsText");
        Center(coinsText, new Vector2(-54, 6), new Vector2(150, 70));
        StyleCounter(coinsText.GetComponent<TextMeshProUGUI>());

        var heart = Req(bar, "HeartIcon");
        Center(heart, new Vector2(125, 5), new Vector2(82, 73));
        Img(heart, K("UI/Heart"));
        var lives = Req(bar, "LivesText");
        Center(lives, new Vector2(229, 5), new Vector2(120, 70));
        StyleCounter(lives.GetComponent<TextMeshProUGUI>());

        var settings = Req(bar, "SettingsButton");
        Center(settings, new Vector2(396, 5), new Vector2(98, 95));
        Img(settings, K("UI/Gear"), true, true);
    }

    static void SetBarDivider(RectTransform rect, float x, float y, float height)
    {
        Center(rect, new Vector2(x, y), new Vector2(4, height));
        var image = Img(rect, null);
        image.color = Navy;
    }

    static void StyleCounter(TextMeshProUGUI text)
    {
        text.enableAutoSizing = false;
        text.fontSize = 52;
        text.fontStyle = FontStyles.Bold;
        text.color = Navy;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;
    }

    static void BuildCompactRewards(RectTransform safe)
    {
        var gift = Req(safe, "DailyGiftButton");
        SetRect(gift, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(.5f, .5f), new Vector2(-72, -205), new Vector2(180, 54));
        var giftText = Req(gift, "Dailygift").GetComponent<TextMeshProUGUI>();
        giftText.enableAutoSizing = true; giftText.fontSizeMin = 17; giftText.fontSizeMax = 29;

        var plus = Req(safe, "+10");
        SetRect(plus, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(.5f, .5f), new Vector2(72, -205), new Vector2(86, 54));
        var plusText = Req(plus, "+10Text").GetComponent<TextMeshProUGUI>();
        plusText.enableAutoSizing = true; plusText.fontSizeMin = 20; plusText.fontSizeMax = 31;
    }

    // Settings popup restyled after the team's mock-up: arched header, red close button with a drawn x,
    // navy line icons. Only sprites/sizes change; every serialized reference of SettingsPanelController is untouched.
    static void BuildSettings(RectTransform safe)
    {
        const string ui = "Assets/Materials/SettingsUI/";
        var popup = Req(safe, "SettingsPanel/Popup");
        var header = Req(popup, "HeaderGold");
        Img(header, Asset<Sprite>(ui + "SettingsHeaderArch.png"), false);
        header.sizeDelta = new Vector2(600, 176);
        Req(header, "Title").anchoredPosition = new Vector2(0, 8);

        var close = Req(popup, "CloseButton");
        Img(close, Asset<Sprite>(ui + "CloseButtonX.png"), true, true);
        close.sizeDelta = new Vector2(124, 124);
        Req(close, "Navy").gameObject.SetActive(false); // the old drawn "X" label; the new sprite has its own x

        var vib = Req(popup, "VibrationRow/VibrationIcon");
        Img(vib, Asset<Sprite>(ui + "VibrationIconNavy.png"));
        vib.sizeDelta = new Vector2(165, 165);
        var snd = Req(popup, "SoundRow/SoundIcon");
        Img(snd, Asset<Sprite>(ui + "SoundIconNavy.png"));
        snd.sizeDelta = new Vector2(165, 165);
    }

    static T Asset<T>(string path) where T : UnityEngine.Object
    {
        var a = AssetDatabase.LoadAssetAtPath<T>(path);
        if (a == null) throw new Exception("MISSING ASSET " + path);
        return a;
    }

    static void BuildBottomBar(RectTransform safe)
    {
        var bar = Req(safe, "BottomBar");
        SetRect(bar, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(.5f, .5f), new Vector2(0, 72), new Vector2(1063, 139));
        Sliced(bar, K("UI/CreamBar"), 131f / 139f, true);

        var home = Req(bar, "HomeButton");
        Center(home, new Vector2(-266, 0), new Vector2(506, 104));
        Sliced(home, K("UI/PressedYellow"), 172f / 104f, true);
        Center(Req(home, "Icon"), new Vector2(-87, 3), new Vector2(69, 68));
        Img(Req(home, "Icon"), K("UI/Home"));
        StyleNavLabel(Req(home, "HomeText"), new Vector2(72, 3), new Vector2(190, 70));

        var shop = Req(bar, "ShopButton");
        Center(shop, new Vector2(265, 0), new Vector2(506, 104));
        Sliced(shop, K("UI/PressedYellow"), 172f / 104f, true);
        Center(Req(shop, "Icon"), new Vector2(-77, 5), new Vector2(85, 72));
        Img(Req(shop, "Icon"), K("UI/Shop"));
        StyleNavLabel(Req(shop, "ShopText"), new Vector2(82, 2), new Vector2(180, 70));

        var divider = Req(bar, "Divider");
        Center(divider, new Vector2(50, 1), new Vector2(4, 64));
        Img(divider, null).color = Navy;
    }

    static void StyleNavLabel(RectTransform rect, Vector2 position, Vector2 size)
    {
        Center(rect, position, size);
        var text = rect.GetComponent<TextMeshProUGUI>();
        text.enableAutoSizing = false;
        text.fontSize = 52;
        text.fontStyle = FontStyles.Bold;
        text.color = Navy;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
    }

    static void BuildLeaves(RectTransform safe)
    {
        var leaves = Child(safe, "TopLeaves");
        Stretch(leaves);
        var l = Child(leaves, "LeavesLeft");
        SetRect(l, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(-30, 45), new Vector2(430, 430));
        Img(l, K("UI/Leaves"));
        var r = Child(leaves, "LeavesRight");
        SetRect(r, new Vector2(1, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(25, 45), new Vector2(350, 360));
        r.localScale = new Vector3(-1, 1, 1);
        Img(r, K("UI/Leaves"));
    }

    // ------------------------------------------------------------------ helpers

    static Sprite K(string name)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(Kit + name + ".png");
        if (s == null) throw new Exception("MISSING SPRITE " + Kit + name + ".png");
        return s;
    }

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
        foreach (var p in paths) { var t = root.Find(p); if (t != null) return (RectTransform)t; }
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

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.pivot = new Vector2(.5f, .5f);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        rt.localRotation = Quaternion.identity; rt.localScale = Vector3.one;
    }

    static Image Img(Component c, Sprite s, bool preserve = true, bool raycast = false, Color? color = null)
    {
        var img = c.GetComponent<Image>();
        if (img == null) img = c.gameObject.AddComponent<Image>();
        img.sprite = s;
        img.type = Image.Type.Simple;
        img.preserveAspect = preserve;
        img.raycastTarget = raycast;
        img.color = color ?? Color.white;
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

    // ------------------------------------------------------------------ capture (edit mode, discarded afterwards)

    struct Shot { public string name; public int w, h; public float inTop, inBottom; public float scroll; public float extraHeight; public bool settings; }

    static void CaptureAll(string dir)
    {
        if (!PrepareScene()) return;
        Directory.CreateDirectory(dir);
        var shots = new[]
        {
            new Shot { name = "home_1080x1920_16x9", w = 1080, h = 1920 },
            new Shot { name = "home_1080x2340_19.5x9", w = 1080, h = 2340 },
            new Shot { name = "home_1080x2400_notch", w = 1080, h = 2400, inTop = 0.045f, inBottom = 0.03f },
            new Shot { name = "home_1536x2048_4x3", w = 1536, h = 2048 },
            new Shot { name = "home_1080x2340_scrolled_top", w = 1080, h = 2340, scroll = 1f },
            new Shot { name = "home_1080x2340_extended_mid", w = 1080, h = 2340, scroll = 0.5f, extraHeight = 1200f },
            new Shot { name = "settings_1080x1920", w = 1080, h = 1920, settings = true },
        };
        foreach (var s in shots)
        {
            CaptureShot(Path.Combine(dir, s.name + ".png"), s);
            EditorSceneManager.OpenScene(HomeScenePath, OpenSceneMode.Single); // discard capture-only changes
        }
        Log("captured " + shots.Length + " shots to " + dir);
    }

    static void CaptureShot(string file, Shot s)
    {
        var safe = (RectTransform)GameObject.Find("Canvas").transform.Find("SafeArea");
        safe.Find("LevelPopup").gameObject.SetActive(false);
        var settings = safe.Find("SettingsPanel");
        settings.gameObject.SetActive(s.settings);
        if (s.settings && settings.TryGetComponent<CanvasGroup>(out var cg)) cg.alpha = 1f;
        foreach (var n in new[] { "BottomBar/HomeButton", "BottomBar/ShopButton" })
            safe.Find(n).GetComponent<Button>().targetGraphic.canvasRenderer.SetAlpha(0f);
        var scroll = safe.Find("MapScroll").GetComponent<ScrollRect>();
        scroll.content.sizeDelta += new Vector2(0, s.extraHeight);
        CaptureCanvas(file, s.w, s.h, s.inTop, s.inBottom, scroll, s.scroll);
    }

    // Renders the canvas at a given phone resolution through a temporary camera.
    public static void CaptureCanvas(string file, int w, int h, float inTop, float inBottom, ScrollRect scroll = null, float scrollPos = -1f)
    {
        var canvasGo = GameObject.Find("Canvas");
        var canvas = canvasGo.GetComponent<Canvas>();
        var crt = (RectTransform)canvasGo.transform;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        var safe = (RectTransform)canvasGo.transform.Find("SafeArea");

        // Same scale factor CanvasScaler (Scale With Screen Size) computes for this screen size.
        float rx = w / scaler.referenceResolution.x, ry = h / scaler.referenceResolution.y;
        float sf = scaler.screenMatchMode == CanvasScaler.ScreenMatchMode.Expand ? Mathf.Min(rx, ry)
                 : scaler.screenMatchMode == CanvasScaler.ScreenMatchMode.Shrink ? Mathf.Max(rx, ry)
                 : Mathf.Pow(2f, Mathf.Lerp(Mathf.Log(rx, 2f), Mathf.Log(ry, 2f), scaler.matchWidthOrHeight));
        var prevMode = canvas.renderMode;
        bool prevScaler = scaler.enabled;
        var prevSafeMin = safe.anchorMin; var prevSafeMax = safe.anchorMax;

        scaler.enabled = false;
        canvas.renderMode = RenderMode.WorldSpace;
        crt.localScale = Vector3.one; crt.rotation = Quaternion.identity; crt.pivot = new Vector2(.5f, .5f);
        crt.sizeDelta = new Vector2(w / sf, h / sf);
        crt.position = Vector3.zero;
        safe.anchorMin = new Vector2(0, inBottom); safe.anchorMax = new Vector2(1, 1 - inTop);
        safe.offsetMin = Vector2.zero; safe.offsetMax = Vector2.zero;

        Canvas.ForceUpdateCanvases();
        if (scroll != null && scrollPos >= 0f)
        {
            float range = Mathf.Max(0f, scroll.content.rect.height - scroll.viewport.rect.height);
            scroll.content.anchoredPosition = new Vector2(0, -range * scrollPos);
            Canvas.ForceUpdateCanvases();
        }
        foreach (var t in canvasGo.GetComponentsInChildren<TMP_Text>(false)) t.ForceMeshUpdate(true);
        Canvas.ForceUpdateCanvases();

        var camGo = new GameObject("__CaptureCam") { hideFlags = HideFlags.HideAndDontSave };
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true; cam.orthographicSize = h / sf / 2f;
        cam.transform.position = new Vector3(0, 0, -500);
        cam.nearClipPlane = 0.1f; cam.farClipPlane = 2000f;
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.black; cam.cullingMask = ~0;
        var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt; cam.aspect = w / (float)h;

        var req = new RenderPipeline.StandardRequest();
        if (RenderPipelineManager.currentPipeline != null && RenderPipeline.SupportsRenderRequest(cam, req))
        {
            req.destination = rt;
            RenderPipeline.SubmitRenderRequest(cam, req);
        }
        else cam.Render();

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
