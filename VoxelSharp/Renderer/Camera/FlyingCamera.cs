using VoxelSharp.Renderer.interfaces;

namespace VoxelSharp.Renderer;

public class FlyingCamera : Camera.Camera
{
    private float _speed = 0.01f;

    public FlyingCamera(float aspectRatio) : base(aspectRatio)
    {
    }


    public void MoveForward()
    {
        var deltaPosition = Forward * _speed;
        UpdatePosition(deltaPosition);
    }

    public void MoveBackward()
    {
        var deltaPosition = -Forward * _speed;
        UpdatePosition(deltaPosition);
    }

    public void MoveLeft()
    {
        var deltaPosition = -Right * _speed;
        UpdatePosition(deltaPosition);
    }

    public void MoveRight()
    {
        var deltaPosition = Right * _speed;
        UpdatePosition(deltaPosition);
    }

    public void MoveUp()
    {
        var deltaPosition = Up * _speed;
        UpdatePosition(deltaPosition);
    }

    public void MoveDown()
    {
        var deltaPosition = -Up * _speed;
        UpdatePosition(deltaPosition);
    }

  
}