using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using VoxelSharp.Core.Interfaces;
using VoxelSharp.Core.Wrappers;

namespace VoxelSharp.Core;

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
            Title = "VoxelSharp.Core",
            Flags = ContextFlags.ForwardCompatible,
        };

        modLoader.ModLoader.LoadMods();

        var updatables = new List<IUpdatable> { modLoader };
        var renderables = new List<IRenderable> { modLoader };

        using var window = new Window(GameWindowSettings.Default, nativeWindowSettings, updatables, renderables);

        window.Run();
    }
}