using HarmonyLib;
using SimpleInjector;
using VoxelSegmentation.structs;
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


    public bool PreInitialize(Harmony harmony, Container container)
    {
        container.RegisterSingleton<PointcloudImporter>();
        container.RegisterSingleton<MainMenuBar>();


        return true;
    }

    public bool Initialize(Harmony harmony, Container container)
    {





        return true;
    }

    public bool PostInitialize(Container container)
    {
        var gameLoop = container.GetInstance<IGameLoop>();
        var pclImporter = container.GetInstance<PointcloudImporter>();
        var bar = container.GetInstance<MainMenuBar>();


        
        gameLoop.RegisterRenderAction(bar, 20);

        gameLoop.RegisterUpdateAction(pclImporter);
      

        return true;
    }

   
}