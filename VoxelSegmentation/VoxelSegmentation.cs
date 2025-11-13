using SimpleInjector;
using VoxelSegmentation.UI;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Modding.Interfaces;
using VoxelSharp.Modding.Structs;
using Version = VoxelSharp.Modding.Structs.Version;

namespace VoxelSegmentation;

public class VoxelSegmentation : IMod
{
    public ModInfo ModInfo { get; } = new("VoxelSegmentation", "net.lukasolson.vs", new Version(1, 0, 0), "Lukas Olson",
    [
        new Dependency("com.voxelsharp.imgui", new Version(1, 0, 0))
    ]);

    public bool PostInitialize(Container container)
    {
        var gameLoop = container.GetInstance<IGameLoop>();
        gameLoop.RegisterRenderAction(new MainMenuBar(), 20); // Priority 10 (runs after 3D scene)


        return true;
    }
}