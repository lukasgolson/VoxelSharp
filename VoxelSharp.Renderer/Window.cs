using System.Runtime.InteropServices;
using ImGuiNET;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Abstractions.Window;
using VoxelSharp.Renderer.UI;
using VoxelSharp.Resources;

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

        LoadOpenGL();
        
        LoadImGui();

        CenterWindow();

        gameLoop.RegisterRenderProcessingAction(this);
        gameLoop.RegisterUpdateAction(this);
    }

    private void LoadImGui()
    {
        ImGui.CreateContext();
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;

        ImGui.StyleColorsClassic();
        

        ImGuiStylePtr style = ImGui.GetStyle();
        if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
        {
            style.WindowRounding = 0.0f;
            style.Colors[(int)ImGuiCol.WindowBg].W = 0.25f;
        }

        ImguiImplOpenTk4.Init(this);
        ImguiImplOpenGl3.Init();
    }

    public void PreRender()
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }

    public void PostRender()
    {
        Shader.Unuse();
        
        RenderImGui();

        
        Context.SwapBuffers();
    }


    private void RenderImGui()
    {
        ImguiImplOpenGl3.NewFrame();
        ImguiImplOpenTk4.NewFrame();
        ImGui.NewFrame();
        
        
        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.WorkPos);
        ImGui.SetNextWindowSize(viewport.WorkSize);
        ImGui.SetNextWindowViewport(viewport.ID);

        ImGuiWindowFlags hostWindowFlags = 
            ImGuiWindowFlags.NoDocking |                 // This window won't be dockable itself
            ImGuiWindowFlags.NoTitleBar |                // No title
            ImGuiWindowFlags.NoCollapse |                // No collapse button
            ImGuiWindowFlags.NoResize |                  // Not resizable
            ImGuiWindowFlags.NoMove |                    // Can't be moved
            ImGuiWindowFlags.NoBringToFrontOnFocus |     // Don't grab focus
            ImGuiWindowFlags.NoNavFocus |                // Don't grab nav focus
            ImGuiWindowFlags.NoBackground;               // *** 1. FIX: MAKE BACKGROUND TRANSPARENT ***

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, (System.Numerics.Vector2)Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        
        ImGui.Begin("DockSpaceHost", hostWindowFlags);
        
        ImGui.PopStyleVar(2);

        ImGuiDockNodeFlags dockspaceFlags = ImGuiDockNodeFlags.PassthruCentralNode; 
        ImGui.DockSpace(ImGui.GetID("MyDockSpace"), (System.Numerics.Vector2)Vector2.Zero, dockspaceFlags);

        ImGui.End();


    
        //ImGui.ShowDemoWindow();


        ImGui.Render();
   
        ImguiImplOpenGl3.RenderDrawData(ImGui.GetDrawData());

        if (ImGui.GetIO().ConfigFlags.HasFlag(ImGuiConfigFlags.ViewportsEnable))
        {
            ImGui.UpdatePlatformWindows();
            ImGui.RenderPlatformWindowsDefault();
        
          
            Context.MakeCurrent(); 
        }
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

    public unsafe long WindowHandle
    {
        get
        {
            var windowHandle = GLFW.GetWin32Window(WindowPtr);
            return windowHandle.ToInt64();
        }
    }
    
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