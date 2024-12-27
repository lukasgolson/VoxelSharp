using HarmonyLib;
using VoxelSharp.Modding.Interfaces;
using VoxelSharp.Modding.Structs;
using Version = VoxelSharp.Modding.Structs.Version;

namespace ExampleMod;

public class Main : IMod
{
    public ModInfo ModInfo { get; } = new(
        "{ModName}",

        "{ModId}",
        new Version(1, 0, 0),
        "{Author}"
    );

    public bool PreInitialize()
    {
        Console.WriteLine("PreInitialize Called");

        return true;
    }

    public bool Initialize(Harmony harmony)
    {
        Console.WriteLine("Initialize Called");

        return true;
    }

    public bool Update(float deltaTime)
    {
        Console.WriteLine("Update Called");

        return true;
    }

    public bool Render()
    {
        Console.WriteLine("Render Called");

        return true;
    }
}