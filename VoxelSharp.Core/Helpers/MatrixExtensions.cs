using System.Numerics;

namespace VoxelSharp.Core.Helpers;

public static class MatrixExtensions
{
    public static Matrix4x4 ClearTranslation(this Matrix4x4 matrix)
    {
        return new Matrix4x4(
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            0f, 0f, 0f, 1 // Zero out translation
        );
    }
}