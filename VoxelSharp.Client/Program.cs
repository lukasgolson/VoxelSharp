using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using VoxelSharp.Client.Wrappers;
using VoxelSharp.Core.Interfaces;
using VoxelSharp.Core.Structs;
using VoxelSharp.Core.World;
using VoxelSharp.Renderer;
using VoxelSharp.Renderer.Interfaces;
using VoxelSharp.Renderer.Mesh.World;

namespace VoxelSharp.Client;

internal class Program
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

        world.SetVoxel(new Position<int>(0, 0, 0), new Voxel(Color.Red));

        var worldRenderer = new WorldRenderer(world);

        var updatables = new List<IUpdatable> { modLoader };
        var renderables = new List<IRenderer> { modLoader, worldRenderer };

        using var window = new Window(updatables, renderables);

        window.Run();
    }
}