using VoxelSharp.Structs;

namespace VoxelSharp.World;

public readonly struct Voxel(Color value)
{
    public readonly Color Color = value;
}