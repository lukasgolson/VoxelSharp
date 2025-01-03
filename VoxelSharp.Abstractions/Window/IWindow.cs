using VoxelSharp.Abstractions.Renderer;

namespace VoxelSharp.Abstractions.Window;

public interface IWindow
{
    public (int Width, int Height) ScreenSize { get; }
    
    public long WindowHandle { get; }
    
    // OnLoad event
    public event EventHandler OnLoadEvent;
    public event EventHandler<double> OnUpdateEvent;
    public event EventHandler<(ICameraMatricesProvider cameraMatrices, double interpolation)> OnRenderEvent;

    public event EventHandler<double> OnWindowResize;


}