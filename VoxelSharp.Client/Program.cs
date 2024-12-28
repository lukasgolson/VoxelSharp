using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using VoxelSharp.Core;
using VoxelSharp.Core.Interfaces;
using VoxelSharp.Core.Structs;
using VoxelSharp.Core.World;
using VoxelSharp.Core.Wrappers;
using VoxelSharp.Renderer;
using VoxelSharp.Renderer.Interfaces;
using VoxelSharp.Renderer.Mesh.World;

namespace VoxelSharp.Client;

class Program
{
    private static void Main(string[] args)
    {
        // Parse CLI arguments
        string modsDirectory = "mods"; // Default directory
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--mods" && i + 1 < args.Length)
            {
                modsDirectory = args[i + 1];
                break;
            }
        }

        // Ensure the mods directory exists
        if (!Directory.Exists(modsDirectory))
        {
            Directory.CreateDirectory(modsDirectory);
        }

        // Initialize ModLoader with the specified directory
        var modLoader = new ModLoaderWrapper(modsDirectory);

        var nativeWindowSettings = new NativeWindowSettings()
        {
            ClientSize = new Vector2i(800, 600),
            Title = "VoxelSharp.CorelSharp.Core",
            Flags = ContextFlags.ForwardCompatible,
        };

        modLoader.ModLoader.LoadMods();


        World _world = new World(2, 16);
        
        _world.SetVoxel(new Position<int>(0,0,0), new Voxel(Color.Red));

        var worldRenderer = new WorldRenderer(_world);
        
        var updatables = new List<IUpdatable> { modLoader };
        var renderables = new List<IRenderer> { modLoader, worldRenderer };

        using var window = new Window(GameWindowSettings.Default, nativeWindowSettings, updatables, renderables);

        window.Run();
    }
}