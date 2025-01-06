using System.Windows.Input;
using DeftSharp.Windows.Input.Keyboard;
using Microsoft.Extensions.Logging;
using VoxelSharp.Abstractions.Client;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Client.Wrappers;
using VoxelSharp.Core.Structs;
using VoxelSharp.Core.World;
using VoxelSharp.Renderer.Rendering;

namespace VoxelSharp.Client;

public class Client : IClient
{
    private readonly IGameLoop _gameLoop;
    private readonly IKeyboardListener _keyboardListener;
    private readonly ModLoaderWrapper _modloaderWrapper;
    public readonly World World;
    private readonly WorldRenderer _worldRenderer;


    private readonly ILogger<Client> _logger;

    public Client(IKeyboardListener keyboardListener, IGameLoop gameLoop, ModLoaderWrapper modLoaderWrapper,
        ILogger<Client> logger,
        WorldRenderer worldRenderer, World world)
    {
        _keyboardListener = keyboardListener;
        _gameLoop = gameLoop;

        _modloaderWrapper = modLoaderWrapper;

        _logger = logger;


        World = world;
        _worldRenderer = worldRenderer;

        _worldRenderer.AssociateWorld(world);


        world.SetVoxel(new Position<int>(0, 0, 0), new Voxel(Color.Red));
    }

    public void Run()
    {
        _keyboardListener.Subscribe(Key.Escape, _gameLoop.Stop);

        _gameLoop.RegisterUpdateAction(_modloaderWrapper);
        _gameLoop.RegisterRenderAction(_modloaderWrapper);
        _gameLoop.RegisterRenderAction(_worldRenderer);


        _modloaderWrapper.InitializeShaders();
        _worldRenderer.InitializeShaders();

        _logger.LogInformation("Starting game loop...");


        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        _gameLoop.Start();
    }
}