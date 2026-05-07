using HarmonyLib;
using SimpleInjector;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Window;
using VoxelSharp.Modding.Interfaces;
using VoxelSharp.Modding.Structs;
using Version = VoxelSharp.Modding.Structs.Version;

namespace ImGUIMod;

public class ImGuiMod : IMod
{
    public ModInfo ModInfo { get; } = new("ImGui", "com.voxelsharp.imgui", new Version(1,0,0), "Lukas Olson");
    public bool Initialize(Harmony harmony, Container container)
    {
        container.RegisterSingleton<ImGuiController>();
        return true;
    }

    public bool PostInitialize(Container container)
    {
        var gameLoop = container.GetInstance<IGameLoop>();
        var controller = container.GetInstance<ImGuiController>();

        gameLoop.RegisterRenderProcessingAction(controller, 10);
        gameLoop.RegisterRenderAction(new DebugWindow(gameLoop), 20);
    
        return true;
    }
}