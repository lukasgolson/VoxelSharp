using System.Windows;
using HarmonyLib;
using VoxelSharp.Client;
using VoxelSharp.Core.GameLoop;

namespace VoxelSegmentation;

/// <summary>
/// This patch intercepts the engine's entry point to launch WPF instead of the native window.
/// </summary>
[HarmonyPatch(typeof(Client), nameof(Client.Run))]
public static class ClientRunPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Client __instance)
    {
        // Create the WPF Application instance
        var app = new Application();
        
        // Initialize the MainWindow using the Container stored in your Mod class
        var mainWindow = new MainWindow(VoxelSegmentation.ModContainer);
        
        // This call blocks the main thread and starts the WPF message pump.
        // The engine effectively "lives" inside this call until the window is closed.
        app.Run(mainWindow);
        
        // Return false to skip the original Client.Run() logic (the native GLFW loop).
        return false; 
    }
}

/// <summary>
/// This patch prevents the background GameLoop thread from attempting to render.
/// Since WPF's UI thread now owns the OpenGL context, background rendering would cause a crash.
/// </summary>
[HarmonyPatch(typeof(GameLoop), "RunRender")]
public static class GameLoopRenderPatch
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        // We skip the original RunRender method entirely.
        // The logic for rendering is now handled by MainWindow.OpenTkControl_OnRender.
        // This allows the background thread to keep Ticking (physics/logic) without touching the GPU.
        return false;
    }
}