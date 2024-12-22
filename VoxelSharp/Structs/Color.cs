namespace VoxelSharp.Structs;

public readonly struct Color(float r, float g, float b, float a)
{
    float R { get; } = r;
    float G { get; } = g;
    float B { get; } = b;
    float A { get; } = a;
}