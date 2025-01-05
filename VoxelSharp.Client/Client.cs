using System.Windows.Input;
using DeftSharp.Windows.Input.Keyboard;
using VoxelSharp.Abstractions.Client;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Client.Wrappers;
using VoxelSharp.Core.Structs;
using VoxelSharp.Core.World;
using VoxelSharp.Renderer.Mesh.World;

namespace VoxelSharp.Client;

public class Client : IClient
{
    private readonly IGameLoop _gameLoop;
    private readonly IKeyboardListener _keyboardListener;
    private readonly ModLoaderWrapper _modloaderWrapper;
    private readonly WorldRenderer _worldRenderer;


    public Client(IKeyboardListener keyboardListener, IGameLoop gameLoop,
        ICameraMatricesProvider cameraMatricesProvider, ModLoaderWrapper modLoaderWrapper)
    {
        _keyboardListener = keyboardListener;
        _gameLoop = gameLoop;

        _modloaderWrapper = modLoaderWrapper;


        World = new World(2, 16);
        _worldRenderer = new WorldRenderer(World, cameraMatricesProvider);


        World.SetVoxel(new Position<int>(0, 0, 0), new Voxel(Color.Red));
    }

    public World World { get; }


    public void Run()
    {
        _keyboardListener.Subscribe(Key.Escape, _gameLoop.Stop);

        _gameLoop.RegisterUpdateAction(_modloaderWrapper);
        _gameLoop.RegisterRenderAction(_modloaderWrapper);
        _gameLoop.RegisterRenderAction(_worldRenderer);


        _modloaderWrapper.InitializeShaders();
        _worldRenderer.InitializeShaders();


        _gameLoop.Start();
    }
}