using System.Reflection;
using System.Windows;
using VoxelSegmentation.WPF;

namespace Downloader;

public partial class AboutWindow : ThemedWindow
{
    public AboutWindow()
    {
        InitializeComponent();
        LoadAssemblyDetails();
    }

    private void LoadAssemblyDetails()
    {
        var assembly = Assembly.GetExecutingAssembly();

        // 1. Get the App Title
        var titleAttr = assembly.GetCustomAttribute<AssemblyProductAttribute>();
        lblTitle.Text = !string.IsNullOrEmpty(titleAttr?.Product)
            ? titleAttr.Product
            : assembly.GetName().Name ?? "Program";

        // 2. Get the Version
        var version = assembly.GetName().Version;
        lblVersion.Text = $"Version {version?.Major}.{version?.Minor}.{version?.Build}";

        // 3. Get the Company (Replacing Copyright)
        var companyAttr = assembly.GetCustomAttribute<AssemblyCompanyAttribute>();
        lblCopyright.Text = !string.IsNullOrEmpty(companyAttr?.Company)
            ? $"© {companyAttr.Company}"
            : "© Unknown Company";

        // 4. Get the Description
        var descriptionAttr = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
        lblDescription.Text = descriptionAttr?.Description ?? "An application.";
    }

    private void BtnOK_Click(object sender, RoutedEventArgs e)
    {
        // Close the dialog and return to the main window
        this.Close();
    }
}