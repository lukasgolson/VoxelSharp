namespace VoxelSharp.Renderer.Camera;

public class FlyingCamera(float aspectRatio) : Camera(aspectRatio)
{
    private const float Speed = 0.01f;


    public void MoveForward()
    {
        var deltaPosition = Forward * Speed;
        UpdatePosition(deltaPosition);
    }

    public void MoveBackward()
    {
        var deltaPosition = -Forward * Speed;
        UpdatePosition(deltaPosition);
    }

    public void MoveLeft()
    {
        var deltaPosition = -Right * Speed;
        UpdatePosition(deltaPosition);
    }

    public void MoveRight()
    {
        var deltaPosition = Right * Speed;
        UpdatePosition(deltaPosition);
    }

    public void MoveUp()
    {
        var deltaPosition = Up * Speed;
        UpdatePosition(deltaPosition);
    }

    public void MoveDown()
    {
        var deltaPosition = -Up * Speed;
        UpdatePosition(deltaPosition);
    }
}