using System.Diagnostics;
using VoxelSharp.Abstractions.Loop;
using VoxelSharp.Abstractions.Renderer;

namespace VoxelSharp.Core.GameLoop;

public class GameLoop : IGameLoop
{
    internal struct PrioritizedProcessor : IEquatable<PrioritizedProcessor>
    {
        public IRendererProcessing Processor;
        public int Priority;


        public bool Equals(PrioritizedProcessor other)
        {
            return Processor.Equals(other.Processor) && Priority == other.Priority;
        }

        public override bool Equals(object? obj)
        {
            return obj is PrioritizedProcessor other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Processor, Priority);
        }

        public static bool operator ==(PrioritizedProcessor left, PrioritizedProcessor right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PrioritizedProcessor left, PrioritizedProcessor right)
        {
            return !left.Equals(right);
        }
    }


    private const double MaxCatchUpTime = 2.0; // Maximum allowable catch-up time in seconds

    private readonly List<PrioritizedProcessor> _processingActions = [];
    private readonly List<Renderer> _renderActions = [];

    private readonly Stopwatch _stopwatch = new();
    private readonly List<Action<double>> _tickActions = [];

    private readonly Dictionary<string, GameLoop> _backgroundLoops = new();
    private readonly Dictionary<string, CancellationTokenSource> _backgroundLoopCts = new();
    private readonly Dictionary<string, Task> _backgroundLoopTasks = new();
    private readonly object _loopLock = new object(); // To protect the dictionaries

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


    internal struct Renderer : IEquatable<Renderer>
    {
        public Action<double> RenderAction;
        public int priority;


        public bool Equals(Renderer other)
        {
            return RenderAction.Equals(other.RenderAction);
        }

        public override bool Equals(object? obj)
        {
            return obj is Renderer other && Equals(other);
        }

        public override int GetHashCode()
        {
            return RenderAction.GetHashCode();
        }

        public static bool operator ==(Renderer left, Renderer right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Renderer left, Renderer right)
        {
            return !left.Equals(right);
        }
    }

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
    public bool IsRenderDecoupled { get; set; }



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
                if (accumulatedTime > MaxCatchUpTime) accumulatedTime = MaxCatchUpTime;

                tickAccumulator += deltaTime;

                // 1. Always run Logic/Physics
                while (tickAccumulator >= _tickDuration)
                {
                    RunTick(_tickDuration);
                    tickAccumulator -= _tickDuration;
                }

                // 2. Only run Rendering if we own the render loop!
                if (!IsRenderDecoupled)
                {
                    frameAccumulator += deltaTime;
                    while (frameAccumulator >= _frameDuration)
                    {
                        RenderFrame(frameAccumulator / _frameDuration);
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

        lock (_loopLock)
        {
            foreach (var loop in _backgroundLoops.Values) loop.Stop();
            foreach (var cts in _backgroundLoopCts.Values) cts.Cancel();

            try
            {
                // Wait for all background tasks to shut down
                Task.WhenAll(_backgroundLoopTasks.Values).Wait(TimeSpan.FromSeconds(2));
            }
            catch (Exception)
            {
                // Log this failure
            }

            _backgroundLoops.Clear();
            _backgroundLoopCts.Clear();
            _backgroundLoopTasks.Clear();
        }
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

    public void RegisterBackgroundUpdateAction(IUpdatable updatable, string loopName)
    {
        lock (_loopLock)
        {
            if (!_backgroundLoops.TryGetValue(loopName, out var loop))
            {
                // The loop doesn't exist, so "spin one up"
                // _logger.LogInformation("Spinning up new background loop: {loopName}", loopName);

                loop = new GameLoop();
                loop.SetTargetTicksPerSecond(30); // Configurable background TPS

                var cts = new CancellationTokenSource();
                var task = Task.Run(() => loop.Start(), cts.Token);

                // Store all the new objects
                _backgroundLoops[loopName] = loop;
                _backgroundLoopCts[loopName] = cts;
                _backgroundLoopTasks[loopName] = task;
            }

            // Add the updatable to the correct loop
            loop.RegisterUpdateAction(updatable);
        }
    }

    public void StopBackgroundLoop(string loopName)
    {
        lock (_loopLock)
        {
            if (_backgroundLoops.TryGetValue(loopName, out var loop))
            {
                // _logger.LogInformation("Spinning down background loop: {loopName}", loopName);

                // Stop the loop and cancel the task
                loop.Stop();
                _backgroundLoopCts[loopName].Cancel();

                // Remove from management
                _backgroundLoops.Remove(loopName);
                _backgroundLoopCts.Remove(loopName);
                _backgroundLoopTasks.Remove(loopName);
            }
        }
    }


    public void RegisterUpdateAction(IUpdatable updatable, int i)
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

    public void RegisterRenderAction(Action<double> renderAction, int priority = 0)
    {
        var renderer = new Renderer { RenderAction = renderAction, priority = priority };

        if (!_renderActions.Contains(renderer))
        {
            _renderActions.Add(renderer);
        }

        _renderActions.Sort((a, b) => a.priority.CompareTo(b.priority));
    }

    public void RegisterRenderAction(IRenderer renderer, int priority = 0)
    {
        RegisterRenderAction(renderer.Render);
    }

    public void RegisterRenderProcessingAction(IRendererProcessing rendererProcessing, int priority = 0)
    {
        var action = new PrioritizedProcessor 
        { 
            Processor = rendererProcessing, 
            Priority = priority 
        };

        if (_processingActions.Contains(action)) return;
        _processingActions.Add(action);
        _processingActions.Sort((a, b) => a.Priority.CompareTo(b.Priority));
    }

    public void UnregisterRenderAction(Action<double> renderAction)
    {
        _renderActions.Remove(new Renderer { RenderAction = renderAction });
    }


    public void UnregisterRenderAction(IRenderer renderer)
    {
        UnregisterRenderAction(renderer.Render);
    }

    public void UnregisterRenderProcessingAction(IRendererProcessing rendererProcessing)
    {
        _processingActions.RemoveAll(p => p.Equals(rendererProcessing));
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

    public void RenderFrame(double interpolationFactor)
    {
        // Run PreRender in ASCENDING order (low to high)
        foreach (var action in _processingActions)
            action.Processor.PreRender();

        // Run main render actions (already prioritized)
        foreach (var action in _renderActions) 
            action.RenderAction(interpolationFactor);

        // Run PostRender in DESCENDING order (high to low)
        for (int i = _processingActions.Count - 1; i >= 0; i--)
            _processingActions[i].Processor.PostRender();

        _framesRendered++;
        _frameTimeAccumulator += interpolationFactor * _tickDuration;
    }
}