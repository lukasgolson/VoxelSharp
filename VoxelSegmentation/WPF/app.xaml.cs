using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using Application = System.Windows.Application;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using MessageBox = System.Windows.MessageBox;
using Point = System.Windows.Point;
using SystemColors = System.Windows.SystemColors;

namespace VoxelSegmentation.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private string _currentManualTheme = "System Auto";

    // ADD THIS PROPERTY:
    public string CurrentManualTheme => _currentManualTheme;


    private string AppRegistryPath => $@"Software\{GetSanitizedAppName()}\Settings";

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        this.DispatcherUnhandledException += App_DispatcherUnhandledException;

        // 2. Catch unhandled background thread exceptions
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        // 3. Catch unhandled exceptions from asynchronous Tasks
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

        LoadThemePreference();

        ChangeUserTheme(_currentManualTheme, saveToRegistry: false);

        SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogCrashAndShowMessage(e.Exception, "UI Thread Crash");
        e.Handled = true; // Prevents the default Windows crash dialog
        Environment.Exit(1); // Force close
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            LogCrashAndShowMessage(ex, "Background Thread Crash");
        }

        Environment.Exit(1);
    }

    private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        LogCrashAndShowMessage(e.Exception, "Async Task Crash");
        e.SetObserved();
        Environment.Exit(1);
    }

    private void LogCrashAndShowMessage(Exception ex, string crashType)
    {
        try
        {
            // Write the crash log to the same folder as the .exe
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log");

            string logContent = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] FATAL ERROR ({crashType})\n" +
                                $"Message: {ex.Message}\n\n" +
                                $"Stack Trace:\n{ex.StackTrace}\n" +
                                $"------------------------------------------------------\n\n";

            File.AppendAllText(logPath, logContent);

            // Show a final message to the user
            MessageBox.Show($"The application encountered a fatal error and must close.\n\n" +
                            $"Error: {ex.Message}\n\n" +
                            $"A detailed log has been saved to:\n{logPath}",
                "Fatal Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        catch
        {
            // If the logger itself fails, there's nothing we can do but let it die.
        }
    }


    private void LoadThemePreference()
    {
        try
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(AppRegistryPath))
            {
                if (key != null && key.GetValue("Theme") is string savedTheme)
                {
                    _currentManualTheme = savedTheme;
                }
            }
        }
        catch
        {
        } // Fails gracefully to "System Auto" if no key exists
    }

    private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        // Only auto-update if the user is using the "System Auto" setting!
        if (e.Category == UserPreferenceCategory.General && _currentManualTheme == "System Auto")
        {
            ApplySystemThemeAndAccent();
        }
    }

    public void ChangeUserTheme(string themeName, bool saveToRegistry = true)
    {
        _currentManualTheme = themeName;

        if (saveToRegistry)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(AppRegistryPath))
                {
                    key.SetValue("Theme", themeName);
                }
            }
            catch
            {
            }
        }

        if (themeName == "System Auto")
        {
            ApplySystemThemeAndAccent();
            return;
        }

        // 1. Determine the correct Resource Dictionary
        string themeFileName = "/WPF/Themes/Theme.Light.xaml"; // Fallback
        switch (themeName)
        {
            case "Classic Light": themeFileName = "/WPF/Themes/Theme.Light.xaml"; break;
            case "Charcoal Dark": themeFileName = "/WPF/Themes/Theme.Dark.xaml"; break;
            case "CRT Matrix": themeFileName = "/WPF/Themes/Theme.CRT.xaml"; break;
            case "Olive": themeFileName = "/WPF/Themes/Theme.Steam.xaml"; break;
            case "Rugged Military": themeFileName = "/WPF/Themes/Theme.Rugged.xaml"; break;
            case "Hot Dog Stand": themeFileName = "/WPF/Themes/Theme.Hotdog.xaml"; break;
        }

        // 2. Apply it on the UI thread
        Application.Current.Dispatcher.Invoke(() =>
        {
            Current.Resources.MergedDictionaries[1] = new ResourceDictionary
                { Source = new Uri(themeFileName, UriKind.Relative) };
            ResetToThemeAccent();
        });
    }

    /// <summary>
    /// Returns the assembly name with all non-alphanumeric characters removed.
    /// This ensures the Registry path is always valid.
    /// </summary>
    public static string GetSanitizedAppName()
    {
        string? rawName = Assembly.GetExecutingAssembly().GetName().Name;
        if (string.IsNullOrEmpty(rawName)) return "WpfApp";

        // Regex: Remove anything that isn't a word character (A-Z, 0-9, _) or a hyphen
        return OnlyWordCharHyph().Replace(rawName, "");
    }

    private void ApplySystemThemeAndAccent()
    {
        bool isDarkTheme = false;
        try
        {
            using (RegistryKey key =
                   Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
            {
                if (key != null && key.GetValue("AppsUseLightTheme") is int registryValue)
                {
                    isDarkTheme = (registryValue == 0);
                }
            }
        }
        catch
        {
        }

        string themeFileName = isDarkTheme ? "/WPF/Themes/Theme.Dark.xaml" : "/WPF/Themes/Theme.Light.xaml";

        Color systemAccentColor = GetWindowsAccentColor();
        Color readableTextColor = GetReadableTextColor(systemAccentColor);

        Application.Current.Dispatcher.Invoke(() =>
        {
            Current.Resources.MergedDictionaries[1] = new ResourceDictionary
                { Source = new Uri(themeFileName, UriKind.Relative) };
            SetCustomAccentColor(systemAccentColor, readableTextColor);
        });
    }

    public void SetCustomAccentColor(Color accentColor, Color textColor)
    {
        SolidColorBrush newHighlightBrush = new SolidColorBrush(accentColor);
        SolidColorBrush newHighlightTextBrush = new SolidColorBrush(textColor);

        LinearGradientBrush newTitleBarGradient = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 0)
        };

        newTitleBarGradient.GradientStops.Add(new GradientStop(accentColor, 0.0));
        Color darkerAccent = Color.FromRgb((byte)Math.Max(0, accentColor.R - 40), (byte)Math.Max(0, accentColor.G - 40),
            (byte)Math.Max(0, accentColor.B - 40));
        newTitleBarGradient.GradientStops.Add(new GradientStop(darkerAccent, 1.0));

        Current.Resources[SystemColors.HighlightBrushKey] = newHighlightBrush;
        Current.Resources[SystemColors.HighlightTextBrushKey] = newHighlightTextBrush;
        Current.Resources[SystemColors.ActiveCaptionBrushKey] = newTitleBarGradient;
        Current.Resources[SystemColors.ActiveCaptionTextBrushKey] = newHighlightTextBrush;
    }

    /// <summary>
    /// Clears the custom accent color injections to allow native theme colors to display.
    /// </summary>
    public void ResetToThemeAccent()
    {
        Current.Resources.Remove(SystemColors.HighlightBrushKey);
        Current.Resources.Remove(SystemColors.HighlightTextBrushKey);
        Current.Resources.Remove(SystemColors.ActiveCaptionBrushKey);
        Current.Resources.Remove(SystemColors.ActiveCaptionTextBrushKey);
    }

    private Color GetWindowsAccentColor()
    {
        Color fallbackColor = (Color)ColorConverter.ConvertFromString("#0078D7");
        try
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM"))
            {
                if (key != null && key.GetValue("ColorizationColor") is int colorInt)
                {
                    byte r = (byte)((colorInt >> 16) & 0xFF);
                    byte g = (byte)((colorInt >> 8) & 0xFF);
                    byte b = (byte)(colorInt & 0xFF);
                    return Color.FromArgb(255, r, g, b);
                }
            }
        }
        catch
        {
        }

        return fallbackColor;
    }

    private Color GetReadableTextColor(Color backgroundColor)
    {
        double luminance = (0.299 * backgroundColor.R + 0.587 * backgroundColor.G + 0.114 * backgroundColor.B) / 255;
        return luminance > 0.5 ? Colors.Black : Colors.White;
    }

    [GeneratedRegex(@"[^\w\-]")]
    private static partial Regex OnlyWordCharHyph();
}