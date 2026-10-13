using OpenTK.Graphics.OpenGL;
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
    private static readonly NativeWindowSettings NativeWindowSettings;

    static Window()
    {
        // GLFW's Wayland backend can't query or set window positions, which CenterWindow and
        // ImGui's multi-viewports rely on, so opt out of Wayland and use X11 (XWayland) instead.
        // OpenTK reads this when it initializes GLFW; an explicit value set by the user still wins.
        // This must run before NativeWindowSettings is created, as that already initializes GLFW.
        if (OperatingSystem.IsLinux() && Environment.GetEnvironmentVariable("OPENTK_4_USE_WAYLAND") is null)
            Environment.SetEnvironmentVariable("OPENTK_4_USE_WAYLAND", "0");

        NativeWindowSettings = new NativeWindowSettings
        {
            ClientSize = new Vector2i(1920 / 2, 1080 / 2),
            Title = "VoxelSharp Client",
            Flags = ContextFlags.ForwardCompatible
        };
    }


    public Window(IGameLoop gameLoop) : base(NativeWindowSettings)
    {
        Context.MakeCurrent();

        LoadOpenGL();
        
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
    public event Action? OnFocus;
    public event Action? OnUnfocus;
    
    public bool IsFocused => base.IsFocused;


    public (int Width, int Height) ScreenSize => (Size.X, Size.Y);

    // The GLFW window pointer; GLFW.GetWin32Window would only work on Windows
    public unsafe long WindowHandle => (long)WindowPtr;
    
    protected override void OnFocusedChanged(FocusedChangedEventArgs e)
    {
        base.OnFocusedChanged(e);

        if (e.IsFocused)
        {
            OnFocus?.Invoke();
        }
        else
        {
            OnUnfocus?.Invoke();
        }
    }

    protected void LoadOpenGL()
    {
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        //GL.Disable(EnableCap.CullFace);
        GL.Enable(EnableCap.CullFace); 

        GL.Clear(ClearBufferMask.DepthBufferBit);


        GL.Enable(EnableCap.Blend); // Enable blending for transparency
        GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);

        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, Size.X, Size.Y);

        OnWindowResize?.Invoke(this, (float)Size.X / Size.Y);
    }
}