namespace VoxelSharp.Modding.Structs;

public readonly struct Version(int major, int minor, int patch) : IComparable<Version>, IEquatable<Version>
{
    public int Major { get; } = major;
    public int Minor { get; } = minor;
    public int Patch { get; } = patch;

    public int CompareTo(Version other)
    {
        // Compare major version
        if (Major != other.Major) return Major.CompareTo(other.Major);

        // Compare minor version
        return Minor != other.Minor
            ? Minor.CompareTo(other.Minor)
            : Patch.CompareTo(other.Patch);
    }

    // Implement a compatible Equals method. If two versions have the same major and minor version, they are considered equal.


    public override bool Equals(object? obj)
    {
        if (obj is Version other) return Major == other.Major && Minor == other.Minor && Patch == other.Patch;

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Major, Minor, Patch);
    }

    public static bool operator ==(Version left, Version right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Version left, Version right)
    {
        return !left.Equals(right);
    }

    public static bool operator <(Version left, Version right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator >(Version left, Version right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator <=(Version left, Version right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >=(Version left, Version right)
    {
        return left.CompareTo(right) >= 0;
    }

    public override string ToString()
    {
        return $"{Major}.{Minor}.{Patch}";
    }

    public bool Equals(Version other)
    {
        return Major == other.Major && Minor == other.Minor && Patch == other.Patch;
    }
}