using DeftSharp.Windows.Input.Keyboard;
using Microsoft.Extensions.Logging;
using Serilog;
using SimpleInjector;
using VoxelSharp.Abstractions.Client;
using VoxelSharp.Abstractions.Input;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Abstractions.Window;
using VoxelSharp.Client.Input;
using VoxelSharp.Client.Wrappers;
using VoxelSharp.Core.ECS;
using VoxelSharp.Core.GameLoop;
using VoxelSharp.Core.World;
using VoxelSharp.Modding;
using VoxelSharp.Renderer;
using VoxelSharp.Renderer.Rendering;

namespace VoxelSharp.Client;

public static class Program
{
    private static Container Container { get; } = new();
    private static ModLoader? ModLoader { get; set; }

    public static void Main(string[] args)
    {
        var loggerFactory = ConfigureLogging(Container);

        ModLoader = new ModLoader(loggerFactory.CreateLogger<ModLoader>());

        var modsDirectory = GetModsDirectory(args);
        ModLoader.LoadMods(modsDirectory);
        ModLoader.PreInitializeMods(Container);

        
        
        ConfigureServices(Container);
        
        var ecs = new Ecs();
        
        ecs.AddWorldToContainer(Container);
        Container.Verify();


        ModLoader.InitializeMods(Container);


        var client = Container.GetInstance<IClient>();
        client.Run();
        
        
        ecs.Dispose();
    }

    private static ILoggerFactory ConfigureLogging(Container container)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/log.txt")
            .CreateLogger();

        var loggerFactory = LoggerFactory.Create(builder => { builder.AddSerilog(); });

        container.RegisterInstance(loggerFactory);
        container.Register(typeof(ILogger<>), typeof(Logger<>), Lifestyle.Singleton);

        return loggerFactory;
    }


    private static string GetModsDirectory(string[] args)
    {
        var modsDirectory = "mods"; // Default directory
        for (var i = 0; i < args.Length; i++)
            if (args[i] == "--mods" && i + 1 < args.Length)
            {
                modsDirectory = args[i + 1];
                break;
            }

        if (!Directory.Exists(modsDirectory)) Directory.CreateDirectory(modsDirectory);

        return modsDirectory;
    }

    private static void ConfigureServices(Container container)
    {
        container.RegisterInstance(new ModLoaderWrapper(ModLoader));

        container.RegisterSingleton<IGameLoop, GameLoop>();

        container.RegisterSingleton<IMouseRelative, MouseInput>();
        container.RegisterSingleton<IKeyboardListener, KeyboardListener>();
        
        var cameraService = Lifestyle.Singleton.CreateRegistration<FlyingBaseCamera>(container);
        
        container.AddRegistration<ICameraMatrices>(cameraService);
        container.AddRegistration<ICameraParameters>(cameraService);
        
        container.RegisterSingleton<IWindow, Window>();
        container.RegisterSingleton<VoxelWorld>();
        container.RegisterSingleton<WorldRenderer>();
        container.RegisterSingleton<IClient, Client>();
    }

 
}