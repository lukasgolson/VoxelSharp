using System.Numerics;
using ImGuiNET;
using VoxelSharp.Renderer.UI;
using OpenTK.Windowing.Desktop;
using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Abstractions.Window;

namespace ImGUIMod;

public class ImGuiController : IRendererProcessing, IDisposable
{
    private readonly NativeWindow _window;

    public ImGuiController(IWindow window)
    {
        ImGui.CreateContext();
        var io = ImGui.GetIO();


        unsafe
        {
            io.NativePtr->IniFilename = null;
        }
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;

        ImGui.StyleColorsClassic();


        var style = ImGui.GetStyle();
        if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
        {
            style.WindowRounding = 0.0f;
            style.Colors[(int)ImGuiCol.WindowBg].W = 1f;
        }

        _window = (NativeWindow)window;

        ImguiImplOpenTk4.Init(_window);
        ImguiImplOpenGl3.Init();
    }


    public void PreRender()
    {
        ImguiImplOpenGl3.NewFrame();
        ImguiImplOpenTk4.NewFrame();
        ImGui.NewFrame();


        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.WorkPos);
        ImGui.SetNextWindowSize(viewport.WorkSize);
        ImGui.SetNextWindowViewport(viewport.ID);

        const ImGuiWindowFlags hostWindowFlags = ImGuiWindowFlags.NoDocking | // This window won't be dockable itself
                                                 ImGuiWindowFlags.NoTitleBar | // No title
                                                 ImGuiWindowFlags.NoCollapse | // No collapse button
                                                 ImGuiWindowFlags.NoResize | // Not resizable
                                                 ImGuiWindowFlags.NoMove | // Can't be moved
                                                 ImGuiWindowFlags.NoBringToFrontOnFocus | // Don't grab focus
                                                 ImGuiWindowFlags.NoNavFocus | // Don't grab nav focus
                                                 ImGuiWindowFlags
                                                     .NoBackground; // *** 1. FIX: MAKE BACKGROUND TRANSPARENT ***

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);

        ImGui.Begin("DockSpaceHost", hostWindowFlags);

        ImGui.PopStyleVar(2);

        const ImGuiDockNodeFlags dockspaceFlags = ImGuiDockNodeFlags.PassthruCentralNode;
        ImGui.DockSpace(ImGui.GetID("MyDockSpace"), Vector2.Zero, dockspaceFlags);

        
        ImGui.End();
    }

    public void PostRender()
    {
        ImGui.Render();

        ImguiImplOpenGl3.RenderDrawData(ImGui.GetDrawData());

        if (!ImGui.GetIO().ConfigFlags.HasFlag(ImGuiConfigFlags.ViewportsEnable)) return;
        ImGui.UpdatePlatformWindows();
        ImGui.RenderPlatformWindowsDefault();


        _window.Context.MakeCurrent();
    }

    public void Dispose()
    {
        
        ImguiImplOpenGl3.Shutdown();
        ImguiImplOpenTk4.Shutdown();
        ImGui.DestroyContext();
        
        _window.Dispose();
    }
}