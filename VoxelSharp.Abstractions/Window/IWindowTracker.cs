namespace VoxelSharp.Abstractions.Window;

public interface IWindowTracker
{
    void StartTracking(IntPtr windowHandle);
    void StopTracking();
}