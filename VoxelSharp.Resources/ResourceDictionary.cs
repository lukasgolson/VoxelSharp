using VoxelSharp.Resources.Resources;

namespace VoxelSharp.Resources;

public class ResourceDictionary
{
    private readonly Dictionary<Address, IResource> _resources = new();


    public void AddResource(Resource<object> resource)
    {
        _resources.Add(resource.Address, resource);
    }

    public void AddResource<T>(Resource<T> resource) where T : class
    {
        _resources.Add(resource.Address, resource);
    }


    public Resource<T> GetResource<T>(Address resource) where T : class
    {
        return (Resource<T>)_resources[resource];
    }
}