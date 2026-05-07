using HarmonyLib;
using SimpleInjector;
using VoxelSharp.Abstractions.Client;
using VoxelSharp.Abstractions.Input;
using VoxelSharp.Abstractions.Window;
using VoxelSharp.Client.Input;
using VoxelSharp.Modding.Interfaces;
using VoxelSharp.Modding.Structs;
using VoxelSharp.Renderer; // For the native Window class
using Version = VoxelSharp.Modding.Structs.Version;

namespace VoxelSharp.Host.Native;

public class NativeHost : IMod
{
    public ModInfo ModInfo { get; } = new(
        "Native Host", 
        "net.voxelsharp.host.native", 
        new Version(1, 0, 0), 
        "VoxelSharp",
        priority: 0 // Default Priority
    );

    public bool Initialize(Harmony harmony, Container container)
    {
        // Use standard engine implementations
        container.Options.AllowOverridingRegistrations = true;

        container.RegisterSingleton<IWindow, Window>();
        container.RegisterSingleton<IMouseRelative, MouseInput>();
        container.RegisterSingleton<IClient, NativeClient>();

        container.Options.AllowOverridingRegistrations = false;
        return true;
    }

    public void PostInitialize(Container container) { }
}