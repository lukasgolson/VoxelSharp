namespace VoxelSharp.Modding.Structs;

public readonly record struct ModInfo(
    string name,
    string id,
    Version version,
    string author,
    Dependency[]? dependencies = null)
{
    public string Id { get; } = id;
    public Version Version { get; } = version;

    public Dependency[]? Dependencies { get; } = dependencies;
}