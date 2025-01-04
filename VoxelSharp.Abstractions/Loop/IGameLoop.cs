using VoxelSharp.Abstractions.Renderer;

namespace VoxelSharp.Abstractions.Loop
{
    /// <summary>
    /// Interface for a tick-based game loop manager with support for dynamic tick rate, rendering, and interpolation.
    /// </summary>
    public interface IGameLoop
    {
      

        /// <summary>
        /// Starts the game loop.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the game loop.
        /// </summary>
        void Stop();

        /// <summary>
        /// Sets the target number of ticks per second.
        /// </summary>
        /// <param name="ticksPerSecond">The desired ticks per second.</param>
        void SetTargetTicksPerSecond(int ticksPerSecond);

        /// <summary>
        /// Pauses the game loop.
        /// </summary>
        void Pause();

        /// <summary>
        /// Resumes the game loop if paused.
        /// </summary>
        void Resume();

        /// <summary>
        /// Determines whether the game loop is currently running.
        /// </summary>
        /// <returns>True if the game loop is running; otherwise, false.</returns>
        bool IsRunning();

        /// <summary>
        /// Registers a tick action, which is called during each tick with a delta time.
        /// </summary>
        /// <param name="tickAction">The action to execute during each tick.</param>
        void RegisterUpdateAction(Action<double> tickAction);
        
        void RegisterUpdateAction(IUpdatable updatable);
        

        /// <summary>
        /// Unregisters a previously registered tick action.
        /// </summary>
        /// <param name="tickAction">The action to remove.</param>
        void UnregisterUpdateAction(Action<double> tickAction);
        
        void UnregisterUpdateAction(IUpdatable updatable);

        /// <summary>
        /// Registers a render action, which is called as often as possible with an interpolation factor.
        /// </summary>
        /// <param name="renderAction">The action to execute during rendering.</param>
        void RegisterRenderAction(Action<double> renderAction);
        
        void RegisterRenderAction(IRenderer renderer);
        
        void RegisterRenderProcessingAction(IRendererProcessing rendererProcessing);

        /// <summary>
        /// Unregisters a previously registered render action.
        /// </summary>
        /// <param name="renderAction">The action to remove.</param>
        void UnregisterRenderAction(Action<double> renderAction);
        
        void UnregisterRenderAction(IRenderer renderer);
        
        void UnregisterRenderProcessingAction(IRendererProcessing rendererProcessing);
    }
}
