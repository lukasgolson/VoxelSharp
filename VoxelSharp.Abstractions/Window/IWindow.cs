namespace VoxelSharp.Abstractions.Window;

public interface IWindow
{
    public (int Width, int Height) ScreenSize { get; }

    public long WindowHandle { get; }
    
    public bool IsFocused { get; }

    public event EventHandler<double> OnWindowResize;
    
    public event Action? OnFocus; 
    public event Action? OnUnfocus; 
}