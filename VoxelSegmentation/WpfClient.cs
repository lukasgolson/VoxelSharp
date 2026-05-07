using System.Windows;
using VoxelSharp.Abstractions.Client;
using SimpleInjector;
using VoxelSegmentation.WPF;

namespace VoxelSegmentation;

public class WpfClient(Container container) : IClient
{
    public void Run()
    {
        var app = new App(); 

        var mainWindow = new MainWindow(container);
        
        app.Run(mainWindow);
    }
}