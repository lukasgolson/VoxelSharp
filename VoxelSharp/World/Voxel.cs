using VoxelSharp.Structs;

namespace VoxelSharp.World;

public struct Voxel(Color value)
{
    public readonly Color Color = value;
}