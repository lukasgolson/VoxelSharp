using System.Numerics;
using System.Windows;
using System.Windows.Input;
using DeftSharp.Windows.Input.Keyboard;
using DeftSharp.Windows.Input.Mouse;
using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Core.Camera;

namespace VoxelSharp.Client
{
    public class FlyingCamera : Camera, IUpdatable
    {
        private const float Speed = 10f;
        private const float MouseSensitivity = 0.1f;
        private const float DampingFactor = 5f; // Controls how quickly movement slows down

        private Vector3 _velocity = Vector3.Zero;
        private Vector2 _lastMousePosition;

        private readonly KeyboardListener _keyboardListener = new();
        private readonly MouseListener _mouseListener = new();
        private readonly MouseManipulator _mouseManipulator = new();

        public FlyingCamera(float aspectRatio) : base(aspectRatio)
        {
            // Initialize mouse position to the center of the screen
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            _lastMousePosition = new Vector2((float)screenWidth / 2, (float)screenHeight / 2);

            // Move mouse cursor to the center of the screen
            
     
            
            _mouseManipulator.SetPosition((int)_lastMousePosition.X, (int)_lastMousePosition.Y);

            _mouseListener.Subscribe(MouseEvent.Move, OnMouseMove);

            _keyboardListener.Subscribe(Key.W, () => _velocity.Z = 1);
            _keyboardListener.Subscribe(Key.S, () => _velocity.Z = -1);
            _keyboardListener.Subscribe(Key.A, () => _velocity.X = -1);
            _keyboardListener.Subscribe(Key.D, () => _velocity.X = 1);
            _keyboardListener.Subscribe(Key.Space, () => _velocity.Y = 1);
            _keyboardListener.Subscribe(Key.LeftShift, () => _velocity.Y = -1);

            // Unsubscribe on key release to stop movement
            _keyboardListener.Subscribe(Key.W, () => _velocity.Z = 0, null, KeyboardEvent.KeyUp);
            _keyboardListener.Subscribe(Key.S, () => _velocity.Z = 0, null, KeyboardEvent.KeyUp);
            _keyboardListener.Subscribe(Key.A, () => _velocity.X = 0, null, KeyboardEvent.KeyUp);
            _keyboardListener.Subscribe(Key.D, () => _velocity.X = 0, null, KeyboardEvent.KeyUp);
            _keyboardListener.Subscribe(Key.Space, () => _velocity.Y = 0, null, KeyboardEvent.KeyUp);
            _keyboardListener.Subscribe(Key.LeftShift, () => _velocity.Y = 0, null, KeyboardEvent.KeyUp);
        }

        private void OnMouseMove()
        {
            var currentMousePosition = new Vector2(_mouseListener.Position.X, _mouseListener.Position.Y);
            var deltaMouse = currentMousePosition - _lastMousePosition;

            // Update camera rotation based on mouse movement
            UpdateRotation(deltaMouse.X * MouseSensitivity, -deltaMouse.Y * MouseSensitivity);

            // Reset mouse position to the center of the screen
            _mouseManipulator.SetPosition((int)_lastMousePosition.X, (int)_lastMousePosition.Y);
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            // Apply damping to velocity
            _velocity *= (1 - DampingFactor * (float)deltaTime);

            var movement = _velocity * Speed * (float)deltaTime;

            // Calculate world movement direction
            var worldMovement =
                movement.Z * Forward + // Forward/backward
                movement.X * Right +   // Left/right
                movement.Y * Up;       // Up/down

            UpdatePosition(worldMovement);
        }
    }
}
