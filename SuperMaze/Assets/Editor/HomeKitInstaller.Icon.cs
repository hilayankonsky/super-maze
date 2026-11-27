using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

#if UNITY_ANDROID
using UnityEditor.Android;
#endif

static partial class HomeKitInstaller
{
    static void SetAppIcon()
    {
        const string dir = "Assets/Materials/Logo/";
        var legacy = Asset<Texture2D>(dir + "AppIcon.png");

        // Default icon for all platforms
        PlayerSettings.SetIcons(
            NamedBuildTarget.Unknown,
            new[] { legacy },
            IconKind.Any
        );

#if UNITY_ANDROID
        var background = Asset<Texture2D>(
            dir + "AppIconBackground.png"
        );

        var foreground = Asset<Texture2D>(
            dir + "AppIconForeground.png"
        );

        SetAndroidIcons(
            AndroidPlatformIconKind.Adaptive,
            background,
            foreground
        );
#endif

        AssetDatabase.SaveAssets();
    }

#if UNITY_ANDROID
    static void SetAndroidIcons(
        PlatformIconKind kind,
        params Texture2D[] layers)
    {
        var icons = PlayerSettings.GetPlatformIcons(
            NamedBuildTarget.Android, kind
        );

        foreach (var icon in icons)
            icon.SetTextures(layers);

        PlayerSettings.SetPlatformIcons(
            NamedBuildTarget.Android, kind, icons
        );

        // Read back to confirm what was stored
        var check = PlayerSettings.GetPlatformIcons(
            NamedBuildTarget.Android, kind
        );

        int filled = 0;

        foreach (var icon in check)
        {
            var tex = icon.GetTextures();

            if (tex != null &&
                tex.Length == layers.Length &&
                System.Array.TrueForAll(tex, t => t != null))
            {
                filled++;
            }
        }

        Log("Android " + kind + ": " + filled + "/" +
            check.Length + " sizes set with " +
            layers.Length + " layer(s)");
    }
#endif
}