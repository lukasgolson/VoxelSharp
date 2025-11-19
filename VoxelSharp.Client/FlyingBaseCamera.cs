using System.Numerics;
using System.Windows.Input;
using DeftSharp.Windows.Input.Keyboard;
using VoxelSharp.Abstractions.Input;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Window;
using VoxelSharp.Core.Camera;
using VoxelSharp.Core.Helpers;

namespace VoxelSharp.Client;

public class FlyingBaseCamera : BaseCamera
{
    private const float Speed = 15f;
    private const float DampingFactor = 5f; // Controls how quickly movement slows down


    private readonly IMouseRelative _mouseInput;

    private readonly IWindow _window; // Store the window interface
    private bool _mouseLocked = true; // Track our lock state

    private Vector3 _input = Vector3.Zero;
    private Vector3 _velocity = Vector3.Zero;

    public FlyingBaseCamera(IGameLoop gameLoop, IMouseRelative mouseInput, IKeyboardListener keyboardListener,
        IWindow window)
        : base(gameLoop)
    {
        UpdatePosition(new Vector3(0, 45, 0));
        _mouseInput = mouseInput;
        _window = window; // Store the window


        window.OnWindowResize += (_, aspectRatio) => UpdateAspectRatio((float)aspectRatio);

        // --- Handle window focus changes ---
        window.OnFocus += () =>
        {
            if (_mouseLocked) // Only re-lock if we are in the "locked" state
            {
                _mouseInput.StartTracking(new IntPtr(_window.WindowHandle));
            }
        };

        window.OnUnfocus += () =>
        {
            _mouseInput.StopTracking(); // Always unlock on unfocus
            _input = Vector3.Zero; // <-- ADD THIS
        };

        // --- Handle Ctrl key for manual lock/unlock ---
        keyboardListener.Subscribe(Key.LeftCtrl, UnlockMouse, null, KeyboardEvent.KeyDown);
        keyboardListener.Subscribe(Key.LeftCtrl, LockMouse, null, KeyboardEvent.KeyUp);

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

        if (_window.IsFocused && _mouseLocked)
        {
            _mouseInput.StartTracking(new IntPtr(_window.WindowHandle));
        }
    }

    private void UnlockMouse()
    {
        _mouseLocked = false;
        _mouseInput.StopTracking();
        _input = Vector3.Zero; // Instantly stop movement
    }

    private void LockMouse()
    {
        _mouseLocked = true; // Set the desired state

        // Only re-lock if the main window is currently focused.
        // If an ImGui viewport has focus, this check will fail,
        // and the OnFocus handler will lock it later when
        // the user clicks back on the main 3D scene.
        if (_window.IsFocused)
        {
            _mouseInput.StartTracking(new IntPtr(_window.WindowHandle));
        }
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
        // Only process movement/rotation if mouse is locked
        if (_mouseLocked)
        {
            _velocity += _input * Speed * (float)deltaTime;

            _velocity = Vector3.Clamp(_velocity, -Vector3.One, Vector3.One);

            var movement = _velocity * Speed * (float)deltaTime;

            // Calculate world movement direction
            var worldMovement =
                movement.Z * Forward + // Forward/backward
                movement.X * Right + // Left/right
                movement.Y * Up; // Up/down

            UpdatePosition(worldMovement);
            UpdateRotation((float)_mouseInput.RelativeX, (float)-_mouseInput.RelativeY);
        }

        // Apply damping even if unlocked, so you glide to a stop
        _velocity *= 1 - DampingFactor * (float)deltaTime;
        if (_velocity.Magnitude() < 0.01f) _velocity = Vector3.Zero;

        // This must be called every frame to update matrices
        base.Update(deltaTime);
    }
}