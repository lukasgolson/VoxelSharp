using System.Buffers;

namespace VoxelSharp.Renderer.Helpers;

public static class MeshBufferPool
{
    // ... existing Create logic ...
    private static readonly ArrayPool<int> Shared = ArrayPool<int>.Create(1024 * 1024, 150);

    // Diagnostics
    private static int _rentCount = 0;
    private static int _returnCount = 0;

    public static int[] Rent(int minimumLength)
    {
        Interlocked.Increment(ref _rentCount);
        return Shared.Rent(minimumLength);
    }

    public static void Return(int[] array, bool clearArray = false)
    {
        Shared.Return(array, clearArray);
        Interlocked.Decrement(ref _rentCount); // Should go back down
        Interlocked.Increment(ref _returnCount);
    }

    public static void LogStats()
    {
        // Print this every few seconds
        Console.WriteLine($"[Pool Stats] Rented: {_rentCount} (Active) | Total Returns: {_returnCount}");
    }

}