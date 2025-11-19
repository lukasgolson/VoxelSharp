using System.Runtime.InteropServices;
using VoxelSharp.Core.World;

namespace VoxelSharp.Renderer.Mesh.World;

[StructLayout(LayoutKind.Explicit, Size = 8)]
public readonly struct VoxelVertex
{


    // Word 0: Position + Metadata
    // X: 5 bits, Y: 5 bits, Z: 5 bits, Face: 3 bits
    [FieldOffset(0)] public readonly uint Data;

    // Word 1: Color
    [FieldOffset(4)] public readonly uint Color;

    public VoxelVertex(uint data, uint color)
    {
        Data = data;
        Color = color;
    }


}