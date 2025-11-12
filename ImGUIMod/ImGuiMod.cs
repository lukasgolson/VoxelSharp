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
    public bool PostInitialize(Container container)
    {
        var gameLoop = container.GetInstance<IGameLoop>();
        var window = container.GetInstance<IWindow>();

        gameLoop.RegisterRenderProcessingAction(new ImGuiController(window));
        
        gameLoop.RegisterRenderAction(new DebugWindow(), 20); // Priority 10 (runs after 3D scene)
        
        return true;
    }
}