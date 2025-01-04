using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Modding;

namespace VoxelSharp.Client.Wrappers;

/// <summary>
///     A wrapper around the ModLoader class, enabling rendering and updating through IRenderable and IUpdatable.
/// </summary>
public class ModLoaderWrapper : IRenderer, IUpdatable
{
    /// <summary>
    ///     Initializes a new instance of the ModLoaderWrapper.
    /// </summary>
    public ModLoaderWrapper(ModLoader modLoader)
    {
        ModLoader = modLoader;
    }


    /// <summary>
    ///     Gets the underlying ModLoader instance.
    /// </summary>
    public ModLoader ModLoader { get; }

    public void InitializeShaders()
    {
        ModLoader.InitializeShaders();
    }

    /// <summary>
    ///     Renders the mods by delegating to the underlying ModLoader.
    /// </summary>
    public void Render(double interpolationFactor)
    {
        ModLoader.Render();
    }


    /// <summary>
    ///     Updates the mods by delegating to the underlying ModLoader.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last update.</param>
    public void Update(double deltaTime)
    {
        ModLoader.Update(deltaTime);
    }
}