using System.Numerics;

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
}