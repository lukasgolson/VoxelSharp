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
        if (container.GetInstance<IClient>() is not Client client)
        {
            return false;
        }
        
        SetCursor(client.VoxelWorld);

        CreateHollowCube(client.VoxelWorld, new Position<int>(5, 5, 5), 8);

        
     

      
        
        return true;
    }


    private void CreateHollowCube(VoxelWorld clientVoxelWorld, Position<int> position, int sideLength)
    {
        // Calculate half the size for centering
        int halfSize = sideLength / 2;

        for (int x = -halfSize; x <= halfSize; x++)
        {
            for (int y = -halfSize; y <= halfSize; y++)
            {
                for (int z = -halfSize; z <= halfSize; z++)
                {
                    // Determine if the current voxel is on the surface of the cube
                    bool isOnSurface = (x == -halfSize || x == halfSize ||
                                        y == -halfSize || y == halfSize ||
                                        z == -halfSize || z == halfSize);

                    if (isOnSurface)
                    {
                        // Set voxel at the calculated position
                        clientVoxelWorld.SetVoxel(
                            new Position<int>(
                                position.X + x,
                                position.Y + y,
                                position.Z + z
                            ),
                            new Voxel(Color.GetRandomColor())
                        );
                    }
                }
            }
        }
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


    private static void SetCursor(VoxelWorld voxelWorld)
    {
        voxelWorld.SetVoxel(new Position<int>(0, 0, 0), new Voxel(Color.Red)); // Origin in red
        voxelWorld.SetVoxel(new Position<int>(1, 0, 0), new Voxel(Color.Green)); // X-axis in green
        voxelWorld.SetVoxel(new Position<int>(0, 1, 0), new Voxel(Color.Blue)); // Y-axis in blue
        voxelWorld.SetVoxel(new Position<int>(0, 0, 1), new Voxel(Color.Yellow)); // Z-axis in yellow
    }
}

public struct Dwarf
{
}