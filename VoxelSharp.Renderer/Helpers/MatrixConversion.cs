using System.Numerics;
using OpenTK.Mathematics;

namespace VoxelSharp.Renderer.Helpers;

// For Matrix4x4

// For Matrix4

public static class MatrixConversion
{
    public static Matrix4 ToMatrix4(this Matrix4x4 systemMatrix)
    {
        return new Matrix4(
            systemMatrix.M11, systemMatrix.M12, systemMatrix.M13, systemMatrix.M14,
            systemMatrix.M21, systemMatrix.M22, systemMatrix.M23, systemMatrix.M24,
            systemMatrix.M31, systemMatrix.M32, systemMatrix.M33, systemMatrix.M34,
            systemMatrix.M41, systemMatrix.M42, systemMatrix.M43, systemMatrix.M44
        );
    }

    public static Matrix4x4 ToMatrix4x4(this Matrix4 openTKMatrix)
    {
        return new Matrix4x4(
            openTKMatrix.M11, openTKMatrix.M12, openTKMatrix.M13, openTKMatrix.M14,
            openTKMatrix.M21, openTKMatrix.M22, openTKMatrix.M23, openTKMatrix.M24,
            openTKMatrix.M31, openTKMatrix.M32, openTKMatrix.M33, openTKMatrix.M34,
            openTKMatrix.M41, openTKMatrix.M42, openTKMatrix.M43, openTKMatrix.M44
        );
    }
}