namespace VoxelSharp.Abstractions.Renderer;

public interface IRendererProcessing
{
    public void PreRender();

    public void PostRender();
}