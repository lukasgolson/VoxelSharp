using System.Diagnostics;
using System.Numerics;
using VoxelSharp.Core.Structs;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace VoxelSharp.Core.Extensions;

public static class OpenTkPositionExtension
{
    /// <summary>
    /// Ensures that the generic type T is a float-like numeric type (float or double).
    /// This check will only occur in debug builds.
    /// </summary>
    /// <typeparam name="T">The numeric type to check.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown if T is not a float-like type.</exception>
    [Conditional("DEBUG")]
    private static void EnsureFloatLike<T>() where T : INumber<T>
    {
        if (typeof(T) != typeof(float) && typeof(T) != typeof(double))
        {
            throw new InvalidOperationException($"The type {typeof(T)} is not a float-like type.");
        }
    }

    /// <summary>
    /// Converts a Position<T> to a Vector3.
    /// </summary>
    /// <typeparam name="T">The numeric type of the Position coordinates.</typeparam>
    /// <param name="position">The Position<T> to convert.</param>
    /// <returns>A Vector3 representing the Position<T>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if T cannot be converted to float.</exception>
    public static Vector3 ToVector3<T>(this Position<T> position) where T : INumber<T>
    {
        EnsureFloatLike<T>();

        var x = Convert.ToSingle(position.X);
        var y = Convert.ToSingle(position.Y);
        var z = Convert.ToSingle(position.Z);

        return new Vector3(x, y, z);
    }
}