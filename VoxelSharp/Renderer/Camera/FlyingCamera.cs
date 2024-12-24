using VoxelSharp.Renderer.interfaces;

namespace VoxelSharp.Renderer;

public class FlyingCamera : Camera
{
    private float _speed = 0.01f;

    public FlyingCamera(float aspectRatio) : base(aspectRatio)
    {
    }


    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        // Console.WriteLine(Position);
        //
        // Console.WriteLine(_pitch);
        // Console.WriteLine(_yaw);
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