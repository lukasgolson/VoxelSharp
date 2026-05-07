using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using ImGUIMod;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Wpf;
using SimpleInjector;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Renderer.Rendering;

namespace VoxelSegmentation;

/// <summary>
/// The WPF Host Window. 
/// Acts as the Master Scheduler and Input Translator for the engine.
/// </summary>
public partial class MainWindow : Window
{
    private readonly Container _container;
    private ImGuiController? _imGuiController;
    private WorldRenderer? _worldRenderer;
    private bool _engineStarted;

    public MainWindow(Container container)
    {
        _container = container;
        InitializeComponent();

        // Configure OpenGL Settings for the Viewport
        var settings = new GLWpfControlSettings
        {
            MajorVersion = 3,
            MinorVersion = 3,
            GraphicsProfile = OpenTK.Windowing.Common.ContextProfile.Core
        };

        // Start the control. Ensure your XAML has x:Name="OpenTkControl"
        OpenTkControl.Start(settings);
    }

    /// <summary>
    /// Triggered when the OpenGL context is created and ready for use.
    /// </summary>
    private void OpenTkControl_OnReady()
    {
        // 1. Resolve core engine renderers
        _worldRenderer = _container.GetInstance<WorldRenderer>();
        
        // 2. Resolve ImGui if the mod is present in the mods folder
        try 
        { 
            _imGuiController = _container.GetInstance<ImGuiController>(); 
        } 
        catch (ActivationException) 
        { 
            // ImGuiMod is not loaded; this is fine.
        }

        // 3. Start the background GameLoop (Ticks/Logic only)
        // Our Harmony patch ensures this background thread doesn't touch OpenGL.
        if (!_engineStarted)
        {
            var gameLoop = _container.GetInstance<IGameLoop>();
            Task.Run(() => gameLoop.Start());
            _engineStarted = true;
        }

        // 4. Initial GL State setup
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        GL.Enable(EnableCap.CullFace);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.ClearColor(0.1f, 0.1f, 0.12f, 1.0f);
    }

    /// <summary>
    /// The Master Render Sequence. 
    /// Executed on the WPF UI thread to ensure thread-safe OpenGL access.
    /// </summary>
    private void OpenTkControl_OnRender(TimeSpan deltaTime)
    {
        if (_worldRenderer == null) return;

        // A. Update ImGui's internal state with WPF Input data
        UpdateImGuiInput(deltaTime);

        // B. Signal ImGui that a new frame is starting
        _imGuiController?.PreRender();

        // C. Clear the buffer and render the 3D Engine scene
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _worldRenderer.Render(1.0);

        // D. Render ImGui on top of the 3D scene
        _imGuiController?.PostRender();
    }

    /// <summary>
    /// Translates WPF Mouse/Keyboard and DPI state into ImGui's IO structure.
    /// </summary>
    private void UpdateImGuiInput(TimeSpan deltaTime)
    {
        var io = ImGui.GetIO();

        // 1. Map Viewport Size (DIPs)
        io.DisplaySize = new Vector2((float)OpenTkControl.ActualWidth, (float)OpenTkControl.ActualHeight);

        // 2. Map DeltaTime (Seconds)
        io.DeltaTime = Math.Max((float)deltaTime.TotalSeconds, 0.0001f);

        // 3. Map Mouse Position (Relative to the 3D Viewport)
        var pos = Mouse.GetPosition(OpenTkControl);
        io.MousePos = new Vector2((float)pos.X, (float)pos.Y);

        // 4. Map Mouse Buttons
        io.MouseDown[0] = Mouse.LeftButton == MouseButtonState.Pressed;
        io.MouseDown[1] = Mouse.RightButton == MouseButtonState.Pressed;
        io.MouseDown[2] = Mouse.MiddleButton == MouseButtonState.Pressed;

        // 5. High-DPI Scaling Factor
        var source = PresentationSource.FromVisual(this);
        if (source?.CompositionTarget != null)
        {
            io.DisplayFramebufferScale = new Vector2(
                (float)source.CompositionTarget.TransformToDevice.M11,
                (float)source.CompositionTarget.TransformToDevice.M22
            );
        }
    }

    /// <summary>
    /// Handles locking the mouse when the user clicks the 3D scene.
    /// </summary>
    private void OpenTkControl_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        // Focus the control to capture keyboard events
        OpenTkControl.Focus();

        // Optional: Trigger the engine's camera lock if the mouse is over the scene
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            // You can resolve your camera here to call camera.LockMouse();
        }
    }
}