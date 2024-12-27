using VoxelSharp.Core.Structs;

namespace VoxelSharp.Core.World;

public readonly struct Voxel(Color value)
{
    public readonly Color Color = value;
}