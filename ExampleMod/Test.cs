using HarmonyLib;
using SimpleInjector;
using VoxelSharp.Abstractions.Client;
using VoxelSharp.Client;
using VoxelSharp.Core.Structs;
using VoxelSharp.Core.World;
using VoxelSharp.Modding.Interfaces;
using VoxelSharp.Modding.Structs;
using Version = VoxelSharp.Modding.Structs.Version;

namespace ExampleMod;

public class Test : IMod
{
    public ModInfo ModInfo { get; } = new(
        "ExampleMod",
        "com.voxelsharp.examplemod",
        new Version(1, 0, 0),
        "VoxelSharp"
    );

    public bool PreInitialize(Container container)
    {
        Console.WriteLine("PreInitialize Called");

        return true;
    }

    private Container _container;

    public bool Initialize(Harmony harmony, Container container)
    {
        Console.WriteLine("Initialize Called");

        _container = container;

        return true;
    }


    private bool triggered = false;

    public bool Update(double deltaTime)
    {
        if (!triggered)
        {
            var client = _container.GetInstance<IClient>() as Client;
            SetCursor(client.World);
        }

        return true;
    }

    public bool Render()
    {
        return true;
    }

    public void InitializeShaders()
    {
        Console.WriteLine("InitializeShaders Called");
    }


    private void SetCursor(World world)
    {
        world.SetVoxel(worldPos: new Position<int>(0, 0, 0), voxel: new Voxel(Color.Red)); // Origin in red
        world.SetVoxel(worldPos: new Position<int>(1, 0, 0), voxel: new Voxel(Color.Green)); // X-axis in green
        world.SetVoxel(worldPos: new Position<int>(0, 1, 0), voxel: new Voxel(Color.Blue)); // Y-axis in blue
        world.SetVoxel(worldPos: new Position<int>(0, 0, 1), voxel: new Voxel(Color.Yellow)); // Z-axis in yellow
    }
}