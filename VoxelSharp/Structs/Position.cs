using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace VoxelSharp.Structs;

/// <summary>
/// Represents a 3D position with generic numeric type.
/// </summary>
/// <typeparam name="T">The numeric type of the position coordinates.</typeparam>
public readonly struct Position<T> where T : INumber<T>
{
    /// <summary>
    /// Gets the X coordinate.
    /// </summary>
    public T X { get; }

    /// <summary>
    /// Gets the Y coordinate.
    /// </summary>
    public T Y { get; }


    /// <summary>
    /// Gets the Z coordinate.
    /// </summary>
    public T Z { get; }


    /// <summary>
    /// Initializes a new instance of the <see cref="Position{T}"/> struct.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="z">The Z coordinate.</param>
    public Position(T x, T y, T z)
    {
        X = x;
        Y = y;
        Z = z;

        if (typeof(T) == typeof(float))
        {
            WarnAboutFloatType();
        }
    }

    [Conditional("DEBUG")] // Only emits the warning in Debug builds
    private static void WarnAboutFloatType()
    {
        Debug.WriteLine(
            "Warning: Position<float> is not recommended for performance-critical code because it does not support SIMD operations. Consider using Vector3 instead.");
    }


    /// <summary>
    /// Calculates the magnitude of the vector.
    /// </summary>
    /// <returns>The magnitude of the vector.</returns>
    public T Magnitude()
    {
        // Use a helper to compute the square root for numeric types.
        var sumOfSquares = X * X + Y * Y + Z * Z;
        return GenericMath.Sqrt(sumOfSquares);
    }

    /// <summary>
    /// Normalizes the vector.
    /// </summary>
    /// <returns>A new <see cref="Position{T}"/> representing the normalized vector.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the vector is a zero vector.</exception>
    public Position<T> Normalize()
    {
        var magnitude = Magnitude();
        if (magnitude == T.Zero)
            throw new InvalidOperationException("Cannot normalize a zero vector.");

        return new Position<T>(X / magnitude, Y / magnitude, Z / magnitude);
    }

    /// <summary>
    /// Adds two vectors.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>The sum of the two vectors.</returns>
    public static Position<T> operator +(Position<T> a, Position<T> b)
    {
        return new Position<T>(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    /// <summary>
    /// Subtracts two vectors.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>The difference of the two vectors.</returns>
    public static Position<T> operator -(Position<T> a, Position<T> b)
    {
        return new Position<T>(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }


    /// <summary>
    /// Multiplies a vector by a scalar.
    /// </summary>
    /// <param name="a">The vector.</param>
    /// <param name="scalar">The scalar value.</param>
    /// <returns>The product of the vector and the scalar.</returns>
    public static Position<T> operator *(Position<T> a, T scalar)
    {
        return new Position<T>(a.X * scalar, a.Y * scalar, a.Z * scalar);
    }


    /// <summary>
    /// Divides a vector by a scalar.
    /// </summary>
    /// <param name="a">The vector.</param>
    /// <param name="scalar">The scalar value.</param>
    /// <returns>The quotient of the vector and the scalar.</returns>
    /// <exception cref="DivideByZeroException">Thrown when the scalar is zero.</exception>
    public static Position<T> operator /(Position<T> a, T scalar)
    {
        if (scalar == T.Zero)
            throw new DivideByZeroException("Cannot divide by zero.");
        return new Position<T>(a.X / scalar, a.Y / scalar, a.Z / scalar);
    }

    /// <summary>
    /// Calculates the dot product of two vectors.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>The dot product of the two vectors.</returns>
    public static T Dot(Position<T> a, Position<T> b)
    {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }

    /// <summary>
    /// Calculates the cross product of two vectors.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>The cross product of the two vectors.</returns>
    public static Position<T> Cross(Position<T> a, Position<T> b)
    {
        return new Position<T>(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X
        );
    }

    /// <summary>
    /// Converts a 3D position to a 1D index. This is useful for storing 3D data in a 1D array.
    /// </summary>
    /// <param name="width">The width of the 3D grid.</param>
    /// <param name="depth">The depth of the 3D grid.</param>
    /// <returns>The 1D index corresponding to the 3D position.</returns>
    /// <exception cref="ArgumentException">Thrown when width or depth is less than or equal to zero.</exception>
    public int ToIndex(int width, int depth)
    {
        if (width <= 0 || depth <= 0)
            throw new ArgumentException("Width and depth must be positive integers.");

        var area = width * depth;
        return Convert.ToInt32(X) + width * Convert.ToInt32(Z) + area * Convert.ToInt32(Y);
    }

    /// <summary>
    /// Converts a 3D position to a 1D index. This is useful for storing 3D data in a 1D array.
    /// </summary>
    /// <param name="sideLength">The side length of the 3D grid.</param>
    /// <returns>The 1D index corresponding to the 3D position.</returns>
    /// <exception cref="ArgumentException">Thrown when side length is less than or equal to zero.</exception>
    public int ToIndex(int sideLength)
    {
        return ToIndex(sideLength, sideLength);
    }

    /// <summary>
    /// Converts a 1D index to a 3D position. This is useful for converting linear array indices back to 3D coordinates.
    /// </summary>
    /// <param name="index">The 1D index to convert.</param>
    /// <param name="width">The width of the 3D grid.</param>
    /// <param name="depth">The depth of the 3D grid.</param>
    /// <returns>A <see cref="Position{T}"/> representing the 3D coordinates corresponding to the given 1D index.</returns>
    /// <exception cref="ArgumentException">Thrown when width or depth is less than or equal to zero.</exception>
    public static Position<T> FromIndex(int index, int width, int depth)
    {
        int area = width * depth;
        int y = index / area;
        int remaining = index % area;
        int z = remaining / width;
        int x = remaining % width;

        // Convert x, y, z to type T
        var convertedX = (T)Convert.ChangeType(x, typeof(T));
        var convertedY = (T)Convert.ChangeType(y, typeof(T));
        var convertedZ = (T)Convert.ChangeType(z, typeof(T));

        return new Position<T>(convertedX, convertedY, convertedZ);
    }

    /// <summary>
    /// Converts a 1D index to a 3D position. This is useful for converting linear array indices back to 3D coordinates.
    /// </summary>
    /// <param name="index">The 1D index to convert.</param>
    /// <param name="sideLength">The side length of the 3D grid.</param>
    /// <returns>A <see cref="Position{T}"/> representing the 3D coordinates corresponding to the given 1D index.</returns>
    /// <exception cref="ArgumentException">Thrown when side length is less than or equal to zero.</exception>
    public static Position<T> FromIndex(int index, int sideLength)
    {
        return FromIndex(index, sideLength, sideLength);
    }


    public override string ToString()
    {
        return $"({X}, {Y}, {Z})";
    }
}

public static class GenericMath
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Sqrt<T>(T value) where T : INumber<T>
    {
        // Avoid (T)(object) where possible
        if (typeof(T) == typeof(float))
            return (T)(object)MathF.Sqrt(Unsafe.As<T, float>(ref value));
        if (typeof(T) == typeof(double))
            return (T)(object)System.Math.Sqrt(Unsafe.As<T, double>(ref value));
        if (typeof(T) == typeof(int))
            return (T)(object)(int)System.Math.Sqrt(Unsafe.As<T, int>(ref value));

        throw new NotSupportedException($"Sqrt is not implemented for type {typeof(T)}.");
    }
}