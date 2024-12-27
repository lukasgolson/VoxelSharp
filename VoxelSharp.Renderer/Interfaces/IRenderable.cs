using VoxelSharp.Core.Renderer;

namespace VoxelSharp.Renderer.Interfaces;

public interface IRenderable
{
    public void Render(Shader shaderProgram);
}