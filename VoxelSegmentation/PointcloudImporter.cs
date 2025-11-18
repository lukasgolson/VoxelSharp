using VoxelSegmentation.structs;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Core.Structs;
using VoxelSharp.Core.World;
using VoxelSharp.Resources;

namespace VoxelSegmentation;

public class PointcloudImporter : IUpdatable
{
    private VoxelWorld _voxelWorld;

    public PointcloudImporter(VoxelWorld world)
    {
        _voxelWorld = world;
    }

    private bool _ran;

    public void Update(double deltaTime)
    {
        if (_ran) return;
        _ran = true;
        var pcl = new Pointcloud("test.txt");

        var quantized = pcl.Quantize(2);


        foreach (var point in quantized)
        {
            _voxelWorld.SetVoxel(new Position<int>((int)point.X, (int)point.Y, (int)point.Z), new Voxel(Rgba.White));
        }
    }
}