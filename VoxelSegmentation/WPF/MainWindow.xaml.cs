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
using VoxelSharp.Abstractions.Window;
using VoxelSharp.Core.World;
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
    
    private bool _isReady = false; 

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

    private void OpenTkControl_OnReady()
    {
        try 
        {
            var gameLoop = _container.GetInstance<IGameLoop>();
            
            // --- ADD THESE 3 LINES ---
            // Resolve the renderer and the world, then introduce them to each other
            _worldRenderer = _container.GetInstance<WorldRenderer>();
            var voxelWorld = _container.GetInstance<VoxelWorld>();
            _worldRenderer.AssociateWorld(voxelWorld);
            // -------------------------

            // TELL THE ENGINE WE ARE TAKING OVER RENDERING
            gameLoop.IsRenderDecoupled = true; 
            
            // Start the background logic thread
            Task.Run(() => 
            {
                try 
                {
                    gameLoop.Start();
                }
                catch (Exception ex)
                {
                    // If the background thread dies, tell us why!
                    MessageBox.Show(ex.ToString(), "Background Thread Fatal Crash");
                }
            });

            _isReady = true; 
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Initialization Failed: {ex.Message}");
        }
        
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        GL.Enable(EnableCap.CullFace);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.ClearColor(0.1f, 0.1f, 0.12f, 1.0f);
    }
    
    private void OpenTkControl_OnRender(TimeSpan deltaTime)
    {
        if (!_isReady) return;

        // Feed input to ImGui
        UpdateImGuiInput(deltaTime); 

        // Clear the screen for the new frame
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        // Command the engine to fire its rendering pipeline!
        // This will automatically run ImGui.PreRender -> WorldRenderer -> ImGui.PostRender
        var gameLoop = _container.GetInstance<IGameLoop>();
        gameLoop.RenderFrame(1.0); 
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
    
    private void OpenTkControl_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Don't try to resize if the container/engine isn't ready yet
        if (!_isReady) return;

        // Resolve the wrapper from the container
        var windowWrapper = _container.GetInstance<IWindow>() as WpfWindowWrapper;
        
        if (windowWrapper != null)
        {
            // Pass the new WPF dimensions to the wrapper.
            // This will trigger windowWrapper.OnWindowResize, which the 
            // FlyingBaseCamera is listening to, automatically recalculating the projection matrix!
            windowWrapper.TriggerResize((int)e.NewSize.Width, (int)e.NewSize.Height);
        }
    }
}