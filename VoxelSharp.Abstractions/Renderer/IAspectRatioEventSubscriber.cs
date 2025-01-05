namespace VoxelSharp.Abstractions.Renderer;

public interface IAspectRatioEventSubscriber
{
    /// <summary>
    ///     Updates the aspect ratio for the camera's projection matrix.
    /// </summary>
    public void UpdateAspectRatio(float aspectRatio);
}