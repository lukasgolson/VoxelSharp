using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Renderer.Camera;
using VoxelSharp.Renderer.Interfaces;

namespace VoxelSharp.Renderer;

public class Window : GameWindow, IWindow
{
    private readonly FlyingCamera _camera;
    private readonly List<IRenderer> _renderers;

    private readonly List<IUpdatable> _updatables;

    private static readonly NativeWindowSettings NativeWindowSettings = new()
    {
        ClientSize = new Vector2i(800, 600),
        Title = "VoxelSharp Client",
        Flags = ContextFlags.ForwardCompatible
    };

    private static readonly GameWindowSettings GameWindowSettings = GameWindowSettings.Default;

    public Window(List<IUpdatable>? updatables = null, List<IRenderer>? renderers = null) :
        base(GameWindowSettings, NativeWindowSettings)
    {
        _camera = new FlyingCamera((float)Size.X / Size.Y);


        _updatables = updatables ?? [];
        _renderers = renderers ?? [];
    }


    public (int Width, int Height) ScreenSize => (Size.X, Size.Y);


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

        foreach (var renderer in _renderers) renderer.InitializeShaders();
    }


    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        foreach (var renderer in _renderers) renderer.Render(_camera);

        Shader.UnUse();


        SwapBuffers();
    }


    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        if (KeyboardState.IsKeyDown(Keys.Escape)) Close();

        // Handle movement inputs
        if (KeyboardState.IsKeyDown(Keys.W)) _camera.MoveForward();
        if (KeyboardState.IsKeyDown(Keys.S)) _camera.MoveBackward();
        if (KeyboardState.IsKeyDown(Keys.A)) _camera.MoveLeft();
        if (KeyboardState.IsKeyDown(Keys.D)) _camera.MoveRight();
        if (KeyboardState.IsKeyDown(Keys.Space)) _camera.MoveUp();
        if (KeyboardState.IsKeyDown(Keys.LeftShift)) _camera.MoveDown();

        // Handle mouse inputs for camera rotation
        var (deltaX, deltaY) = MouseState.Delta;
        _camera.UpdateRotation(deltaX, -deltaY);

        // Update camera state
        _camera.Update((float)e.Time);

        foreach (var updatable in _updatables) updatable.Update((float)e.Time);
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, Size.X, Size.Y);

        // Update projection matrix for the new aspect ratio
        _camera.UpdateAspectRatio((float)Size.X / Size.Y);
    }
}