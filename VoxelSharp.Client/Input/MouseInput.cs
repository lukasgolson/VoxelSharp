using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using VoxelSharp.Abstractions.Input;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Window;

namespace VoxelSharp.Client.Input;

/// <summary>
///     Cross-platform relative mouse input. Uses GLFW's grabbed cursor mode, which hides the cursor,
///     confines it to the window and reports unbounded motion.
/// </summary>
public class MouseInput : IUpdatable, IMouseRelative
{
    private readonly NativeWindow _window;

    private bool _isTracking;
    private double _accumulatedX;
    private double _accumulatedY;

    public MouseInput(IGameLoop gameLoop, IWindow window)
    {
        _window = (NativeWindow)window;
        _window.MouseMove += OnMouseMove;

        gameLoop.RegisterUpdateAction(this);
    }

    public double RelativeX { get; private set; }
    public double RelativeY { get; private set; }

    public void Update(double deltaTime)
    {
        // Publish the motion gathered since the last update
        (RelativeX, RelativeY) = (_accumulatedX, _accumulatedY);
        (_accumulatedX, _accumulatedY) = (0, 0);
    }

    public void StartTracking(IntPtr windowHandle)
    {
        if (_isTracking) return;

        _isTracking = true;
        _window.CursorState = CursorState.Grabbed;

        // Discard any motion from before the cursor was grabbed
        (_accumulatedX, _accumulatedY) = (0, 0);
    }

    public void StopTracking()
    {
        _isTracking = false;
        _window.CursorState = CursorState.Normal;

        (_accumulatedX, _accumulatedY) = (0, 0);
        (RelativeX, RelativeY) = (0, 0);
    }

    private void OnMouseMove(MouseMoveEventArgs e)
    {
        if (!_isTracking) return;

        _accumulatedX += e.DeltaX;
        _accumulatedY += e.DeltaY;
    }
}
