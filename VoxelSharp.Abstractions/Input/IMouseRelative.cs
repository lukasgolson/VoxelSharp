using VoxelSharp.Abstractions.Window;

namespace VoxelSharp.Abstractions.Input;

public interface IMouseRelative : IWindowTracker
{
    public double RelativeX { get; }
    public double RelativeY { get; }
}