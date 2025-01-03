using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Abstractions.Window;

namespace VoxelSharp.Renderer;

public class Window : GameWindow, IWindow
{
    private readonly ICameraMatrices _cameraMatrices;


    private static readonly NativeWindowSettings NativeWindowSettings = new()
    {
        ClientSize = new Vector2i(1920 / 2, 1080 / 2),
        Title = "VoxelSharp Client",
        Flags = ContextFlags.ForwardCompatible
    };

    private static readonly GameWindowSettings GameWindowSettings = GameWindowSettings.Default;


    public Window(ICameraMatrices cameraMatrices) :
        base(GameWindowSettings, NativeWindowSettings)
    {
        _cameraMatrices = cameraMatrices;
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

    public event EventHandler? OnLoadEvent;
    public event EventHandler<double>? OnUpdateEvent;
    public event EventHandler<(ICameraMatrices cameraMatrices, double dTime)>? OnRenderEvent;
    public event EventHandler<double>? OnWindowResize;


    protected override void OnLoad()
    {
        base.OnLoad();


        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        GL.Disable(EnableCap.CullFace);

        GL.Clear(ClearBufferMask.DepthBufferBit);


        GL.Enable(EnableCap.Blend); // Enable blending for transparency
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha); // Set blending function


        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

        OnWindowResize?.Invoke(this,
            (float)Size.X / Size.Y); // Invoke resize event to set the aspect ratio for any components that need it


        OnLoadEvent?.Invoke(this, EventArgs.Empty);
    }


    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


        OnRenderEvent?.Invoke(this, (_cameraMatrices, e.Time));

        Shader.UnUse();
        SwapBuffers();
    }


    private bool _isF11Pressed;
    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        if (KeyboardState.IsKeyDown(Keys.Escape)) Close();

        if (KeyboardState.IsKeyDown(Keys.F11))
        {
            if (!_isF11Pressed)
            {
                _isF11Pressed = true;
                WindowState = WindowState == WindowState.Fullscreen ? WindowState.Normal : WindowState.Fullscreen;
            }
        }
        else
        {
            _isF11Pressed = false;
        }

        OnUpdateEvent?.Invoke(this, e.Time);
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, Size.X, Size.Y);

        OnWindowResize?.Invoke(this, (float)Size.X / Size.Y);
    }
}