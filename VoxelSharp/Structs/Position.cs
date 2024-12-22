using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace VoxelSharp.Structs;

public readonly struct Position<T> where T : INumber<T>
{
    public T X { get; }
    public T Y { get; }
    public T Z { get; }


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


    // Magnitude of the vector
    public T Magnitude()
    {
        // Use a helper to compute the square root for numeric types.
        var sumOfSquares = X * X + Y * Y + Z * Z;
        return GenericMath.Sqrt(sumOfSquares);
    }

    // Normalize the vector (returns a new Position<T>)
    public Position<T> Normalize()
    {
        var magnitude = Magnitude();
        if (magnitude == T.Zero)
            throw new InvalidOperationException("Cannot normalize a zero vector.");

        return new Position<T>(X / magnitude, Y / magnitude, Z / magnitude);
    }

    // Add two vectors
    public static Position<T> operator +(Position<T> a, Position<T> b)
    {
        return new Position<T>(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    // Subtract two vectors
    public static Position<T> operator -(Position<T> a, Position<T> b)
    {
        return new Position<T>(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }

    // Scalar multiplication
    public static Position<T> operator *(Position<T> a, T scalar)
    {
        return new Position<T>(a.X * scalar, a.Y * scalar, a.Z * scalar);
    }

    // Scalar division
    public static Position<T> operator /(Position<T> a, T scalar)
    {
        if (scalar == T.Zero)
            throw new DivideByZeroException("Cannot divide by zero.");
        return new Position<T>(a.X / scalar, a.Y / scalar, a.Z / scalar);
    }

    // Dot product of two vectors
    public static T Dot(Position<T> a, Position<T> b)
    {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }

    // Cross product of two vectors (returns a new Position<T>)
    public static Position<T> Cross(Position<T> a, Position<T> b)
    {
        return new Position<T>(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X
        );
    }

    // Override ToString for better readability
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