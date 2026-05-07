

using VoxelSharp.Abstractions.Client;
using VoxelSharp.Abstractions.Loop;

namespace VoxelSharp.Host.Native;

public class NativeClient : IClient
{
    private readonly IGameLoop _gameLoop;

    public NativeClient(IGameLoop gameLoop)
    {
        _gameLoop = gameLoop;
    }

    public void Run()
    {
        // Standard blocking native loop
        _gameLoop.Start();
    }
}