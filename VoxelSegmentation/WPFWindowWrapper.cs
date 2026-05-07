using System.Windows;
using System.Windows.Interop;
using VoxelSharp.Abstractions.Window;

namespace VoxelSegmentation;

public class WpfWindowWrapper : IWindow
{
    public (int Width, int Height) ScreenSize { get; set; } = (1920, 1080);
    
    public long WindowHandle 
    {
        get 
        {
            if (Application.Current?.MainWindow == null) return 0;
            return new WindowInteropHelper(Application.Current.MainWindow).Handle.ToInt64();
        }
    }

    public bool IsFocused => Application.Current?.MainWindow?.IsActive ?? false;

    public event EventHandler<double>? OnWindowResize;
    public event Action? OnFocus;
    public event Action? OnUnfocus;

    public void TriggerResize(int w, int h)
    {
        ScreenSize = (w, h);
        OnWindowResize?.Invoke(this, (double)w / h);
    }
}