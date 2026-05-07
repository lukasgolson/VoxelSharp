using System.Windows;
using VoxelSharp.Abstractions.Client;
using SimpleInjector;

namespace VoxelSegmentation;

public class WpfClient(Container container) : IClient
{
    public void Run()
    {
        var app = new Application();
        // We pass the container to the window so it can resolve the WorldRenderer
        var mainWindow = new MainWindow(container);
        app.Run(mainWindow);
    }
}