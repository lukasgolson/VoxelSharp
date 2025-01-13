using System.Numerics;
using VoxelSharp.Core.Structs;

namespace VoxelSharp.Core.Helpers;

public static class Vector3Extensions
{
    public static Vector3 Normalize(this Vector3 vector)
    {
        return Vector3.Normalize(vector);
    }

    public static float Magnitude(this Vector3 vector)
    {
        return vector.Length();
    }
    
    public static Vector3 Cross(this Vector3 vector, Vector3 other)
    {
        return Vector3.Cross(vector, other);
    }
    
    public static Position<float> AsPosition(this Vector3 vector)
    {
        return new Position<float>(vector.X, vector.Y, vector.Z);
    }
}