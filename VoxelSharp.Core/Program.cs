using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using VoxelSharp.Core;
using VoxelSharp.Core.Interfaces;
using VoxelSharp.Core.Wrappers;

var nativeWindowSettings = new NativeWindowSettings()
{
    ClientSize = new Vector2i(800, 600),
    Title = "VoxelSharp.Core",
    Flags = ContextFlags.ForwardCompatible,
};

if (!Directory.Exists("mods"))
{
    Directory.CreateDirectory("mods");
}

var modLoader = new ModLoaderWrapper("mods");

modLoader.ModLoader.LoadMods();


var updatables = new List<IUpdatable> { modLoader };
var renderables = new List<IRenderable> { modLoader };

using var window = new Window(GameWindowSettings.Default, nativeWindowSettings, updatables, renderables);

window.Run();