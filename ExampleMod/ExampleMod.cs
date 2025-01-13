using ExampleMod.WorldGen;
using HarmonyLib;
using SimpleInjector;
using VoxelSharp.Abstractions.Client;
using VoxelSharp.Client;
using VoxelSharp.Core.Interfaces.WorldGen;
using VoxelSharp.Modding.Interfaces;
using VoxelSharp.Modding.Structs;
using Version = VoxelSharp.Modding.Structs.Version;

namespace ExampleMod;

public class ExampleMod : IMod
{
    public ModInfo ModInfo { get; } = new(
        "ExampleMod",
        "com.voxelsharp.examplemod",
        new Version(1, 0, 0),
        "VoxelSharp"
    );

    public bool PreInitialize(Harmony harmony, Container container)
    {
        return true;
    }

    public bool Initialize(Harmony harmony, Container container)
    {
        container.Options.AllowOverridingRegistrations = true;
        container.RegisterSingleton<IWorldGenerator, BasicWorldGenerator>();


        return true;
    }



    public bool PostInitialize(Container container)
    {
        return true;
    }
}

public struct Dwarf
{
}