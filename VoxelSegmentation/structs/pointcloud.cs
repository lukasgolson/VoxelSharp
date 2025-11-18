using System.Globalization;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using VoxelSharp.Core.Structs;

namespace VoxelSegmentation.structs;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Point(float x, float y, float z, float r, float g, float b)
{
    public readonly float X = x;
    public readonly float Y = y, Z = z;
    public readonly float R = r, G = g, B = b;
}

public class Pointcloud : IDisposable
{
    private MemoryMappedFile _mmf;
    private MemoryMappedViewAccessor _accessor;
    private long _pointCount;
    private int _pointSize; // Size of a single Point struct

    public Pointcloud(string filename, char delimiter = ' ')
    {
        LoadFromTxtToMmf(filename, delimiter);
    }

    
    private void LoadFromTxtToMmf(string filePath, char delimiter = ' ')
    {
        try
        {
            _pointCount = 0;
            foreach (var line in File.ReadLines(filePath))
            {
                if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith('#'))
                {
                    _pointCount++;
                }
            }

            if (_pointCount == 0)
            {
                Console.WriteLine("Warning: No valid points found in file.");
                return;
            }

            _pointSize = Marshal.SizeOf<Point>();
            long requiredBytes = _pointCount * _pointSize;

            string tempMmfPath = Path.GetTempFileName();
            Console.WriteLine("Creating {0} points.", tempMmfPath);
            _mmf = MemoryMappedFile.CreateFromFile(tempMmfPath, FileMode.Create, null, requiredBytes);
            _accessor = _mmf.CreateViewAccessor();

            long currentByteOffset = 0;
            foreach (var line in File.ReadLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                {
                    continue;
                }

                var parts = line.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length < 6) continue; // Skip malformed

                if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) &&
                    float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y) &&
                    float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var z) &&
                    float.TryParse(parts[^3], NumberStyles.Float, CultureInfo.InvariantCulture, out var r) &&
                    float.TryParse(parts[^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var g) &&
                    float.TryParse(parts[^1], NumberStyles.Float, CultureInfo.InvariantCulture, out var b))
                {
                    var point = new Point(x, y, z, r, g, b);

                    _accessor.Write(currentByteOffset, ref point);
                    currentByteOffset += _pointSize;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while loading to MMF: {ex.Message}");
            _accessor?.Dispose();
            _mmf?.Dispose();
        }
    }


    public unsafe List<Point> Quantize(float voxelSize)
    {
        switch (_pointCount)
        {
            case 0:
                return []; // No data
            case > int.MaxValue:
                Console.WriteLine(
                    $"Error: Point count ({_pointCount}) exceeds int.MaxValue. Cannot use unsafe Span. Reverting to safe read.");
      
                throw new OverflowException(
                    "Cannot create a Span larger than int.MaxValue. Implement a chunking or safe-read fallback.");
        }

        var points = new Dictionary<(float x, float y, float z), (int count, float r, float g, float b)>();
        byte* pointer = null;

        try
        {
            _accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref pointer);

            var span = new Span<Point>(pointer, (int)_pointCount);

            foreach (var point in span)
            {
                var x = MathF.Floor(point.X / voxelSize);
                var y = MathF.Floor(point.Y / voxelSize);
                var z = MathF.Floor(point.Z / voxelSize);
                var key = (x, y, z);

                if (points.TryGetValue(key, out var currentVal))
                {
                    points[key] = (currentVal.count + 1,
                        currentVal.r + point.R,
                        currentVal.g + point.G,
                        currentVal.b + point.B);
                }
                else
                {
                    points.Add(key, (1, point.R, point.G, point.B));
                }
            }
        }
        finally
        {
            if (pointer != null)
            {
                _accessor.SafeMemoryMappedViewHandle.ReleasePointer();
            }
        }

        var quantizedList = points.AsParallel()
            .Select(pair =>
            {
                var (key, val) = pair;
                var r = val.r / val.count;
                var g = val.g / val.count;
                var b = val.b / val.count;
                return new Point(key.x, key.y, key.z, r, g, b);
            })
            .ToList();

        return quantizedList;
    }

    // --- IDisposable Implementation ---
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _accessor?.Dispose();
            _accessor = null;
            _mmf?.Dispose();
            _mmf = null;
        }
    }
}