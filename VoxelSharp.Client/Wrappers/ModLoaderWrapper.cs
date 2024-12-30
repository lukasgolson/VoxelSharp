using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Modding;
using VoxelSharp.Renderer.Interfaces;

namespace VoxelSharp.Client.Wrappers;

/// <summary>
///     A wrapper around the ModLoader class, enabling rendering and updating through IRenderable and IUpdatable.
/// </summary>
public class ModLoaderWrapper : IRenderer, IUpdatable
{
    /// <summary>
    ///     Initializes a new instance of the ModLoaderWrapper.
    /// </summary>
    /// <param name="modPath">The path to the mods to load.</param>
    public ModLoaderWrapper(string modPath)
    {
        ModLoader = new ModLoader(modPath);
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
    public void Render(ICameraMatrices camera)
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