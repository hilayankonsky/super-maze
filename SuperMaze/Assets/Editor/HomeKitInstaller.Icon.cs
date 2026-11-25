// Assets/Editor/HomeKitInstaller.Icon.cs — TEMPORARY (see HomeKitInstaller.cs).
// Sets the team logo as the app icon (requested by the team). Changes Player Settings > Icon only:
// the Default Icon plus the Android Adaptive icon (background + foreground layers).
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEngine;

static partial class HomeKitInstaller
{
    static void SetAppIcon()
    {
        const string dir = "Assets/Materials/Logo/";
        var legacy = Asset<Texture2D>(dir + "AppIcon.png");
        var background = Asset<Texture2D>(dir + "AppIconBackground.png");
        var foreground = Asset<Texture2D>(dir + "AppIconForeground.png");

        // Default icon: used by every platform that has no icon of its own.
        PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { legacy }, IconKind.Any);

        // Android: adaptive layers are background (0) then foreground (1), as in the Player Settings icon panel.
        // (Round and Legacy icon kinds are obsolete in Unity 6.6; Adaptive is the only Android icon kind.)
        SetAndroidIcons(AndroidPlatformIconKind.Adaptive, background, foreground);
        AssetDatabase.SaveAssets();
    }

    static void SetAndroidIcons(PlatformIconKind kind, params Texture2D[] layers)
    {
        var icons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind);
        foreach (var icon in icons) icon.SetTextures(layers);
        PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, kind, icons);

        // Read back to confirm what was stored.
        var check = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind);
        int filled = 0;
        foreach (var icon in check)
        {
            var tex = icon.GetTextures();
            if (tex != null && tex.Length == layers.Length && System.Array.TrueForAll(tex, t => t != null)) filled++;
        }
        Log("Android " + kind + ": " + filled + "/" + check.Length + " sizes set with " + layers.Length + " layer(s)");
    }
}
