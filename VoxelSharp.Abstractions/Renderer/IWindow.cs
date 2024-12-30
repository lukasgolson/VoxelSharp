namespace VoxelSharp.Abstractions.Renderer;

public interface IWindow
{
    public (int Width, int Height) ScreenSize { get; }
    
    // OnLoad event
    public event EventHandler OnLoadEvent;
    public event EventHandler<double> OnUpdateEvent;
    public event EventHandler<(ICameraMatrices cameraMatrices, double dTime)> OnRenderEvent;

    public event EventHandler<double> OnWindowResize;


}