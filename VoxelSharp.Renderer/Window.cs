using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Abstractions.Window;

namespace VoxelSharp.Renderer;

public class Window : NativeWindow, IWindow, IRendererProcessing, IUpdatable
{
    private static readonly NativeWindowSettings NativeWindowSettings = new()
    {
        ClientSize = new Vector2i(1920 / 2, 1080 / 2),
        Title = "VoxelSharp Client",
        Flags = ContextFlags.ForwardCompatible
    };


    public Window(IGameLoop gameLoop) : base(NativeWindowSettings)
    {
        Context.MakeCurrent();

        Load();

        CenterWindow();

        gameLoop.RegisterRenderProcessingAction(this);
        gameLoop.RegisterUpdateAction(this);
    }

    public void PreRender()
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }

    public void PostRender()
    {
        Shader.Unuse();
        Context.SwapBuffers();
    }

    public void Update(double deltaTime)
    {
        ProcessWindowEvents(IsEventDriven);
    }

    public event EventHandler<double>? OnWindowResize;


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
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, Size.X, Size.Y);

        OnWindowResize?.Invoke(this, (float)Size.X / Size.Y);
    }
}