using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Client.Input;
using VoxelSharp.Client.Wrappers;
using VoxelSharp.Core.World;
using VoxelSharp.Modding;
using VoxelSharp.Renderer;
using VoxelSharp.Renderer.Mesh.World;

namespace VoxelSharp.Client;

public class Client
{
    private readonly MouseInput _mouseInput;
    private readonly WorldRenderer _worldRenderer;
    private ModLoaderWrapper ModLoader { get; init; }

    public World World { get; }

    public List<IUpdatable> Updatables { get; } = [];
    public List<IRenderer> Renderables { get; } = [];

    public Client(string[] args)
    {
        // Parse CLI arguments
        ModLoader = LoadMods(args);

        World = new World(2, 16);
        _mouseInput = new MouseInput();
        _worldRenderer = new WorldRenderer(World);
    }

    private ModLoaderWrapper LoadMods(string[] args)
    {
        var modsDirectory = GetModsDirectory(args);

        // Ensure the mods directory exists
        if (!Directory.Exists(modsDirectory)) Directory.CreateDirectory(modsDirectory);

        // Initialize ModLoader with the specified directory
        var loader = new ModLoaderWrapper(modsDirectory);

        loader.ModLoader.LoadMods();

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
        var camera = new FlyingCamera(16f / 9f, _mouseInput);


        using var window = new Window(camera);

        _mouseInput.StartTracking(new IntPtr(window.WindowHandle));


        Updatables.AddRange([ModLoader, camera, _mouseInput]);
        Renderables.AddRange([ModLoader, _worldRenderer]);


        window.OnLoadEvent += (_, _) =>
        {
            ModLoader.InitializeShaders();
            _worldRenderer.InitializeShaders();
        };

        window.OnUpdateEvent += (_, deltaTime) =>
        {
            foreach (var updatable in Updatables) updatable.Update((float)deltaTime);
        };

        window.OnRenderEvent += (_, param) =>
        {
            foreach (var renderable in Renderables)
            {
                renderable.Render(param.cameraMatrices);
            }
        };

        window.OnWindowResize += (_, d) => { camera.UpdateAspectRatio((float)d); };

        window.Run();
    }
}