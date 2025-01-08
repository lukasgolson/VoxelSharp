using Schedulers;
using SimpleInjector;

namespace VoxelSharp.Core.ECS;

public class Ecs : IDisposable
{
    private readonly JobScheduler _scheduler = new(new JobScheduler.Config()
    {
        ThreadPrefixName = "VoxelSharp",
        ThreadCount = 0,
        MaxExpectedConcurrentJobs = 64,
        StrictAllocationMode = false,
    });

    ~Ecs()
    {
        Dispose(false);
    }

    public void AddWorldToContainer(Container container)
    {
        var world = Arch.Core.World.Create();


        Arch.Core.World.SharedJobScheduler = _scheduler;



        container.RegisterInstance(world);
    }

    private static void ReleaseUnmanagedResources()
    {
        
    }

    private void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
        if (disposing)
        {
            _scheduler.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}