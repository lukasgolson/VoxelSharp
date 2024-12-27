using VoxelSharp.Core.Interfaces;
using VoxelSharp.Modding;

namespace VoxelSharp.Core.Wrappers;

/// <summary>
/// A wrapper around the ModLoader class, enabling rendering and updating through IRenderable and IUpdatable.
/// </summary>
public class ModLoaderWrapper : IRenderable, IUpdatable
{
    /// <summary>
    /// Gets the underlying ModLoader instance.
    /// </summary>
    public ModLoader ModLoader { get; }

    /// <summary>
    /// Initializes a new instance of the ModLoaderWrapper.
    /// </summary>
    /// <param name="modPath">The path to the mods to load.</param>
    public ModLoaderWrapper(string modPath)
        => ModLoader = new ModLoader(modPath);

    /// <summary>
    /// Initializes a new instance of the ModLoaderWrapper with an existing ModLoader instance.
    /// </summary>
    /// <param name="modLoader">An existing ModLoader instance.</param>
    public ModLoaderWrapper(ModLoader modLoader)
        => ModLoader = modLoader;

    /// <summary>
    /// Renders the mods by delegating to the underlying ModLoader.
    /// </summary>
    public void Render() => ModLoader.Render();

    /// <summary>
    /// Updates the mods by delegating to the underlying ModLoader.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last update.</param>
    public void Update(float deltaTime) => ModLoader.Update(deltaTime);


}