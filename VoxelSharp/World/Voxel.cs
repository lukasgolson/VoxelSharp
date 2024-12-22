using VoxelSharp.Structs;

namespace VoxelSharp.World;

public struct Voxel(Color value)
{
    private readonly Color _color = value;
}