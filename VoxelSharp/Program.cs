using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using VoxelSharp;

var nativeWindowSettings = new NativeWindowSettings()
{
    ClientSize = new Vector2i(800, 600),
    Title = "VoxelSharp",
    Flags = ContextFlags.ForwardCompatible,
};

using var window = new Window(GameWindowSettings.Default, nativeWindowSettings);
window.Run();