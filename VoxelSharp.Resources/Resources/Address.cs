namespace VoxelSharp.Resources.Resources;

/// <summary>
/// This struct is used to represent an address to a resource. It is of the form "namespace:path/to/resource".
/// </summary>
public readonly record struct Address
{
    private readonly string _namespace;
    private readonly string _path;
    
    public Address(string address)
    {
        var parts = address.Split(':');
        _namespace = parts[0];
        _path = parts[1];
    }

    public Address(string space, string path)
    {
        _namespace = space;
        _path = path;
    }
   
    
    public override string ToString() => $"{_namespace}:{_path}";
}