namespace VoxelSharp.Resources;

/// <summary>
/// This struct is used to represent an address to a resource. It is of the form "namespace:path/to/resource".
/// </summary>
public struct Address
{
    private readonly string _namespace;
    private readonly string _path;
    
    public Address(string address)
    {
        var parts = address.Split(':');
        _namespace = parts[0];
        _path = parts[1];
    }
    
    public override string ToString() => $"{_namespace}:{_path}";
}