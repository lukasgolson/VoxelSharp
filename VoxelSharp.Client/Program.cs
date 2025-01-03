using SimpleInjector;

namespace VoxelSharp.Client;

public static class Program
{
    public static Client Client { get; private set; }
    
    public static void Main(string[] args)
    {
        Client = new Client(args);
        Client.Run();
    }
}