using System.Numerics;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Core.Helpers;
using Vector3 = System.Numerics.Vector3;

namespace VoxelSharp.Core.Camera;

public abstract class BaseCamera : IUpdatable, ICameraMatricesProvider, IAspectRatioEventSubscriber, ICameraParameters
{
    private const float MouseSensitivity = 0.5f;

    // Camera position and directions
    public Vector3 Position { get; private set; }

    public Vector3 Rotation => _rotation;

    public float FieldOfView => 45;
    public float NearClip => 0.01f;
    public float FarClip => 2000f;
    public float AspectRatio { get; }
    public ICameraParameters.CameraType Camera => ICameraParameters.CameraType.Perspective;


    // Matrices
    private Matrix4x4 _projectionMatrix;
    private Matrix4x4 _viewMatrix = Matrix4x4.Identity;


    private Vector3 _rotation;

    
    protected Vector3 Forward = -Vector3.UnitZ;
    protected Vector3 Right = Vector3.UnitX;
    protected Vector3 Up = Vector3.UnitY;


    /// <summary>
    ///     Initializes a new instance of the Camera class.
    /// </summary>
    /// <param name="gameLoop"> The gameloop to register camera updates to.</param>
    /// <param name="aspectRatio">The aspect ratio of the camera's view.</param>
    protected BaseCamera(IGameLoop gameLoop, float aspectRatio = 16f / 9f)
    {
        AspectRatio = aspectRatio;
        gameLoop.RegisterUpdateAction(this);
        SetProjectionMatrix(FieldOfView, AspectRatio);
    }

    /// <summary>
    ///     Updates the aspect ratio for the camera's projection matrix.
    /// </summary>
    public void UpdateAspectRatio(float aspectRatio)
    {
        SetProjectionMatrix(FieldOfView, aspectRatio);
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
        var verticalFovRadians = verticalFov.ToRadians();


        _projectionMatrix =
            Matrix4x4.CreatePerspectiveFieldOfView(verticalFovRadians, aspectRatio, NearClip, FarClip);
    }

    /// <summary>
    ///     Updates the view matrix based on the camera's position and direction.
    /// </summary>
    private void UpdateViewMatrix()
    {
        _viewMatrix = Matrix4x4.CreateLookAt(Position, Position + Forward, Up);
    }

    /// <summary>
    ///     Updates the camera's relative directional vectors based on its rotation.
    /// </summary>
    private void UpdateRelativeVectors()
    {
        var pitchRadians = Rotation.X.ToRadians();
        var yawRadians = Rotation.Y.ToRadians();


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
        Position += deltaPosition;
    }

    /// <summary>
    ///     Updates the camera's rotation based on the given yaw and pitch deltas.
    /// </summary>
    protected void UpdateRotation(float deltaYaw, float deltaPitch)
    {
        var pitch = _rotation.X;
        var yaw = _rotation.Y;

        pitch = Math.Clamp(pitch + deltaPitch * MouseSensitivity, -89f, 89f);


        yaw = (yaw + deltaYaw * MouseSensitivity) % 360f; // Normalize yaw to the range [0, 360)
        if (yaw < 0f) yaw += 360f; // Ensure positive yaw

        _rotation.X = pitch;
        _rotation.Y = yaw;
    }
}