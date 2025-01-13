using HarmonyLib;
using SimpleInjector;
using VoxelSharp.Modding.Structs;

namespace VoxelSharp.Modding.Interfaces;

/// <summary>
///     Interface representing a mod in the VoxelSharp framework.
///     Implement this interface to define the behaviour and lifecycle of your mod.
/// </summary>
public interface IMod
{
    /// <summary>
    ///     Gets information about the mod, including its name, version, and author details.
    /// </summary>
    ModInfo ModInfo { get; }

    /// <summary>
    ///     Called before the mod is initialized.
    ///     Use this method to perform any setup tasks that do not rely on other mods being initialized.
    /// </summary>
    /// <param name="container">
    ///     The SimpleInjector container to register dependencies. This container is shared with all other mods and should not be used to retrieve dependencies.
    /// </param>
    /// <returns>
    ///     Returns <c>true</c> if the pre-initialization was successful; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>The container is shared with all other mods and should not be used to retrieve dependencies in this method as it will lock the dependency graph.</remarks>

    bool PreInitialize(Harmony harmony, Container container);

    /// <summary>
    ///     Called when the mod is initialized.
    ///     Perform setup tasks that depend on other mods being initialized, and register any HarmonyX patches using the
    ///     provided <see cref="Harmony" /> instance.
    /// </summary>
    /// <param name="harmony">The Harmony instance to use for patch registration.</param>
    /// <param name="container">
    ///     The SimpleInjector container to register dependencies.  
    /// </param>
    /// <returns>
    ///     Returns <c>true</c> if the initialization was successful; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>The container is shared with all other mods and should not be used to retrieve dependencies in this method as it will lock the dependency graph.</remarks>
    bool Initialize(Harmony harmony, Container container);

    /// <summary>
    /// Called once all the mods are initialized to perform any post-initialization tasks.
    /// </summary>
    /// <param name="container">The SimpleInjector container to retrieve dependencies.</param>
    /// <returns>
    /// Returns <c>true</c> if the post-initialization was successful; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>The container can be used to retrieve dependencies in this method.</remarks>
    bool PostInitialize(Container container);

  
}