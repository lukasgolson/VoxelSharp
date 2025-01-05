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

    public bool PreInitialize(Harmony harmony, Container container)
    {

        return true;
    }

    public bool Initialize(Harmony harmony, Container container)
    {
        if (container.GetInstance<IClient>() is not Client client)
        {
            return false;
        }
        
        SetCursor(client.World);
        
        return true;
    }

    public bool Update(double deltaTime)
    {
        return true;
    }

    public bool Render()
    {
        return true;
    }

    public void InitializeShaders()
    {
    }


    private static void SetCursor(World world)
    {
        world.SetVoxel(new Position<int>(0, 0, 0), new Voxel(Color.Red)); // Origin in red
        world.SetVoxel(new Position<int>(1, 0, 0), new Voxel(Color.Green)); // X-axis in green
        world.SetVoxel(new Position<int>(0, 1, 0), new Voxel(Color.Blue)); // Y-axis in blue
        world.SetVoxel(new Position<int>(0, 0, 1), new Voxel(Color.Yellow)); // Z-axis in yellow
    }
}