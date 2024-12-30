using System.Numerics;
using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Core.Helpers;
using Vector3 = System.Numerics.Vector3;

namespace VoxelSharp.Core.Camera;

public abstract class Camera : IUpdatable, ICameraMatrices
{
    private const float MouseSensitivity = 0.5f;
    private float _pitch; // Vertical rotation

    // Camera position and directions
    private Vector3 _position = Vector3.Zero;
    private Matrix4x4 _projectionMatrix;

    // Matrices
    private Matrix4x4 _viewMatrix = Matrix4x4.Identity;

    // Rotation
    private float _yaw; // Horizontal rotation
    protected Vector3 Forward = -Vector3.UnitZ;
    protected Vector3 Right = Vector3.UnitX;
    protected Vector3 Up = Vector3.UnitY;

    /// <summary>
    ///     Initializes a new instance of the Camera class.
    /// </summary>
    /// <param name="aspectRatio">The aspect ratio of the camera's view.</param>
    protected Camera(float aspectRatio)
    {
        SetProjectionMatrix(45, aspectRatio);
    }

    /// <summary>
    ///     Gets the camera's view matrix.
    /// </summary>
    public Matrix4x4 GetViewMatrix()
    {
        return _viewMatrix;
    }

    /// <summary>
    ///     Gets the camera's projection matrix.
    /// </summary>
    public Matrix4x4 GetProjectionMatrix()
    {
        return _projectionMatrix;
    }

    /// <summary>
    ///     Updates the camera's view matrix and relative vectors.
    /// </summary>
    public virtual void Update(double deltaTime)
    {
        UpdateRelativeVectors();
        UpdateViewMatrix();
    }

    /// <summary>
    ///     Sets the projection matrix for the camera.
    /// </summary>
    private void SetProjectionMatrix(float verticalFov, float aspectRatio)
    {
        const float nearPlane = 0.01f;
        const float farPlane = 2000f;



        var verticalFovRadians = verticalFov.ToRadians();

        
        _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(verticalFovRadians, aspectRatio, nearPlane, farPlane);
        
    }

    /// <summary>
    ///     Updates the view matrix based on the camera's position and direction.
    /// </summary>
    private void UpdateViewMatrix()
    {
        _viewMatrix = Matrix4x4.CreateLookAt(_position, _position + Forward, Up);
    }

    /// <summary>
    ///     Updates the camera's relative directional vectors based on its rotation.
    /// </summary>
    private void UpdateRelativeVectors()
    {
        var pitchRadians = _pitch.ToRadians();
        var yawRadians = _yaw.ToRadians();

        
        Forward = new Vector3(
            MathF.Cos(yawRadians) * MathF.Cos(pitchRadians),
            MathF.Sin(pitchRadians),
            MathF.Sin(yawRadians) * MathF.Cos(pitchRadians)
        ).Normalize();

        Right = Vector3.Cross(Forward, Vector3.UnitY).Normalize();
        Up = Vector3.Cross(Right, Forward).Normalize();
    }

    /// <summary>
    ///     Updates the camera's position based on the given delta position.
    /// </summary>
    protected void UpdatePosition(Vector3 deltaPosition)
    {
        _position += deltaPosition;
    }

    /// <summary>
    ///     Updates the camera's rotation based on the given yaw and pitch deltas.
    /// </summary>
    public void UpdateRotation(float deltaYaw, float deltaPitch)
    {
        _yaw = (_yaw + deltaYaw * MouseSensitivity) % 360f; // Normalize yaw to the range [0, 360)
        if (_yaw < 0f) _yaw += 360f; // Ensure positive yaw


        _pitch = Math.Clamp(_pitch + deltaPitch * MouseSensitivity, -89f, 89f);
        

    }

    /// <summary>
    ///     Updates the aspect ratio for the camera's projection matrix.
    /// </summary>
    public void UpdateAspectRatio(float aspectRatio)
    {
        SetProjectionMatrix(45, aspectRatio);
    }
}