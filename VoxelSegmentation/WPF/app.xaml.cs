using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace VoxelSegmentation.WPF;

public partial class App : Application
{
    private string _currentTheme = "System Auto";
    public string CurrentManualTheme => _currentTheme;
    private string AppRegistryPath => $@"Software\{GetSanitizedAppName()}\Settings";

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 1. Load preference from Registry
        LoadThemePreference();

        // 2. Apply the theme (initial load)
        ChangeUserTheme(_currentTheme, saveToRegistry: false);

        // 3. Listen for Windows System theme changes
        SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
    }

    public void ChangeUserTheme(string themeName, Uri themeUri = null, bool saveToRegistry = true)
    {
        _currentTheme = themeName;

        if (saveToRegistry)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(AppRegistryPath))
                {
                    key.SetValue("Theme", themeName);
                }
            }
            catch { }
        }

        if (themeName == "System Auto")
        {
            ApplySystemTheme();
            return;
        }

        // If URI isn't provided (e.g. on startup), find it by the string name
        if (themeUri == null)
        {
            themeUri = FindThemeUriByName(themeName);
        }

        if (themeUri != null)
        {
            UpdateResources(themeUri, isSystemAuto: false);
        }
    }

    private void UpdateResources(Uri uri, bool isSystemAuto)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var newDict = new ResourceDictionary { Source = uri };
            
            // We assume index 0 is Controls.xaml and index 1 is the Theme
            if (Resources.MergedDictionaries.Count > 1)
                Resources.MergedDictionaries[1] = newDict;
            else
                Resources.MergedDictionaries.Add(newDict);

            if (isSystemAuto)
            {
                Color systemAccentColor = GetWindowsAccentColor();
                Color readableTextColor = GetReadableTextColor(systemAccentColor);
                SetCustomAccentColor(systemAccentColor, readableTextColor);
            }
            else
            {
                ResetToThemeAccent();
            }
        });
    }

    private void LoadThemePreference()
    {
        try
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(AppRegistryPath))
            {
                if (key != null && key.GetValue("Theme") is string savedTheme)
                {
                    _currentTheme = savedTheme;
                }
            }
        }
        catch { }
    }

    private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category == UserPreferenceCategory.General && _currentTheme == "System Auto")
        {
            ApplySystemTheme();
        }
    }

    private Uri FindThemeUriByName(string targetThemeName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            string assemblyName = assembly.GetName().Name;
            if (assemblyName.StartsWith("System") || assemblyName.StartsWith("Microsoft")) continue;

            string resourceName = assemblyName + ".g.resources";
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null) continue;
                using (var reader = new ResourceReader(stream))
                {
                    foreach (DictionaryEntry entry in reader)
                    {
                        string key = entry.Key.ToString();
                        if (key.EndsWith(".baml") && Path.GetFileName(key).StartsWith("theme."))
                        {
                            Uri uri = new Uri($"pack://application:,,,/{assemblyName};component/{key.Replace(".baml", ".xaml")}");
                            try
                            {
                                var dict = new ResourceDictionary { Source = uri };
                                if (dict.Contains("ThemeDisplayName") && dict["ThemeDisplayName"].ToString() == targetThemeName)
                                    return uri;
                            }
                            catch { }
                        }
                    }
                }
            }
        }
        return null;
    }

    private void ApplySystemTheme()
    {
        bool isDarkTheme = false;
        try
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
            {
                if (key != null && key.GetValue("AppsUseLightTheme") is int registryValue)
                    isDarkTheme = (registryValue == 0);
            }
        }
        catch { }

        // Adjust these paths to your actual file locations
        string themePath = isDarkTheme 
            ? "pack://application:,,,/VoxelSegmentation;component/WPF/Themes/Theme.Dark.xaml" 
            : "pack://application:,,,/VoxelSegmentation;component/WPF/Themes/Theme.Light.xaml";

        UpdateResources(new Uri(themePath, UriKind.Absolute), isSystemAuto: true);
    }

    private void SetCustomAccentColor(Color accentColor, Color textColor)
    {
        var res = Application.Current.Resources;
        res[SystemColors.HighlightBrushKey] = new SolidColorBrush(accentColor);
        res[SystemColors.HighlightTextBrushKey] = new SolidColorBrush(textColor);
        
        var titleGradient = new LinearGradientBrush { StartPoint = new Point(0,0), EndPoint = new Point(1,0) };
        titleGradient.GradientStops.Add(new GradientStop(accentColor, 0.0));
        titleGradient.GradientStops.Add(new GradientStop(Color.FromRgb((byte)Math.Max(0, accentColor.R - 40), (byte)Math.Max(0, accentColor.G - 40), (byte)Math.Max(0, accentColor.B - 40)), 1.0));
        
        res[SystemColors.ActiveCaptionBrushKey] = titleGradient;
        res[SystemColors.ActiveCaptionTextBrushKey] = new SolidColorBrush(textColor);
    }

    private void ResetToThemeAccent()
    {
        var res = Application.Current.Resources;
        res.Remove(SystemColors.HighlightBrushKey);
        res.Remove(SystemColors.HighlightTextBrushKey);
        res.Remove(SystemColors.ActiveCaptionBrushKey);
        res.Remove(SystemColors.ActiveCaptionTextBrushKey);
    }

    private Color GetWindowsAccentColor()
    {
        try
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM"))
            {
                if (key?.GetValue("ColorizationColor") is int colorInt)
                {
                    return Color.FromArgb(255, (byte)((colorInt >> 16) & 0xFF), (byte)((colorInt >> 8) & 0xFF), (byte)(colorInt & 0xFF));
                }
            }
        }
        catch { }
        return Color.FromRgb(0, 120, 215); // Fallback Blue
    }

    private Color GetReadableTextColor(Color backgroundColor)
    {
        double luminance = (0.299 * backgroundColor.R + 0.587 * backgroundColor.G + 0.114 * backgroundColor.B) / 255;
        return luminance > 0.5 ? Colors.Black : Colors.White;
    }

    public static string GetSanitizedAppName()
    {
        string rawName = Assembly.GetExecutingAssembly().GetName().Name ?? "VoxelSharp";
        return OnlyWordCharHyph().Replace(rawName, "");
    }

    [GeneratedRegex(@"[^\w\-]")]
    private static partial Regex OnlyWordCharHyph();
}