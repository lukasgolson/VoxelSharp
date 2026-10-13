namespace VoxelSharp.Resources.Resources;

public class Resource<T>(Address address, T value) : IResource
    where T : class
{
    public Address Address { get; } = address;
    public T Value { get; private set; } = value;

    public static implicit operator T(Resource<T> resource) => resource.Value;
}

internal interface IResource
{
    public Address Address { get; }
}