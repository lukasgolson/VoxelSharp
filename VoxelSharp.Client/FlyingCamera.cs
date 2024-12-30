using System.Numerics;
using System.Windows.Input;
using DeftSharp.Windows.Input.Keyboard;
using DeftSharp.Windows.Input.Mouse;
using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Core.Camera;

namespace VoxelSharp.Client;

public class FlyingCamera : Camera, IUpdatable
{
    private const float Speed = 10f;
    private const float MouseSpeed = 0.5f;

    private Vector3 _deltaPosition = Vector3.Zero;
    
    private Vector2 _currentMousePosition = Vector2.Zero;
    private Vector2 _lastMousePosition = Vector2.Zero;
    
    
    private KeyboardListener _keyboardListener = new KeyboardListener();
    private MouseListener _mouseListener = new MouseListener();

    public FlyingCamera(float aspectRatio) : base(aspectRatio)
    {


        _mouseListener.Subscribe(MouseEvent.Move, () =>
        {
            _lastMousePosition = _currentMousePosition;
            _currentMousePosition = new Vector2(_mouseListener.Position.X, _mouseListener.Position.Y);
        });


        _keyboardListener.Subscribe(Key.W, MoveForward);
        _keyboardListener.Subscribe(Key.S, MoveBackward);
        _keyboardListener.Subscribe(Key.A, MoveLeft);
        _keyboardListener.Subscribe(Key.D, MoveRight);
        _keyboardListener.Subscribe(Key.Space, MoveUp);
        _keyboardListener.Subscribe(Key.LeftShift, MoveDown);
    }


    public override void Update(double deltaTime)
    {
        base.Update(deltaTime);


        var adjustedSpeed = Speed * (float)deltaTime;

        var adjustedDeltaPosition = _deltaPosition * adjustedSpeed;

        // we should then orient the deltaPosition to the camera's orientation using the forward, right, and up vectors
        var worldDeltaPosition =
            adjustedDeltaPosition.X * Forward + // Forward/backward
            adjustedDeltaPosition.Z * Right + // Left/right
            adjustedDeltaPosition.Y * Up; // Up/down

        UpdatePosition(worldDeltaPosition);

        
        var deltaMouse = _currentMousePosition - _lastMousePosition;
        
        UpdateRotation(deltaMouse.X * (float) deltaTime * MouseSpeed, -deltaMouse.Y * (float)deltaTime * MouseSpeed);


    }


    public void MoveForward()
    {
        _deltaPosition.X = 1;
    }

    public void MoveBackward()
    {
        _deltaPosition.X = -1;
    }

    public void MoveLeft()
    {
        _deltaPosition.Z = -1;
    }

    public void MoveRight()
    {
        _deltaPosition.Z = 1;
    }

    public void MoveUp()
    {
        _deltaPosition.Y = 1;
    }

    public void MoveDown()
    {
        _deltaPosition.Y = -1;
    }
}