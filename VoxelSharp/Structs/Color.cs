namespace VoxelSharp.Structs;

public readonly struct Color(byte r, byte g, byte b, byte a) : IEquatable<Color>
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

    public override bool Equals(object? obj) => obj is Color other &&
                                                R == other.R && G == other.G && B == other.B && A == other.A;

    public override int GetHashCode() => HashCode.Combine(R, G, B, A);

    public static bool operator ==(Color left, Color right) => left.Equals(right);

    public static bool operator !=(Color left, Color right) => !left.Equals(right);


    public bool Equals(Color other)
    {
        return R == other.R && G == other.G && B == other.B && A == other.A;
    }

    public static Color FromGrayscale(byte intensity)
    {
        return new Color(intensity, intensity, intensity, 255);
    }

    public static Color Red { get; } = new(255, 0, 0, 255);
    public static Color Green { get; } = new(0, 255, 0, 255);
    public static Color Blue { get; } = new(0, 0, 255, 255);
    public static Color White { get; } = new(255, 255, 255, 255);
    public static Color Black { get; } = new(0, 0, 0, 255);

    public static Color Transparent { get; } = new(0, 0, 0, 0);

    public static Color GetRandomColor()
    {
        return new Color((byte)Random.Shared.Next(0, 255), (byte)Random.Shared.Next(0, 255),
            (byte)Random.Shared.Next(0, 255), 255);
    }
}