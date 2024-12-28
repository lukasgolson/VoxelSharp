using OpenTK.Mathematics;
using VoxelSharp.Core.Interfaces;
using VoxelSharp.Renderer.Interfaces;

namespace VoxelSharp.Renderer.Camera
{
    public abstract class Camera : ICamera, IUpdatable
    {
        // Rotation
        protected float Yaw = 0f;  // Horizontal rotation
        protected float Pitch = 0f;  // Vertical rotation

        // Camera position and directions
        public Vector3 Position = Vector3.Zero;
        protected Vector3 Up = Vector3.UnitY;
        protected Vector3 Right = Vector3.UnitX;
        protected Vector3 Forward = -Vector3.UnitZ;

        // Matrices
        private Matrix4 _viewMatrix = Matrix4.Identity;
        private Matrix4 _projectionMatrix;
        private float _mouseSensitivity = 0.5f;

        /// <summary>
        /// Initializes a new instance of the Camera class.
        /// </summary>
        /// <param name="aspectRatio">The aspect ratio of the camera's view.</param>
        public Camera(float aspectRatio)
        {
            SetProjectionMatrix(45, aspectRatio);
        }

        /// <summary>
        /// Sets the projection matrix for the camera.
        /// </summary>
        private void SetProjectionMatrix(float verticalFov, float aspectRatio)
        {
            const float nearPlane = 0.01f;
            const float farPlane = 2000f;

            var verticalFovRadians = MathHelper.DegreesToRadians(verticalFov);
            
            _projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(verticalFovRadians, aspectRatio, nearPlane, farPlane);
        }

        /// <summary>
        /// Updates the view matrix based on the camera's position and direction.
        /// </summary>
        private void UpdateViewMatrix()
        {
            _viewMatrix = Matrix4.LookAt(Position, Position + Forward, Up);
        }

        /// <summary>
        /// Updates the camera's relative directional vectors based on its rotation.
        /// </summary>
        private void UpdateRelativeVectors()
        {
            var pitchRadians = MathHelper.DegreesToRadians(Pitch);
            var yawRadians = MathHelper.DegreesToRadians(Yaw);

            Forward = new Vector3(
                MathF.Cos(yawRadians) * MathF.Cos(pitchRadians),
                MathF.Sin(pitchRadians),
                MathF.Sin(yawRadians) * MathF.Cos(pitchRadians)
            ).Normalized();

            Right = Vector3.Cross(Forward, Vector3.UnitY).Normalized();
            Up = Vector3.Cross(Right, Forward).Normalized();

        }

        /// <summary>
        /// Updates the camera's position based on the given delta position.
        /// </summary>
        public void UpdatePosition(Vector3 deltaPosition)
        {
            Position += deltaPosition;
        }

        /// <summary>
        /// Updates the camera's rotation based on the given yaw and pitch deltas.
        /// </summary>
        public void UpdateRotation(float deltaYaw, float deltaPitch)
        {
            Yaw = (Yaw + deltaYaw * _mouseSensitivity) % 360f; // Normalize yaw to the range [0, 360)
            if (Yaw < 0f) Yaw += 360f;    // Ensure positive yaw

            Pitch = MathHelper.Clamp(Pitch + (deltaPitch * _mouseSensitivity), -89f, 89f); // Clamp pitch to avoid gimbal lock
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
            SetProjectionMatrix(45, aspectRatio);
        }
    }
}
