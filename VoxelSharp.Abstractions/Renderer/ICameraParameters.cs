namespace VoxelSharp.Abstractions.Renderer;

public interface ICameraParameters
{
    float PositionX { get; }
    float PositionY { get; }
    float PositionZ { get; }

    float RotationPitch { get; }
    float RotationYaw { get; }
    float RotationRoll { get; }

    float FieldOfView { get; }
    float NearClip { get; }
    float FarClip { get; }
    float AspectRatio { get; }
    
    CameraType Camera { get; }


    public enum CameraType
    {
        Perspective,
        Orthographic
    }
}