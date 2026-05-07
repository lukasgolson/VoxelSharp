using System.Collections;
using System.IO;
using System.Resources;
using System.Windows;

namespace VoxelSegmentation.WPF.Windows; // Ensure this matches your project namespace

public partial class SettingsWindow : ThemedWindow
{
    // This is the missing symbol! 
    // It maps "Charcoal Dark" -> "pack://application:,,,/VoxelSegmentation;component/WPF/Themes/Theme.Dark.xaml"
    private Dictionary<string, Uri> _availableThemes = new Dictionary<string, Uri>();

    public SettingsWindow()
    {
        InitializeComponent();
        DiscoverThemes();
        SetCurrentTheme();
    }

    private void DiscoverThemes()
    {
        _availableThemes.Clear();
        cmbTheme.Items.Clear();

        // 1. Add the default system option
        _availableThemes.Add("System Auto", null);
        cmbTheme.Items.Add("System Auto");

        // 2. Discover all assemblies to find "Theme.*.xaml" files
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            string assemblyName = assembly.GetName().Name;
            if (string.IsNullOrEmpty(assemblyName) || assemblyName.StartsWith("System") || assemblyName.StartsWith("Microsoft")) 
                continue;

            string resourceName = assemblyName + ".g.resources";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null) continue;

                using (ResourceReader reader = new ResourceReader(stream))
                {
                    foreach (DictionaryEntry entry in reader)
                    {
                        string resourceKey = entry.Key.ToString();

                        // Look for .baml files following our naming convention
                        if (resourceKey.EndsWith(".baml"))
                        {
                            string fileName = Path.GetFileName(resourceKey);
                            if (!fileName.StartsWith("theme.")) continue;

                            string xamlPath = resourceKey.Replace(".baml", ".xaml");
                            Uri themeUri = new Uri($"pack://application:,,,/{assemblyName};component/{xamlPath}");

                            try
                            {
                                var tempDict = new ResourceDictionary { Source = themeUri };
                                if (tempDict.Contains("ThemeDisplayName"))
                                {
                                    string displayName = tempDict["ThemeDisplayName"].ToString();
                                    if (!_availableThemes.ContainsKey(displayName))
                                    {
                                        _availableThemes.Add(displayName, themeUri);
                                        cmbTheme.Items.Add(displayName);
                                    }
                                }
                            }
                            catch { /* Skip non-dictionary resources */ }
                        }
                    }
                }
            }
        }
    }

    private void SetCurrentTheme()
    {
        if (Application.Current is App app)
        {
            string currentTheme = app.CurrentManualTheme ?? "System Auto";
            
            if (cmbTheme.Items.Contains(currentTheme))
                cmbTheme.SelectedItem = currentTheme;
            else
                cmbTheme.SelectedIndex = 0;
        }
    }

    private void ApplySettings()
    {
        if (cmbTheme.SelectedItem != null)
        {
            string selectedDisplayName = cmbTheme.SelectedItem.ToString();
            
            // Retrieve the URI we discovered earlier
            Uri themeUri = _availableThemes[selectedDisplayName];

            if (Application.Current is App app)
            {
                // Pass both the name and the discovered URI to the App class
                app.ChangeUserTheme(selectedDisplayName, themeUri); 
            }
        }
    }

    private void BtnOK_Click(object sender, RoutedEventArgs e)
    {
        ApplySettings();
        this.DialogResult = true;
        this.Close();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        this.DialogResult = false;
        this.Close();
    }
}