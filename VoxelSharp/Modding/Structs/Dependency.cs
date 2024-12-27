namespace VoxelSharp.Modding.Structs;

public readonly struct Dependency(string id, Version version)
{
    public string Id { get; } = id;
    public Version Version { get; } = version;
}