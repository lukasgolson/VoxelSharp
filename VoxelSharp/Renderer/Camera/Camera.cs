using OpenTK.Mathematics;
using VoxelSharp.Extensions;
using VoxelSharp.Renderer.interfaces;
using VoxelSharp.Structs;

namespace VoxelSharp.Renderer;

public abstract class Camera : IUpdatable
{
    protected float _yaw = 0;
    protected float _pitch = 0;

    protected Position<float> Position = new(0.0f, 0.0f, 0.0f);

    protected static Position<float> Up = new(0.0f, 1.0f, 0.0f);
    protected static Position<float> Right = new(1.0f, 0.0f, 0.0f);
    protected static Position<float> Forward = new(0.0f, 0.0f, -1.0f);

    private readonly Matrix4 _viewMatrix;
    private Matrix4 _projectionMatrix;


    public Camera(float aspectRatio)
    {
        
        
   

        var verticalFov = MathHelper.DegreesToRadians(45.0f);



        SetProjectionMatrix(verticalFov, aspectRatio);
        _viewMatrix = Matrix4.Identity;
    }

    private void SetProjectionMatrix(float verticalFov, float aspectRatio)
    {
        const float nearPlane = 0.1f;
        const float farPlane = 2000.0f;

        _projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(verticalFov, aspectRatio, nearPlane, farPlane);
    }

    private void UpdateViewMatrix()
    {
        Matrix4.LookAt(Position.ToVector3(), (Position + Forward).ToVector3(), Up.ToVector3());
    }

    private void UpdateRelativeVectors()
    {
        var x = MathF.Cos(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));
        var y = MathF.Sin(MathHelper.DegreesToRadians(_pitch));
        var z = MathF.Sin(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));

        Forward = new Position<float>(x, y, z).Normalize();
        Right = Position<float>.Cross(Forward, new Position<float>(0, 1, 0)).Normalize();
        Up = Position<float>.Cross(Right, Forward).Normalize();
    }

    public void UpdatePosition(Position<float> deltaPosition)
    {
        Position += Right * deltaPosition.X;
        Position += Up * deltaPosition.Y;
        Position += Forward * deltaPosition.Z;
    }

    public void UpdateRotation(float deltaYaw, float deltaPitch)
    {
        _yaw += deltaYaw;

        switch (_yaw)
        {
            case > 180.0f:
                _yaw -= 360.0f;
                break;
            case < -180.0f:
                _yaw += 360.0f;
                break;
        }

        _pitch += deltaPitch;
        _pitch = MathHelper.Clamp(_pitch, -89.0f, 89.0f);
    }


    /// <summary>
    /// Updates the camera's view matrix.
    /// </summary>
    public virtual void Update(float deltaTime)
    {
        UpdateRelativeVectors();
        UpdateViewMatrix();
    }

    /// <summary>
    /// Returns the view matrix of the camera.
    /// </summary>
    /// <returns></returns>
    public Matrix4 GetViewMatrix()
    {
        return _viewMatrix;
    }

    /// <summary>
    /// Returns the projection matrix of the camera.
    /// </summary>
    /// <returns></returns>
    public Matrix4 GetProjectionMatrix()
    {
        return _projectionMatrix;
    }

    public void UpdateAspectRatio(float aspectRatio)
    {
        var verticalFov = MathHelper.DegreesToRadians(45.0f);

        SetProjectionMatrix(verticalFov, aspectRatio);
        
    }


}