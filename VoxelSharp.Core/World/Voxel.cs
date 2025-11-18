using VoxelSharp.Resources;

namespace VoxelSharp.Core.World;

public readonly struct Voxel(Rgba value)
{
    public readonly Rgba Rgba = value;


    public bool IsEmpty => Rgba.IsTransparent;
    public static Voxel Empty = new(Rgba.Transparent);
}