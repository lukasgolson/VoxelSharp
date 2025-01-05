using DeftSharp.Windows.Input.Keyboard;
using SimpleInjector;
using VoxelSharp.Abstractions.Client;
using VoxelSharp.Abstractions.Input;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Abstractions.Window;
using VoxelSharp.Client.Input;
using VoxelSharp.Client.Wrappers;
using VoxelSharp.Core.GameLoop;
using VoxelSharp.Modding;
using VoxelSharp.Renderer;

namespace VoxelSharp.Client;

public static class Program
{
    private static Container Container { get; } = new();
    private static ModLoader ModLoader { get; } = new();


    public static void Main(string[] args)
    {
        var modsDirectory = GetModsDirectory(args);
        ModLoader.LoadMods(modsDirectory);
        ModLoader.PreInitializeMods(Container);

        RegisterDefaultDependencies();


        ModLoader.InitializeMods(Container);

        
        


        var client = Container.GetInstance<IClient>();
        client.Run();
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

    private static void RegisterDefaultDependencies()
    {
        Container.RegisterInstance(new ModLoaderWrapper(ModLoader));

        Container.RegisterSingleton<IGameLoop, GameLoop>();

        Container.RegisterSingleton<IMouseRelative, MouseInput>();
        Container.RegisterSingleton<IKeyboardListener, KeyboardListener>();
        Container.RegisterSingleton<ICameraMatricesProvider, FlyingCamera>();
        Container.RegisterSingleton<IWindow, Window>();

        Container.RegisterSingleton<IClient, Client>();


        Container.Verify();
    }
}