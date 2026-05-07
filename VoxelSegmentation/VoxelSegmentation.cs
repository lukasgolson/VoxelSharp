using HarmonyLib;
using SimpleInjector;
using VoxelSharp.Abstractions.Client;
using VoxelSharp.Abstractions.Window;
using VoxelSharp.Modding.Interfaces;
using VoxelSharp.Modding.Structs;

namespace VoxelSegmentation;

public class VoxelSegmentation : IMod
{
    public static Container ModContainer { get; private set; }
    public ModInfo ModInfo { get; } = new(
        "WPF Host",
        "net.voxelsharp.host.wpf",
        new VoxelSharp.Modding.Structs.Version(1, 0, 0),
        "Lukas Olson",
        priority: 100 // High priority to override the Native Host
    );

    public bool Initialize(Harmony harmony, Container container)
    {
        ModContainer = container;
        
        // We MUST enable overriding so we can kick the Native Host out of the container
        container.Options.AllowOverridingRegistrations = true;

        container.RegisterSingleton<IWindow, WpfWindowWrapper>();
        container.RegisterSingleton<IClient, WpfClient>();

        container.Options.AllowOverridingRegistrations = false;

        // Apply our Harmony patches (to silence the background GameLoop render, etc.)
        harmony.PatchAll();

        return true;
    }

    public bool PostInitialize(Container container)
    {
        return true;
    }
}