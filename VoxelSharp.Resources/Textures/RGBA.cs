namespace VoxelSharp.Resources;

/// <summary>
/// This struct is used to represent an RGBA color.
/// </summary>
/// <param name="r">Red component</param>
/// <param name="g">Green component</param>
/// <param name="b">Blue component</param>
/// <param name="a">Alpha component</param>
public readonly struct Rgba(byte r, byte g, byte b, byte a) : IEquatable<Rgba>
{
    public byte R { get; } = r;
    public byte G { get; } = g;
    public byte B { get; } = b;
    public byte A { get; } = a;


    public override bool Equals(object? obj)
    {
        return obj is Rgba other &&
               R == other.R && G == other.G && B == other.B && A == other.A;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(R, G, B, A);
    }

    public static bool operator ==(Rgba left, Rgba right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Rgba left, Rgba right)
    {
        return !left.Equals(right);
    }


    public bool Equals(Rgba other)
    {
        return R == other.R && G == other.G && B == other.B && A == other.A;
    }

    public static Rgba FromGrayscale(byte intensity)
    {
        return new Rgba(intensity, intensity, intensity, 255);
    }

    public static Rgba Red { get; } = new(255, 0, 0, 255);
    public static Rgba Green { get; } = new(0, 255, 0, 255);
    public static Rgba Blue { get; } = new(0, 0, 255, 255);
    public static Rgba White { get; } = new(255, 255, 255, 255);
    public static Rgba Black { get; } = new(0, 0, 0, 255);
    public static Rgba Yellow { get; } = new(255, 255, 0, 255);
    public static Rgba Grey { get; } = new(128, 128, 128, 255);

    public static Rgba Transparent { get; } = new(0, 0, 0, 0);

    public static Rgba GetRandomColor(int max = 127)
    {
        if (max is < 0 or > 255)
        {
            throw new ArgumentOutOfRangeException(nameof(max), "Value must be between 0 and 255.");
        }
        
        return new Rgba((byte)Random.Shared.Next(0, max), (byte)Random.Shared.Next(0, max),
            (byte)Random.Shared.Next(0, max), 255);
    }
}