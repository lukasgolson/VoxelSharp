using OpenTK.Mathematics;

namespace VoxelSharp.Renderer.Interfaces;

public interface ICamera
{
    public Matrix4 GetViewMatrix();
    public Matrix4 GetProjectionMatrix();
}