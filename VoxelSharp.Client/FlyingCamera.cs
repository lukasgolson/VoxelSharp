using System.Numerics;
using System.Windows.Input;
using DeftSharp.Windows.Input.Keyboard;
using VoxelSharp.Abstractions.Input;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Window;
using VoxelSharp.Core.Camera;
using VoxelSharp.Core.Helpers;

namespace VoxelSharp.Client;

public class FlyingCamera : Camera
{
    private const float Speed = 10f;
    private const float DampingFactor = 5f; // Controls how quickly movement slows down


    private readonly IMouseRelative _mouseInput;
    private Vector3 _input = Vector3.Zero;

    private Vector3 _velocity = Vector3.Zero;

    public FlyingCamera(IGameLoop gameLoop, IMouseRelative mouseInput, IKeyboardListener keyboardListener,
        IWindow window)
        : base(gameLoop)
    {
        _mouseInput = mouseInput;

        window.OnWindowResize += (_, aspectRatio) => UpdateAspectRatio((float)aspectRatio);


        keyboardListener.Subscribe(Key.W, forward_start);
        keyboardListener.Subscribe(Key.W, forward_stop, null, KeyboardEvent.KeyUp);
        keyboardListener.Subscribe(Key.S, backward_start);
        keyboardListener.Subscribe(Key.S, backward_stop, null, KeyboardEvent.KeyUp);

        keyboardListener.Subscribe(Key.A, left_start);
        keyboardListener.Subscribe(Key.A, left_stop, null, KeyboardEvent.KeyUp);

        keyboardListener.Subscribe(Key.D, right_start);
        keyboardListener.Subscribe(Key.D, right_stop, null, KeyboardEvent.KeyUp);

        keyboardListener.Subscribe(Key.Space, up_start);
        keyboardListener.Subscribe(Key.Space, up_stop, null, KeyboardEvent.KeyUp);

        keyboardListener.Subscribe(Key.LeftShift, down_start);
        keyboardListener.Subscribe(Key.LeftShift, down_stop, null, KeyboardEvent.KeyUp);
    }

    private void forward_start()
    {
        _input.Z = Math.Abs(_input.Z - -1) < 0.01 ? 0 : 1;
    }

    private void forward_stop()
    {
        _input.Z = 0;
    }

    private void backward_start()
    {
        _input.Z = Math.Abs(_input.Z - 1) < 0.01 ? 0 : -1;
    }

    private void backward_stop()
    {
        _input.Z = 0;
    }

    private void right_start()
    {
        _input.X = Math.Abs(_input.X - 1) < 0.01 ? 0 : 1;
    }

    private void right_stop()
    {
        _input.X = 0;
    }

    private void left_start()
    {
        _input.X = Math.Abs(_input.X - -1) < 0.01 ? 0 : -1;
    }

    private void left_stop()
    {
        _input.X = 0;
    }

    private void up_start()
    {
        _input.Y = Math.Abs(_input.Y - 1) < 0.01 ? 0 : 1;
    }

    private void up_stop()
    {
        _input.Y = 0;
    }

    private void down_start()
    {
        _input.Y = Math.Abs(_input.Y - -1) < 0.01 ? 0 : -1;
    }

    private void down_stop()
    {
        _input.Y = 0;
    }


    public override void Update(double deltaTime)
    {
        _velocity += _input * Speed * (float)deltaTime;

        // clamp velocity to 0 to 1
        _velocity = Vector3.Clamp(_velocity, -Vector3.One, Vector3.One);

        // Apply damping to velocity
        _velocity *= 1 - DampingFactor * (float)deltaTime;

        if (_velocity.Magnitude() < 0.01f) _velocity = Vector3.Zero;

        var movement = _velocity * Speed * (float)deltaTime;

        // Calculate world movement direction
        var worldMovement =
            movement.Z * Forward + // Forward/backward
            movement.X * Right + // Left/right
            movement.Y * Up; // Up/down


        UpdatePosition(worldMovement);
        UpdateRotation((float)_mouseInput.RelativeX, (float)-_mouseInput.RelativeY);

        base.Update(deltaTime);
    }
}