// Assets/Editor/HomeKitInstaller.PlayTest.cs — TEMPORARY (see HomeKitInstaller.cs).
// Play Mode verification of the kit Home screen. It never saves a scene. It records coins and settings
// at the start and checks they are unchanged at the end. Input-system focus settings and the virtual
// test mouse exist only during the test and are restored/removed when Play Mode exits.
using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

static class KitPlayTest
{
    const string StepKey = "claude.kitplay.step";
    const string DirKey = "claude.kitplay.dir";
    const string LogFile = "Temp/claude_kitplay_log.txt";

    static double waitUntil;
    static bool registered;
    static Mouse mouse;
    static bool inputPatched;
    static InputSettings.BackgroundBehavior prevBg;
    static InputSettings.EditorInputBehaviorInPlayMode prevEd;

    static int level, dragStep;
    static int coins0; static bool vib0, snd0;
    static float contentY0;
    static Vector2 dragPos;
    static Vector3 topBar0, bottomBar0;

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
        Directory.CreateDirectory(dir);
        File.WriteAllText(LogFile, "KIT PLAYTEST " + DateTime.Now + "\n");
        SessionState.SetInt(StepKey, 1);
        SessionState.SetString(DirKey, dir);
        Register();
        EditorApplication.EnterPlaymode();
    }

    static void W(string s) => File.AppendAllText(LogFile, s + "\n");
    static void Check(bool ok, string what) => W((ok ? "PASS " : "FAIL ") + what);
    static void Wait(double sec) => waitUntil = EditorApplication.timeSinceStartup + sec;
    static void Next(int s, double wait = 0.4) { SessionState.SetInt(StepKey, s); Wait(wait); }

    static void OnLog(string msg, string stack, LogType type)
    {
        if (SessionState.GetInt(StepKey, 0) == 0 || type == LogType.Log) return;
        W("[" + type + "] " + msg + (type == LogType.Exception || type == LogType.Error ? "\n" + stack : ""));
    }

    static void OnPlayMode(PlayModeStateChange s)
    {
        if (s == PlayModeStateChange.ExitingPlayMode) RestoreInput();
        if (s == PlayModeStateChange.EnteredEditMode && SessionState.GetInt(StepKey, 0) != 0)
        {
            W("DONE (back in edit mode)");
            SessionState.SetInt(StepKey, 0);
        }
    }

    static void Tick()
    {
        int st = SessionState.GetInt(StepKey, 0);
        if (st == 0 || !EditorApplication.isPlaying) return;
        // Keep the player loop (coroutines, input processing) running while the editor window is in the background.
        EditorApplication.QueuePlayerLoopUpdate();
        if (EditorApplication.timeSinceStartup < waitUntil) return;
        try { Run(st); }
        catch (Exception e) { W("EXCEPTION in step " + st + ": " + e); SessionState.SetInt(StepKey, 99); EditorApplication.ExitPlaymode(); }
    }

    // ---------------------------------------------------------------- helpers

    static Transform Safe => GameObject.Find("Canvas")?.transform.Find("SafeArea");
    static ScrollRect Scroll => Safe.Find("MapScroll").GetComponent<ScrollRect>();
    static Transform Levels => Safe.Find("MapScroll/Viewport/Content/Levels");
    static Button LevelButton(int i) => Levels.Find("Level" + i + "Button").GetComponent<Button>();
    static GameObject Popup => Safe.Find("LevelPopup").gameObject;
    static string PopupTitle => Safe.Find("LevelPopup/PopupWindow/LevelTitle").GetComponent<TMP_Text>().text;

    static Rect WorldRect(RectTransform rt)
    {
        var c = new Vector3[4];
        rt.GetWorldCorners(c);
        return Rect.MinMaxRect(c[0].x, c[0].y, c[2].x, c[2].y);
    }

    static Vector2 ScreenCenter(RectTransform rt) => RectTransformUtility.WorldToScreenPoint(null, rt.TransformPoint(rt.rect.center));

    static void Shot(string name)
    {
        HomeKitInstaller.CaptureCanvas(Path.Combine(SessionState.GetString(DirKey, ""), name + ".png"), 1080, 2340, 0, 0);
        Canvas.ForceUpdateCanvases();
    }

    static void PatchInput()
    {
        var s = InputSystem.settings;
        prevBg = s.backgroundBehavior;
        prevEd = s.editorInputBehaviorInPlayMode;
        s.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        s.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        inputPatched = true;
        mouse = InputSystem.AddDevice<Mouse>("KitTestMouse");
    }

    static void RestoreInput()
    {
        if (mouse != null && mouse.added) InputSystem.RemoveDevice(mouse);
        mouse = null;
        if (inputPatched)
        {
            InputSystem.settings.backgroundBehavior = prevBg;
            InputSystem.settings.editorInputBehaviorInPlayMode = prevEd;
            inputPatched = false;
        }
    }

    static void MouseAt(Vector2 p, bool down) =>
        InputSystem.QueueStateEvent(mouse, new MouseState { position = p }.WithButton(MouseButton.Left, down));

    static void Pointer(GameObject go, bool down)
    {
        var ped = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left, pointerId = -1 };
        if (down)
        {
            ExecuteEvents.Execute(go, ped, ExecuteEvents.pointerEnterHandler);
            ped.pointerPress = go;
            ExecuteEvents.Execute(go, ped, ExecuteEvents.pointerDownHandler);
        }
        else
        {
            ExecuteEvents.Execute(go, ped, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(go, ped, ExecuteEvents.pointerExitHandler);
        }
    }

    static float Alpha(string path) => Safe.Find(path).GetComponent<Button>().targetGraphic.canvasRenderer.GetAlpha();

    // ---------------------------------------------------------------- steps

    static void Run(int st)
    {
        string scene = SceneManager.GetActiveScene().name;
        switch (st)
        {
            case 1:
            {
                if (scene != "Home") { Wait(0.3); return; }
                W("Home loaded via Bootstrap. Screen " + Screen.width + "x" + Screen.height + " frame=" + Time.frameCount + " paused=" + EditorApplication.isPaused + " timeScale=" + Time.timeScale);
                Check(UnityEngine.Object.FindObjectsByType<Bootstrap>().Length <= 1, "single Bootstrap instance");
                coins0 = Services.Save.GetInt("coins"); vib0 = Services.Settings.VibrationOn; snd0 = Services.Settings.SoundOn;
                W("start: coins=" + coins0 + " vibration=" + vib0 + " sound=" + snd0);
                Check(Safe.Find("TopBar/CoinsText").GetComponent<TMP_Text>().text.Contains(coins0.ToString()), "coin label shows saved coins");
                W("lives label (static scene text, no lives service yet): '" + Safe.Find("TopBar/LivesText").GetComponent<TMP_Text>().text + "'");

                var sc = Scroll;
                Check(!sc.horizontal && sc.vertical, "ScrollRect is vertical only");
                Check(sc.verticalNormalizedPosition < 0.01f, "map opens at the bottom (normalized " + sc.verticalNormalizedPosition.ToString("0.000") + ")");
                var vp = WorldRect(sc.viewport);
                var co = WorldRect((RectTransform)sc.content.Find("Checkout"));
                Check(co.yMin >= vp.yMin - 1 && co.yMax <= vp.yMax + 1, "checkout fully inside the viewport at start");
                var bottomBar = WorldRect((RectTransform)Safe.Find("BottomBar"));
                Check(co.yMin >= bottomBar.yMax - 40, "checkout sits above the bottom bar (gap " + (co.yMin - bottomBar.yMax).ToString("0") + ")");

                for (int i = 1; i <= 6; i++)
                {
                    var view = LevelButton(i).GetComponent<LevelNodeView>();
                    int saved = Mathf.Clamp(Services.Save.GetInt("level_" + i + "_best_stars", 0), 0, 3);
                    Check(view != null && view.LevelId == i && view.ShownStars == saved, "Level " + i + " node shows number " + i + " and saved best stars " + saved);
                    Check(LevelButton(i).transform.Find("Level" + i + "Text").GetComponent<TMP_Text>().text == i.ToString(), "Level " + i + " number label");
                }
                for (int i = 1; i <= 6; i++)
                for (int j = i + 1; j <= 6; j++)
                {
                    var a = WorldRect((RectTransform)LevelButton(i).transform);
                    var b = WorldRect((RectTransform)LevelButton(j).transform);
                    if (a.Overlaps(b)) Check(false, "hit areas of Level " + i + " and " + j + " overlap");
                }
                W("hit areas checked for overlap");
                int blocking = sc.content.GetComponentsInChildren<Graphic>(true)
                    .Count(g => g.raycastTarget && g.GetComponentInParent<Button>() == null);
                Check(blocking == 0, "no decorative raycast targets inside the map content (" + blocking + ")");
                Shot("play_home");
                level = 1;
                Next(2);
                break;
            }
            case 2:
                LevelButton(level).onClick.Invoke();
                Next(3, 0.3);
                break;
            case 3:
                Check(Popup.activeSelf && PopupTitle == "Level " + level, "Level " + level + " button opens popup titled '" + PopupTitle + "'");
                Safe.Find("LevelPopup/PopupWindow/CloseButton").GetComponent<Button>().onClick.Invoke();
                if (level < 6) { level++; Next(2, 0.3); } else Next(4, 0.3);
                break;
            case 4:
                Check(!Popup.activeSelf, "level popup closes");
                Safe.Find("TopBar/SettingsButton").GetComponent<Button>().onClick.Invoke();
                Next(5, 1.0);
                break;
            case 5:
                Check(Safe.Find("SettingsPanel").gameObject.activeSelf, "settings opens from the gear");
                Safe.Find("SettingsPanel/Popup/DoneButton").GetComponent<Button>().onClick.Invoke();
                Next(6, 2.0);
                break;
            case 6:
            {
                var sp = Safe.Find("SettingsPanel");
                W("diag settings: paused=" + EditorApplication.isPaused + " timeScale=" + Time.timeScale + " active=" + sp.gameObject.activeSelf + " alpha=" + sp.GetComponent<CanvasGroup>().alpha
                  + " frame=" + Time.frameCount + " unscaled=" + Time.unscaledTime.ToString("0.00"));
                Check(!sp.gameObject.activeSelf, "Done closes settings");
                PatchInput();
                dragPos = ScreenCenter((RectTransform)LevelButton(1).transform);
                MouseAt(dragPos, false);
                W("real-input tap on Level 1 at screen " + dragPos);
                Next(7, 0.2);
                break;
            }
            case 7: MouseAt(dragPos, true); Next(8, 0.15); break;
            case 8:
            {
                var es = EventSystem.current;
                W("diag: frame=" + Time.frameCount + " unscaled=" + Time.unscaledTime.ToString("0.00") + " focused=" + Application.isFocused
                  + " runInBackground=" + Application.runInBackground + " mousePos=" + mouse.position.ReadValue() + " leftPressed=" + mouse.leftButton.isPressed
                  + " module=" + (es != null && es.currentInputModule != null ? es.currentInputModule.GetType().Name : "none")
                  + " bg=" + InputSystem.settings.backgroundBehavior + " editorMode=" + InputSystem.settings.editorInputBehaviorInPlayMode
                  + " updateMode=" + InputSystem.settings.updateMode);
                MouseAt(dragPos, false);
                Next(9, 0.5);
                break;
            }
            case 9:
            {
                bool opened = Popup.activeSelf && PopupTitle == "Level 1";
                Check(opened, "real-input tap on Level 1 opens its popup");
                if (Popup.activeSelf) Safe.Find("LevelPopup/PopupWindow/CloseButton").GetComponent<Button>().onClick.Invoke();
                contentY0 = Scroll.content.anchoredPosition.y;
                dragPos = ScreenCenter((RectTransform)LevelButton(2).transform);
                MouseAt(dragPos, false);
                W("real-input drag starting on Level 2 at screen " + dragPos);
                Next(10, 0.3);
                break;
            }
            case 10: MouseAt(dragPos, true); dragStep = 0; Next(11, 0.1); break;
            case 11:
                dragStep++;
                dragPos += new Vector2(0, -Screen.height * 0.03f); // finger moves down = map scrolls toward later levels
                MouseAt(dragPos, true);
                if (dragStep < 8) Next(11, 0.05); else Next(12, 0.1);
                break;
            case 12: MouseAt(dragPos, false); Next(13, 0.8); break;
            case 13:
            {
                float y = Scroll.content.anchoredPosition.y;
                Check(!Popup.activeSelf, "dragging the map from a level node does not open the popup");
                Check(y < contentY0 - 40f, "drag scrolls the map (content y " + contentY0.ToString("0") + " -> " + y.ToString("0") + ")");
                Shot("play_after_drag");
                RestoreInput();
                Next(14, 0.2);
                break;
            }
            case 14:
            {
                // Scrolling with a temporarily longer Content (test only; no levels are added).
                topBar0 = Safe.Find("TopBar").position; bottomBar0 = Safe.Find("BottomBar").position;
                Scroll.content.sizeDelta += new Vector2(0, 1200);
                Scroll.verticalNormalizedPosition = 1f;
                Next(15, 0.6);
                break;
            }
            case 15:
            {
                var vp = WorldRect(Scroll.viewport);
                var co = WorldRect((RectTransform)Scroll.content.Find("Checkout"));
                Check(co.yMax < vp.yMin, "checkout scrolls out of view with the content");
                Check(Safe.Find("TopBar").position == topBar0 && Safe.Find("BottomBar").position == bottomBar0, "top bar and bottom navigation stay fixed while scrolling");
                Shot("play_extended_top");
                Scroll.content.sizeDelta -= new Vector2(0, 1200);
                Scroll.verticalNormalizedPosition = 0f;
                Next(16, 0.6);
                break;
            }
            case 16:
                Pointer(Safe.Find("BottomBar/HomeButton").gameObject, true);
                Next(17, 0.25);
                break;
            case 17:
                Check(Alpha("BottomBar/HomeButton") > 0.99f, "Home yellow highlight visible while pressed");
                Pointer(Safe.Find("BottomBar/HomeButton").gameObject, false);
                Next(18, 0.25);
                break;
            case 18:
                Check(Alpha("BottomBar/HomeButton") < 0.01f && Alpha("BottomBar/ShopButton") < 0.01f, "Home/Shop highlight hidden after release");
                LevelButton(1).onClick.Invoke();
                Next(19, 0.3);
                break;
            case 19:
                Safe.Find("LevelPopup/PopupWindow/PopupPlayButton").GetComponent<Button>().onClick.Invoke();
                Next(20, 1.0);
                break;
            case 20:
            {
                if (scene != "Level") { Check(false, "Play loads Level (got " + scene + ")"); Next(99, 0); EditorApplication.ExitPlaymode(); return; }
                Check(true, "Play loads Level scene");
                var back = UnityEngine.Object.FindObjectsByType<Button>()
                    .FirstOrDefault(b => Enumerable.Range(0, b.onClick.GetPersistentEventCount()).Any(i => b.onClick.GetPersistentMethodName(i) == "Back"));
                Check(back != null, "Level has a Back button");
                back?.onClick.Invoke();
                Next(21, 1.0);
                break;
            }
            case 21:
                Check(scene == "Home", "Back returns to Home");
                Check(UnityEngine.Object.FindObjectsByType<Bootstrap>().Length <= 1, "Bootstrap not duplicated after navigation");
                Check(Scroll.verticalNormalizedPosition < 0.01f, "map opens at the bottom again after returning");
                Check(Services.Save.GetInt("coins") == coins0, "coins unchanged (" + Services.Save.GetInt("coins") + ")");
                Check(Services.Settings.VibrationOn == vib0 && Services.Settings.SoundOn == snd0, "settings unchanged");
                Next(99, 0.3);
                EditorApplication.ExitPlaymode();
                break;
        }
    }
}
