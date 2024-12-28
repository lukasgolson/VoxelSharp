namespace VoxelSharp.Renderer.Interfaces;

public interface IRenderer
{
    public void InitializeShaders();
    public void Render(ICamera camera);
}