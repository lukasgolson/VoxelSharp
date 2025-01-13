namespace VoxelSharp.Core.Structs;

public readonly struct Color(byte r, byte g, byte b, byte a) : IEquatable<Color>
{
    public byte R { get; } = r;
    public byte G { get; } = g;
    public byte B { get; } = b;
    public byte A { get; } = a;


    public override bool Equals(object? obj)
    {
        return obj is Color other &&
               R == other.R && G == other.G && B == other.B && A == other.A;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(R, G, B, A);
    }

    public static bool operator ==(Color left, Color right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Color left, Color right)
    {
        return !left.Equals(right);
    }


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
    public static Color Yellow { get; } = new(255, 255, 0, 255);
    public static Color Grey { get; } = new(128, 128, 128, 255);

    public static Color Transparent { get; } = new(0, 0, 0, 0);

    public static Color GetRandomColor(int max = 127)
    {
        if (max is < 0 or > 255)
        {
            throw new ArgumentOutOfRangeException(nameof(max), "Value must be between 0 and 255.");
        }
        
        return new Color((byte)Random.Shared.Next(0, max), (byte)Random.Shared.Next(0, max),
            (byte)Random.Shared.Next(0, max), 255);
    }
}