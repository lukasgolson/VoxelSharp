namespace VoxelSharp.Modding.Structs;

public readonly struct ModInfo(
    string name,
    string id,
    Version version,
    string author,
    Dependency[]? dependencies = null)
    : IEquatable<ModInfo>
{
    public string Id { get; } = id;
    public Version Version { get; } = version;

    public Dependency[] Dependencies { get; } = dependencies ?? [];
    public string Author { get; } = author;

    public bool Equals(ModInfo other)
    {
        return Id == other.Id && Version.Equals(other.Version) && Dependencies.Equals(other.Dependencies) &&
               Author == other.Author;
    }

    public override bool Equals(object? obj)
    {
        return obj is ModInfo other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Version, Dependencies, Author);
    }

    public static bool operator ==(ModInfo left, ModInfo right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ModInfo left, ModInfo right)
    {
        return !left.Equals(right);
    }
}