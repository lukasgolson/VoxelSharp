namespace VoxelSharp.Abstractions.Window;

public interface IWindow
{
    public (int Width, int Height) ScreenSize { get; }
    
    public long WindowHandle { get; }
    
    public event EventHandler<double> OnWindowResize;


}