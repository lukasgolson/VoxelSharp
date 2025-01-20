namespace VoxelSharp.Resources.Resources;

public class Resource<T>(Address address, T value)
    where T : class
{
   
    
    public Address Address { get; private set; } = address;
    public T Value { get; private set; } = value;

    public static implicit operator T(Resource<T> resource) => resource.Value;


}
