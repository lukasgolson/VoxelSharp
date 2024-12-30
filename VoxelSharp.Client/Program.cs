using DeftSharp.Windows.Input.Keyboard;
using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Client.Wrappers;
using VoxelSharp.Core.Camera;
using VoxelSharp.Core.Structs;
using VoxelSharp.Core.World;
using VoxelSharp.Renderer;
using VoxelSharp.Renderer.Mesh.World;

namespace VoxelSharp.Client;

internal static class Program
{
    private static void Main(string[] args)
    {
        // Parse CLI arguments
        var modsDirectory = "mods"; // Default directory
        for (var i = 0; i < args.Length; i++)
            if (args[i] == "--mods" && i + 1 < args.Length)
            {
                modsDirectory = args[i + 1];
                break;
            }

        // Ensure the mods directory exists
        if (!Directory.Exists(modsDirectory)) Directory.CreateDirectory(modsDirectory);

        // Initialize ModLoader with the specified directory
        var modLoader = new ModLoaderWrapper(modsDirectory);


        modLoader.ModLoader.LoadMods();


        var world = new World(2, 16);

        world.SetVoxel(worldPos: new Position<int>(0, 0, 0), voxel: new Voxel(Color.Red));

        var worldRenderer = new WorldRenderer(world);

        
        
        
        var camera = new FlyingCamera(16f/9f); 

        var updatables = new List<IUpdatable> { modLoader, camera };
        var renderables = new List<IRenderer> { modLoader, worldRenderer };

        using var window = new Window(camera);

        window.OnLoadEvent += (_, _) =>
        {
            modLoader.InitializeShaders();
            worldRenderer.InitializeShaders();
        };

        window.OnUpdateEvent += (_, deltaTime) =>
        {
            foreach (var updatable in updatables) updatable.Update((float)deltaTime);
        };

        window.OnRenderEvent += (_, param) =>
        {
            foreach (var renderable in renderables)
            {
                renderable.Render(param.cameraMatrices);
            }
        };

        window.OnWindowResize += (_, d) =>
        {
            camera.UpdateAspectRatio((float)d);
            
        };

        window.Run();
    }
}