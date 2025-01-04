using System.Diagnostics;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Renderer;

namespace VoxelSharp.Core.GameLoop
{
    public class GameLoop : IGameLoop
    {
        private readonly List<Action<double>> _tickActions = [];

        private readonly List<Action<double>> _renderActions = [];
        private readonly List<Action> _preRenderActions = [];
        private readonly List<Action> _postRenderActions = [];

        private bool _isRunning;
        private bool _isPaused;
        private int _targetTicksPerSecond = 20;
        private double _tickDuration; // Time per tick in seconds
        private readonly Stopwatch _stopwatch = new();
        private const double MaxCatchUpTime = 0.25; // Maximum allowable catch-up time (e.g., 250ms)


        public void Initialize()
        {
            _isRunning = false;
            _isPaused = false;
            SetTargetTicksPerSecond(_targetTicksPerSecond);
        }

        public void Start()
        {
            _isRunning = true;
            _stopwatch.Start();

            double accumulatedTime = 0.0;
            long previousTime = _stopwatch.ElapsedMilliseconds;

            while (_isRunning)
            {
                if (_isPaused)
                {
                    previousTime = _stopwatch.ElapsedMilliseconds;
                    continue;
                }

                long currentTime = _stopwatch.ElapsedMilliseconds;
                double deltaTime = (currentTime - previousTime) / 1000.0; // Convert to seconds
                previousTime = currentTime;

                accumulatedTime += deltaTime;

                // Cap accumulated time to prevent cascading lag
                if (accumulatedTime > MaxCatchUpTime)
                {
                    accumulatedTime = MaxCatchUpTime; // Discard excess time
                }

                // Perform ticks while accumulated time allows
                while (accumulatedTime >= _tickDuration)
                {
                    RunTick(_tickDuration); // Fixed tick duration
                    accumulatedTime -= _tickDuration;
                }

                // Render with interpolation
                double interpolationFactor = accumulatedTime / _tickDuration;
                RunRender(interpolationFactor);
            }

            _stopwatch.Stop();
        }

        public void Stop() => _isRunning = false;

        public void Pause() => _isPaused = true;

        public void Resume() => _isPaused = false;

        public bool IsRunning() => _isRunning;

        public void SetTargetTicksPerSecond(int ticksPerSecond)
        {
            _targetTicksPerSecond = ticksPerSecond;
            _tickDuration = 1.0 / ticksPerSecond;
        }

        public void RegisterUpdateAction(Action<double> tickAction)
        {
            if (!_tickActions.Contains(tickAction))
            {
                _tickActions.Add(tickAction);
            }
        }

        public void RegisterUpdateAction(IUpdatable updatable)
        {
            _tickActions.Add(updatable.Update);
        }

        public void UnregisterUpdateAction(Action<double> tickAction)
        {
            _tickActions.Remove(tickAction);
        }

        public void UnregisterUpdateAction(IUpdatable updatable)
        {
            _tickActions.Remove(updatable.Update);
        }

        public void RegisterRenderAction(Action<double> renderAction)
        {
            if (!_renderActions.Contains(renderAction))
            {
                _renderActions.Add(renderAction);
            }
        }

        public void RegisterRenderAction(IRenderer renderer)
        {
            _renderActions.Add(renderer.Render);
        }

        public void RegisterRenderProcessingAction(IRendererProcessing rendererProcessing)
        {
            _preRenderActions.Add(rendererProcessing.PreRender);
            _postRenderActions.Add(rendererProcessing.PostRender);
        }

        public void UnregisterRenderAction(Action<double> renderAction)
        {
            _renderActions.Remove(renderAction);
        }

        public void UnregisterRenderAction(IRenderer renderer)
        {
            _renderActions.Remove(renderer.Render);
        }

        public void UnregisterRenderProcessingAction(IRendererProcessing rendererProcessing)
        {
            _preRenderActions.Remove(rendererProcessing.PreRender);
            _postRenderActions.Remove(rendererProcessing.PostRender);
        }

        

        private void RunTick(double deltaTime)
        {
            foreach (var action in _tickActions)
            {
                action(deltaTime);
            }
        }

        private void RunRender(double interpolationFactor)
        {
            foreach (var preRenderAction in _preRenderActions)
            {
                preRenderAction();
            }

            foreach (var action in _renderActions)
            {
                action(interpolationFactor);
            }

            foreach (var postRenderAction in _postRenderActions)
            {
                postRenderAction();
            }
        }
    }
}