namespace VoxelSharp.Abstractions.Renderer;

public interface ILightSource
{
    System.Numerics.Vector3 GetCurrentLightDirection();
    System.Numerics.Vector3 GetCurrentLightColor();
}