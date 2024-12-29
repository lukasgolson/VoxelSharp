namespace VoxelSharp.Abstractions.Renderer;

public interface IWindow
{
    public (int Width, int Height) ScreenSize { get; }
}