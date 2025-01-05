using System.Numerics;

namespace VoxelSharp.Abstractions.Renderer;

public interface ICameraMatricesProvider
{
    public Matrix4x4 GetViewMatrix();
    public Matrix4x4 GetProjectionMatrix();
}