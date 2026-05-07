using System.Globalization;
using System.IO;
using System.Numerics;
using VoxelSharp.Core.Structs;

namespace VoxelSegmentation.structs;

public readonly struct Point(float x, float y, float z, float r, float g, float b)
{
    public readonly float X = x;
    public readonly float Y = y, Z = z;
    public readonly float R = r, G = g, B = b;
}

public class Pointcloud
{
    private List<Point> _points = new();
    
    // Store bounds to help with centering
    private Vector3 _minBounds = new(float.MaxValue);

    // Modified constructor to accept an optional progress callback
    public Pointcloud(string filename, char delimiter = ' ', Action<float>? onProgress = null)
    {
        LoadFromTxt(filename, delimiter, onProgress);
    }

    private void LoadFromTxt(string filePath, char delimiter, Action<float>? onProgress)
    {
        try
        {
            // Get file size for progress
            long totalBytes = new FileInfo(filePath).Length;
            long bytesRead = 0;

            using var sr = new StreamReader(filePath);
            string? line;
            
            // We'll update progress every ~100 lines to avoid overhead
            int lineCount = 0;

            while ((line = sr.ReadLine()) != null)
            {
                bytesRead += line.Length + 2; // Approx bytes (line + newline)
                lineCount++;

                if (onProgress != null && lineCount % 1000 == 0)
                {
                    onProgress((float)bytesRead / totalBytes);
                }

                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;

                var parts = line.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length < 6) continue;

                if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) &&
                    float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y) &&
                    float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var z) &&
                    float.TryParse(parts[^3], NumberStyles.Float, CultureInfo.InvariantCulture, out var r) &&
                    float.TryParse(parts[^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var g) &&
                    float.TryParse(parts[^1], NumberStyles.Float, CultureInfo.InvariantCulture, out var b))
                {
                    _points.Add(new Point(x, y, z, r, g, b));
                    
                    // Track the minimum values found
                    if (x < _minBounds.X) _minBounds.X = x;
                    if (y < _minBounds.Y) _minBounds.Y = y;
                    if (z < _minBounds.Z) _minBounds.Z = z;
                }
            }
            
            Console.WriteLine($"Successfully loaded {_points.Count} points. Min Bounds: {_minBounds}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while loading PointCloud: {ex.Message}");
        }
    }

    // NEW: Method to rotate all points
    public void Rotate(Vector3 rotationDegrees)
    {
        // Convert to radians
        float pitch = rotationDegrees.X * (MathF.PI / 180f);
        float yaw = rotationDegrees.Y * (MathF.PI / 180f);
        float roll = rotationDegrees.Z * (MathF.PI / 180f);

        var rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, roll);
        var newPoints = new List<Point>(_points.Count);
        
        // Reset bounds
        _minBounds = new Vector3(float.MaxValue);

        foreach (var p in _points)
        {
            var v = new Vector3(p.X, p.Y, p.Z);
            var rotated = Vector3.Transform(v, rotationMatrix);
            
            newPoints.Add(new Point(rotated.X, rotated.Y, rotated.Z, p.R, p.G, p.B));

            // Re-calculate bounds
            if (rotated.X < _minBounds.X) _minBounds.X = rotated.X;
            if (rotated.Y < _minBounds.Y) _minBounds.Y = rotated.Y;
            if (rotated.Z < _minBounds.Z) _minBounds.Z = rotated.Z;
        }

        _points = newPoints;
    }

    public List<Point> Quantize(float voxelSize, bool centerAtOrigin = true)
    {
        if (_points.Count == 0) return [];

        var voxelMap = new Dictionary<Position<int>, (int count, float r, float g, float b)>();

        // If centering, we subtract the min bounds so the model starts at (0,0,0)
        float offsetX = centerAtOrigin ? _minBounds.X : 0;
        float offsetY = centerAtOrigin ? _minBounds.Y : 0;
        float offsetZ = centerAtOrigin ? _minBounds.Z : 0;

        foreach (var point in _points)
        {
            // Subtract offset BEFORE scaling
            var x = (int)MathF.Floor((point.X - offsetX) / voxelSize);
            var y = (int)MathF.Floor((point.Y - offsetY) / voxelSize);
            var z = (int)MathF.Floor((point.Z - offsetZ) / voxelSize);
            
            var key = new Position<int>(x, y, z);

            if (voxelMap.TryGetValue(key, out var val))
            {
                voxelMap[key] = (val.count + 1, val.r + point.R, val.g + point.G, val.b + point.B);
            }
            else
            {
                voxelMap.Add(key, (1, point.R, point.G, point.B));
            }
        }

        var quantizedList = new List<Point>(voxelMap.Count);
        foreach (var kvp in voxelMap)
        {
            var pos = kvp.Key;
            var val = kvp.Value;
            
            var r = val.r / val.count;
            var g = val.g / val.count;
            var b = val.b / val.count;
            
            quantizedList.Add(new Point(pos.X, pos.Y, pos.Z, r, g, b));
        }

        return quantizedList;
    }
}