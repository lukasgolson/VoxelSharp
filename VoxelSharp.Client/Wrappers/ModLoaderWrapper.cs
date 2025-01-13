using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Renderer;
using VoxelSharp.Modding;

namespace VoxelSharp.Client.Wrappers;

/// <summary>
///     A wrapper around the ModLoader class, enabling rendering and updating through IRenderable and IUpdatable.
/// </summary>
public class ModLoaderWrapper
{
    /// <summary>
    ///     Initializes a new instance of the ModLoaderWrapper.
    /// </summary>
    public ModLoaderWrapper(ModLoader? modLoader)
    {
        ModLoader = modLoader;
    }


    /// <summary>
    ///     Gets the underlying ModLoader instance.
    /// </summary>
    public ModLoader? ModLoader { get; }

  
}