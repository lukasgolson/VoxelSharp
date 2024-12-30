namespace VoxelSharp.Abstractions.Renderer;

public interface IRenderer
{
    public void InitializeShaders();
    public void Render(ICameraMatrices cameraMatrices);
}