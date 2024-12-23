namespace VoxelSharp.Structs;

public readonly struct Color(byte r, byte g, byte b, byte a)
{
    public byte R { get; } = r;
    public byte G { get; } = g;
    public byte B { get; } = b;
    public byte A { get; } = a;


    public byte this[int index] => index switch
    {
        0 => R,
        1 => G,
        2 => B,
        3 => A,
        _ => throw new IndexOutOfRangeException("Invalid index")
    };
    
}