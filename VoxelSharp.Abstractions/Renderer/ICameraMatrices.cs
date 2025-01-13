using System.Numerics;

namespace VoxelSharp.Abstractions.Renderer;

public interface ICameraMatrices
{
    public Matrix4x4 GetViewMatrix();
    public Matrix4x4 GetProjectionMatrix();
}