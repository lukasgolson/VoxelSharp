using OpenTK.Mathematics;
using VoxelSharp.Extensions;
using VoxelSharp.Renderer.interfaces;
using VoxelSharp.Structs;

namespace VoxelSharp.Renderer.Camera;

public abstract class Camera : IUpdatable
{
    // Rotation
    protected float Yaw = 0; // Horizontal rotation
    protected float Pitch = 0; // Vertical rotation

    // Camera position and directions
    public Position<float> Position = new(0.0f, 0.0f, 0.0f);
    protected Position<float> Up = new(0.0f, 1.0f, 0.0f);
    protected Position<float> Right = new(1.0f, 0.0f, 0.0f);
    protected Position<float> Forward = new(0.0f, 0.0f, -1.0f);

    // Matrices
    private Matrix4 _viewMatrix;
    private Matrix4 _projectionMatrix;

    public Camera(float aspectRatio)
    {
        var verticalFov = MathHelper.DegreesToRadians(45.0f);
        SetProjectionMatrix(verticalFov, aspectRatio);
        _viewMatrix = Matrix4.Identity;
    }

    /// <summary>
    /// Sets the projection matrix for the camera.
    /// </summary>
    private void SetProjectionMatrix(float verticalFov, float aspectRatio)
    {
        const float nearPlane = 0.1f;
        const float farPlane = 2000.0f;

        _projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(verticalFov, aspectRatio, nearPlane, farPlane);
    }

    /// <summary>
    /// Updates the view matrix based on the camera's position and direction.
    /// </summary>
    private void UpdateViewMatrix()
    {
        _viewMatrix = Matrix4.LookAt(Position.ToVector3(), (Position + Forward).ToVector3(), Up.ToVector3());
    }

    /// <summary>
    /// Updates the camera's relative directional vectors based on its rotation.
    /// </summary>
    private void UpdateRelativeVectors()
    {
        var x = MathF.Cos(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch));
        var y = MathF.Sin(MathHelper.DegreesToRadians(Pitch));
        var z = MathF.Sin(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch));

        Forward = new Position<float>(x, y, z).Normalize();
        Right = Position<float>.Cross(Forward, new Position<float>(0, 1, 0)).Normalize();
        Up = Position<float>.Cross(Right, Forward).Normalize();
    }

    /// <summary>
    /// Updates the camera's position based on the given delta position.
    /// </summary>
    public void UpdatePosition(Position<float> deltaPosition)
    {
        Position += Right * deltaPosition.X;
        Position += Up * deltaPosition.Y;
        Position += Forward * deltaPosition.Z;
    }

    /// <summary>
    /// Updates the camera's rotation based on the given yaw and pitch deltas.
    /// </summary>
    public void UpdateRotation(float deltaYaw, float deltaPitch)
    {
        Yaw = (Yaw + deltaYaw) % 360.0f; // Normalize yaw to the range [0, 360)
        if (Yaw < 0) Yaw += 360.0f; // Ensure positive yaw

        Pitch = MathHelper.Clamp(Pitch + deltaPitch, -89.0f, 89.0f); // Clamp pitch to avoid gimbal lock
    }

    /// <summary>
    /// Updates the camera's view matrix and relative vectors.
    /// </summary>
    public virtual void Update(float deltaTime)
    {
        UpdateRelativeVectors();
        UpdateViewMatrix();
    }

    /// <summary>
    /// Gets the camera's view matrix.
    /// </summary>
    public Matrix4 GetViewMatrix()
    {
        return _viewMatrix;
    }

    /// <summary>
    /// Gets the camera's projection matrix.
    /// </summary>
    public Matrix4 GetProjectionMatrix()
    {
        return _projectionMatrix;
    }

    /// <summary>
    /// Updates the aspect ratio for the camera's projection matrix.
    /// </summary>
    public void UpdateAspectRatio(float aspectRatio)
    {
        var verticalFov = MathHelper.DegreesToRadians(45.0f);
        SetProjectionMatrix(verticalFov, aspectRatio);
    }
}
