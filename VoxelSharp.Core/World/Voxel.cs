using VoxelSharp.Resources;

namespace VoxelSharp.Core.World;

public readonly struct Voxel(Rgba value)
{
    public readonly Rgba Rgba = value;
}