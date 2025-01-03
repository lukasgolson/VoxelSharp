using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Abstractions.Window;
using VoxelSharp.Renderer.Interfaces;

namespace VoxelSharp.Renderer;

public class Window : NativeWindow, IWindow, IRenderer
{
    private readonly ICameraMatricesProvider _cameraMatricesProvider;

    public event EventHandler? OnLoadEvent;
    public event EventHandler<double>? OnUpdateEvent;
    public event EventHandler<(ICameraMatricesProvider cameraMatrices, double interpolation)>? OnRenderEvent;
    public event EventHandler<double>? OnWindowResize;


    private static readonly NativeWindowSettings NativeWindowSettings = new()
    {
        ClientSize = new Vector2i(1920 / 2, 1080 / 2),
        Title = "VoxelSharp Client",
        Flags = ContextFlags.ForwardCompatible
    };


    public Window(ICameraMatricesProvider cameraMatricesProvider) :
        base(NativeWindowSettings)
    {
        _cameraMatricesProvider = cameraMatricesProvider;

        Context.MakeCurrent();


        Load();


        CenterWindow();
    }


    public (int Width, int Height) ScreenSize => (Size.X, Size.Y);

    public unsafe long WindowHandle
    {
        get
        {
            var windowHandle = GLFW.GetWin32Window(WindowPtr);
            return windowHandle.ToInt64();
        }
    }

  


    protected void Load()
    {
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        GL.Disable(EnableCap.CullFace);

        GL.Clear(ClearBufferMask.DepthBufferBit);


        GL.Enable(EnableCap.Blend); // Enable blending for transparency
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha); // Set blending function


        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

        OnLoadEvent?.Invoke(this, EventArgs.Empty);
    }


    public void InitializeShaders()
    {
    }

    void IRenderer.Render(double interpolationFactor)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


        OnRenderEvent?.Invoke(this, (_cameraMatricesProvider, interpolationFactor));

        Shader.UnUse();
        Context.SwapBuffers();
    }


    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, Size.X, Size.Y);

        OnWindowResize?.Invoke(this, (float)Size.X / Size.Y);
    }
}