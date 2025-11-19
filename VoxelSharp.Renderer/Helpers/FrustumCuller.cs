

using System.Numerics;
using VoxelSharp.Core.Structs;

namespace VoxelSharp.Renderer.Helpers;

public static class FrustumCuller
{
    private static readonly Plane[] Planes = new Plane[6];

    public static void UpdateFrustum(Matrix4x4 viewProjection)
    {
        // Extract frustum planes from View-Projection Matrix
        
        // Left
        Planes[0] = new Plane(
            viewProjection.M14 + viewProjection.M11,
            viewProjection.M24 + viewProjection.M21,
            viewProjection.M34 + viewProjection.M31,
            viewProjection.M44 + viewProjection.M41).Normalize();

        // Right
        Planes[1] = new Plane(
            viewProjection.M14 - viewProjection.M11,
            viewProjection.M24 - viewProjection.M21,
            viewProjection.M34 - viewProjection.M31,
            viewProjection.M44 - viewProjection.M41).Normalize();

        // Bottom
        Planes[2] = new Plane(
            viewProjection.M14 + viewProjection.M12,
            viewProjection.M24 + viewProjection.M22,
            viewProjection.M34 + viewProjection.M32,
            viewProjection.M44 + viewProjection.M42).Normalize();

        // Top
        Planes[3] = new Plane(
            viewProjection.M14 - viewProjection.M12,
            viewProjection.M24 - viewProjection.M22,
            viewProjection.M34 - viewProjection.M32,
            viewProjection.M44 - viewProjection.M42).Normalize();

        // Near
        Planes[4] = new Plane(
            viewProjection.M13,
            viewProjection.M23,
            viewProjection.M33,
            viewProjection.M43).Normalize();

        // Far
        Planes[5] = new Plane(
            viewProjection.M14 - viewProjection.M13,
            viewProjection.M24 - viewProjection.M23,
            viewProjection.M34 - viewProjection.M33,
            viewProjection.M44 - viewProjection.M43).Normalize();
    }

    public static bool IsChunkVisible(Position<int> chunkPos, int chunkSize)
    {
        // Calculate AABB center and extent
        float size = chunkSize;
        float x = chunkPos.X * size + size / 2f;
        float y = chunkPos.Y * size + size / 2f;
        float z = chunkPos.Z * size + size / 2f;
        
        // Radius of the sphere enclosing the cube (sqrt(3)/2 * size)
        float radius = size * 0.8660254f; 

        // Check sphere against planes (Faster than AABB)
        foreach (var plane in Planes)
        {
            if (Vector3.Dot(plane.Normal, new Vector3(x, y, z)) + plane.D < -radius)
            {
                return false; // Outside
            }
        }
        return true;
    }
    
    // Extension to normalize planes easily
    private static Plane Normalize(this Plane p)
    {
        float len = p.Normal.Length();
        return new Plane(p.Normal / len, p.D / len);
    }
}