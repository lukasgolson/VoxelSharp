using System.Windows;
using System.Windows.Input;

namespace VoxelSegmentation.WPF;

/// <summary>
/// A base window class that automatically handles the custom title bar SystemCommands 
/// defined in Theme.Controls.xaml.
/// </summary>
public class ThemedWindow : Window
{

   
    
    public ThemedWindow()
    {
        this.SetResourceReference(StyleProperty, typeof(ThemedWindow));
        
        // Wire up the title bar commands for ANY window that inherits this class
        CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, OnCloseWindow));
        CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, OnMaximizeWindow));
        CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, OnMinimizeWindow));
        CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, OnRestoreWindow));
    }

    private void OnCloseWindow(object sender, ExecutedRoutedEventArgs e) => SystemCommands.CloseWindow(this);
    
    private void OnMaximizeWindow(object sender, ExecutedRoutedEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
            SystemCommands.RestoreWindow(this);
        else
            SystemCommands.MaximizeWindow(this);
    }

    private void OnMinimizeWindow(object sender, ExecutedRoutedEventArgs e) => SystemCommands.MinimizeWindow(this);
    
    private void OnRestoreWindow(object sender, ExecutedRoutedEventArgs e) => SystemCommands.RestoreWindow(this);
}