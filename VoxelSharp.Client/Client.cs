using DeftSharp.Windows.Input.Keyboard;
using SimpleInjector;
using VoxelSharp.Abstractions.Input;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Abstractions.Window;
using VoxelSharp.Client.Input;
using VoxelSharp.Client.Wrappers;
using VoxelSharp.Core.GameLoop;
using VoxelSharp.Core.Structs;
using VoxelSharp.Core.World;
using VoxelSharp.Renderer;
using VoxelSharp.Renderer.Mesh.World;

namespace VoxelSharp.Client;

public class Client
{
    public Container Container { get; }

    private readonly WorldRenderer _worldRenderer;
    private ModLoaderWrapper ModLoader { get; init; }

    public World World { get; }


    public Client(string[] args)
    {
        Container = new Container();

        // Parse CLI arguments
        ModLoader = LoadMods(GetModsDirectory(args), Container);

        Container.RegisterSingleton<IGameLoop, GameLoop>();

        Container.RegisterSingleton<IMouseRelative, MouseInput>();
        Container.RegisterSingleton<IKeyboardListener, KeyboardListener>();
        Container.RegisterSingleton<ICameraMatricesProvider, FlyingCamera>();
        Container.RegisterSingleton<IWindow, Window>();


        Container.Verify();


        var cameraMatricesProvider = Container.GetInstance<ICameraMatricesProvider>();


        World = new World(2, 16);
        _worldRenderer = new WorldRenderer(World, cameraMatricesProvider);


        World.SetVoxel(new Position<int>(0, 0, 0), new Voxel(Color.Red));
    }

    private static ModLoaderWrapper LoadMods(string modsDirectory, Container container)
    {
        // Ensure the mods directory exists
        if (!Directory.Exists(modsDirectory)) Directory.CreateDirectory(modsDirectory);

        // Initialize ModLoader with the specified directory
        var loader = new ModLoaderWrapper(modsDirectory);

        loader.ModLoader.LoadMods();

        loader.ModLoader.InitializeMods(container);

        return loader;
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

        return modsDirectory;
    }

    public void Run()
    {
        var camera = Container.GetInstance<ICameraMatricesProvider>();
        var mouseRelative = Container.GetInstance<IMouseRelative>();
        var window = Container.GetInstance<IWindow>();

        if (mouseRelative is MouseInput mouseInput)
        {
            mouseInput.StartTracking(new IntPtr(window.WindowHandle));
        }

        // get all IUpdatable and IRenderer instances from the container


        var updatables = new List<IUpdatable>();
        var renderables = new List<IRenderer>();
        var aspectRatioUpdatables = new List<IAspectRatioEventSubscriber>();

        if (camera is IAspectRatioEventSubscriber aspectRatioSubscriber)
            aspectRatioUpdatables.Add(aspectRatioSubscriber);


        updatables.Add(ModLoader);

        if (camera is IUpdatable cameraUpdatable) updatables.Add(cameraUpdatable);

        if (mouseRelative is IUpdatable mouseUpdatable) updatables.Add(mouseUpdatable);


        renderables.AddRange([ModLoader, _worldRenderer]);


        window.OnLoadEvent += (_, _) =>
        {
            ModLoader.InitializeShaders();
            _worldRenderer.InitializeShaders();
        };

        window.OnUpdateEvent += (_, deltaTime) =>
        {
            foreach (var updatable in updatables) updatable.Update((float)deltaTime);
        };

        window.OnRenderEvent += (_, param) =>
        {
            foreach (var renderable in renderables)
            {
                renderable.Render(0);
            }
        };

        window.OnWindowResize += (_, d) =>
        {
            foreach (var subscriber in aspectRatioUpdatables)
            {
                subscriber.UpdateAspectRatio((float)d);
            }
        };

      
    }
}