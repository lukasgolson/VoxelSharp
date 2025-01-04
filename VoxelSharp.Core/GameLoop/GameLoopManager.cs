using System.Diagnostics;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Renderer;

namespace VoxelSharp.Core.GameLoop;

public class GameLoop : IGameLoop
{
    private const double MaxCatchUpTime = 2.0; // Maximum allowable catch-up time in seconds
    private readonly List<Action> _postRenderActions = [];
    private readonly List<Action> _preRenderActions = [];
    private readonly List<Action<double>> _renderActions = [];

    private readonly Stopwatch _stopwatch = new();
    private readonly List<Action<double>> _tickActions = [];

    private readonly long _ticksPerSecond;
    private double _frameDuration;
    private int _framesRendered;
    private double _frameTimeAccumulator;
    private bool _isPaused;

    private bool _isRunning;
    private int _targetFramesPerSecond = 60;
    private int _targetTicksPerSecond = 60;

    private double _tickDuration;


    private int _ticksProcessed;
    private double _tickTimeAccumulator;

    public GameLoop()
    {
        _isRunning = false;
        _isPaused = false;
        SetTargetTicksPerSecond(_targetTicksPerSecond);
        SetTargetFramesPerSecond(_targetFramesPerSecond);
        _ticksPerSecond = Stopwatch.Frequency;
    }

    public double CurrentUpdateFrequency { get; private set; }
    public double CurrentRenderFrequency { get; private set; }


    public void Start()
    {
        _isRunning = true;
        _stopwatch.Start();

        var previousTime = _stopwatch.ElapsedTicks;
        var accumulatedTime = 0.0;

        var tickAccumulator = 0.0;
        var frameAccumulator = 0.0;

        while (_isRunning)
        {
            var currentTime = _stopwatch.ElapsedTicks;
            var deltaTime = (currentTime - previousTime) / (double)_ticksPerSecond;
            previousTime = currentTime;

            if (!_isPaused)
            {
                accumulatedTime += deltaTime;

                if (accumulatedTime > MaxCatchUpTime) accumulatedTime = MaxCatchUpTime; // Prevent spiral of death

                tickAccumulator += deltaTime;
                frameAccumulator += deltaTime;

                // Interlace updates and frames
                while (tickAccumulator >= _tickDuration || frameAccumulator >= _frameDuration)
                {
                    if (tickAccumulator >= _tickDuration)
                    {
                        RunTick(_tickDuration);
                        tickAccumulator -= _tickDuration;
                    }

                    if (frameAccumulator >= _frameDuration)
                    {
                        RunRender(frameAccumulator /
                                  _frameDuration); // Use interpolation factor for smooth rendering
                        frameAccumulator -= _frameDuration;
                    }
                }

                UpdatePerformanceMetrics();
            }
        }

        _stopwatch.Stop();
    }

    public void Stop()
    {
        _isRunning = false;
    }

    public void Pause()
    {
        _isPaused = true;
    }

    public void Resume()
    {
        _isPaused = false;
    }

    public bool IsRunning()
    {
        return _isRunning;
    }

    public void SetTargetTicksPerSecond(int ticksPerSecond)
    {
        _targetTicksPerSecond = ticksPerSecond;
        _tickDuration = 1.0 / ticksPerSecond;
    }

    public void RegisterUpdateAction(Action<double> tickAction)
    {
        if (!_tickActions.Contains(tickAction)) _tickActions.Add(tickAction);
    }

    public void RegisterUpdateAction(IUpdatable updatable)
    {
        RegisterUpdateAction(updatable.Update);
    }

    public void UnregisterUpdateAction(Action<double> tickAction)
    {
        _tickActions.Remove(tickAction);
    }

    public void UnregisterUpdateAction(IUpdatable updatable)
    {
        UnregisterUpdateAction(updatable.Update);
    }

    public void RegisterRenderAction(Action<double> renderAction)
    {
        if (!_renderActions.Contains(renderAction)) _renderActions.Add(renderAction);
    }

    public void RegisterRenderAction(IRenderer renderer)
    {
        RegisterRenderAction(renderer.Render);
    }

    public void RegisterRenderProcessingAction(IRendererProcessing rendererProcessing)
    {
        if (!_preRenderActions.Contains(rendererProcessing.PreRender))
            _preRenderActions.Add(rendererProcessing.PreRender);

        if (!_postRenderActions.Contains(rendererProcessing.PostRender))
            _postRenderActions.Add(rendererProcessing.PostRender);
    }

    public void UnregisterRenderAction(Action<double> renderAction)
    {
        _renderActions.Remove(renderAction);
    }

    public void UnregisterRenderAction(IRenderer renderer)
    {
        UnregisterRenderAction(renderer.Render);
    }

    public void UnregisterRenderProcessingAction(IRendererProcessing rendererProcessing)
    {
        _preRenderActions.Remove(rendererProcessing.PreRender);
        _postRenderActions.Remove(rendererProcessing.PostRender);
    }

    private void UpdatePerformanceMetrics()
    {
        const double updateInterval = 1.0; // Update metrics every second

        if (_tickTimeAccumulator >= updateInterval)
        {
            CurrentUpdateFrequency = _ticksProcessed / _tickTimeAccumulator;
            _ticksProcessed = 0;
            _tickTimeAccumulator = 0.0;
        }

        if (_frameTimeAccumulator >= updateInterval)
        {
            CurrentRenderFrequency = _framesRendered / _frameTimeAccumulator;
            _framesRendered = 0;
            _frameTimeAccumulator = 0.0;
        }
    }

    public void SetTargetFramesPerSecond(int framesPerSecond)
    {
        _targetFramesPerSecond = framesPerSecond;
        _frameDuration = 1.0 / framesPerSecond;
    }

    private void RunTick(double deltaTime)
    {
        foreach (var action in _tickActions) action(deltaTime);

        _ticksProcessed++;
        _tickTimeAccumulator += deltaTime;
    }

    private void RunRender(double interpolationFactor)
    {
        foreach (var preRenderAction in _preRenderActions) preRenderAction();

        foreach (var action in _renderActions) action(interpolationFactor);

        foreach (var postRenderAction in _postRenderActions) postRenderAction();

        _framesRendered++;
        _frameTimeAccumulator += interpolationFactor * _tickDuration;
    }
}