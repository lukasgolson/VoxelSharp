using VoxelSharp.Resources.Resources;

namespace VoxelSharp.Resources.Loading;

public static class TextLoader
{
    
    
    public static void AddTextResource(this ResourceDictionary dictionary, string address, string path)
    {
        Console.WriteLine($"Adding text resource {address} from {path}");
        var addr = new Address(address);
        var text = File.ReadAllText(path);
        var resource = new Resource<string>(addr, text);
        dictionary.AddResource(resource);
    }
}