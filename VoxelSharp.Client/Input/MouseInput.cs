using VoxelSharp.Abstractions.Renderer;

namespace VoxelSharp.Client.Input;

using System.Runtime.InteropServices;

public partial class MouseInput : IUpdatable
{
    private Point _lastMousePosition;
    private bool _isTracking = false;

    public double X { get; private set; }
    public double Y { get; private set; }

    public void StartTracking()
    {
        if (_isTracking) return;

        _isTracking = true;
        GetCursorPos(out _lastMousePosition);
    }

    public void StopTracking()
    {
        _isTracking = false;
    }

    public void Update(double deltaTime)
    {
        if (!_isTracking) return;

        if (!GetCursorPos(out var currentMousePosition)) return;
        // Calculate relative movement
        var deltaX = currentMousePosition.X - _lastMousePosition.X;
        var deltaY = currentMousePosition.Y - _lastMousePosition.Y;


        // Update last mouse position
        _lastMousePosition = currentMousePosition;


        if (deltaX != 0 || deltaY != 0)
        {
            (X, Y) = (deltaX, deltaY);
        }
        else
        {
            (X, Y) = (0, 0);
        }
    }

    // Import GetCursorPos from User32.dll
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetCursorPos(out Point lpPoint);

    // Define a POINT structure for cursor position
    [StructLayout(LayoutKind.Sequential)]
    private struct Point
    {
        public int X;
        public int Y;
    }
}