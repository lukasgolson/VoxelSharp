using ExampleMod.WorldGen;
using HarmonyLib;
using SimpleInjector;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Core.Interfaces.WorldGen;
using VoxelSharp.Modding.Interfaces;
using VoxelSharp.Modding.Structs;
using VoxelSharp.Resources;
using VoxelSharp.Resources.Loading;
using Version = VoxelSharp.Modding.Structs.Version;

namespace ExampleMod;

public class ExampleMod : IMod
{
    private SkyRenderer _skyRenderer;

    public ModInfo ModInfo { get; } = new(
        "ExampleMod",
        "com.voxelsharp.examplemod",
        new Version(1, 0, 0),
        "VoxelSharp"
    );


    public bool Initialize(Harmony harmony, Container container)
    {
        container.Options.AllowOverridingRegistrations = true;
        //container.RegisterSingleton<IWorldGenerator, BasicWorldGenerator>();

        
        container.RegisterSingleton<ILightSource, SkyRenderer>();


        var resourceDictionary = container.GetInstance<ResourceDictionary>();

        var assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var resourcePath = Path.Combine(Path.GetDirectoryName(assemblyPath)!, "Resources");

        resourceDictionary.AddTextResource("ExampleMod:Shaders/Sky.frag",
            Path.Combine(resourcePath, "Shaders/Sky.frag"));
        resourceDictionary.AddTextResource("ExampleMod:Shaders/Sky.vert",
            Path.Combine(resourcePath, "Shaders/Sky.vert"));


     
        
        

        return true;
    }


    public bool PostInitialize(Container container)
    {
        // Get the Resource dictionary from the container


        _skyRenderer = container.GetInstance<ILightSource>() as SkyRenderer;
        var gameLoop = container.GetInstance<IGameLoop>();
        
        gameLoop.RegisterRenderAction(_skyRenderer, 1);
        gameLoop.RegisterUpdateAction(_skyRenderer);
        

        return true;
    }
}