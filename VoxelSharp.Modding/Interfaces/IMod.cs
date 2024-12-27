using HarmonyLib;
using VoxelSharp.Modding.Structs;

namespace VoxelSharp.Modding.Interfaces;

/// <summary>
/// Interface representing a mod in the VoxelSharp framework. 
/// Implement this interface to define the behaviour and lifecycle of your mod.
/// </summary>
public interface IMod
{
    /// <summary>
    /// Gets information about the mod, including its name, version, and author details.
    /// </summary>
    ModInfo ModInfo { get; }

    /// <summary>
    /// Called before the mod is initialized. 
    /// Use this method to perform any setup tasks that do not rely on other mods being initialized.
    /// </summary>
    /// <returns>
    /// Returns <c>true</c> if the pre-initialization was successful; otherwise, <c>false</c>.
    /// </returns>
    bool PreInitialize();

    /// <summary>
    /// Called when the mod is initialized. 
    /// Perform setup tasks that depend on other mods being initialized, and register any HarmonyX patches using the provided <see cref="Harmony"/> instance.
    /// 
    /// For more information on HarmonyX, visit <see href="https://github.com/BepInEx/HarmonyX/wiki">HarmonyX Documentation</see>.
    /// </summary>
    /// <param name="harmony">The Harmony instance to use for patch registration.</param>
    /// <returns>
    /// Returns <c>true</c> if the initialization was successful; otherwise, <c>false</c>.
    /// </returns>
    bool Initialize(Harmony harmony);

    /// <summary>
    /// Called once per frame to update the mod's state.
    /// Use the <paramref name="deltaTime"/> parameter to calculate the time elapsed since the last update, as the interval between calls is not guaranteed to be constant.
    /// 
    /// Note: This method is a temporary solution and may be replaced with a more robust system in future versions.
    /// </summary>
    /// <param name="deltaTime">The time, in seconds, that has elapsed since the last update.</param>
    /// <returns>
    /// Returns <c>true</c> if the update was successful; otherwise, <c>false</c>.
    /// </returns>
    bool Update(float deltaTime);

    /// <summary>
    /// Called once per frame to render the mod.
    /// 
    /// Note: This method is a temporary solution and may be replaced with a more robust system in future versions.
    /// </summary>
    /// <returns>
    /// Returns <c>true</c> if rendering was successful; otherwise, <c>false</c>.
    /// </returns>
    bool Render();
}
