using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

        // TODO: Add support for multiple windows, non-native windowing, and better control over the window.
        // 1. Check if we are running in a native window or a wrapper (WPF)
        if (window is NativeWindow nativeWindow)
        {
            // Use the original OpenTK native backend
            ImguiImplOpenTk4.Init(nativeWindow);
        }
        else
        {
            // If it's a wrapper, we tell ImGui the host will provide input manually
            unsafe {
                io.NativePtr->BackendPlatformName = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference("wpf_manual_bridge"u8));
            }
            io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;
        }
        
        ImguiImplOpenGl3.Init();

        ImguiImplOpenGl3.Init();
    }


    public void PreRender()
    {
        ImguiImplOpenGl3.NewFrame();
        if (_window is NativeWindow)
        {
            ImguiImplOpenTk4.NewFrame();
        }
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
        if (_window is NativeWindow nativeWindow)
        {
            ImguiImplOpenTk4.Shutdown();
            nativeWindow.Dispose();
        }
        
        ImGui.DestroyContext();
        
        _window.Dispose();
    }
}