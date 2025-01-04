using System.Numerics;
using System.Runtime.CompilerServices;

namespace VoxelSharp.Core;

public static class Math
{
    /// <summary>
    ///     Computes the Euclidean modulo operation, ensuring a non-negative result.
    /// </summary>
    /// <param name="value">The value to be divided.</param>
    /// <param name="mod">The divisor.</param>
    /// <returns>The remainder of the division, adjusted to be non-negative.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int EModulo(int value, int mod)
    {
        return mod switch
        {
            0 => throw new DivideByZeroException("Modulo by zero is undefined."),
            < 0 => throw new ArgumentException("Modulo must be positive.", nameof(mod)),
            _ => (value % mod + mod) % mod
        };
    }

    public static T Clamp<T>(T value, T min, T max) where T : INumber<T>
    {
        return value < min ? min : value > max ? max : value;
    }

    /// <summary>
    ///     Converts an angle in degrees to radians.
    /// </summary>
    /// <returns>The angle in radians.</returns>
    public static T ToRadians<T>(this T degrees) where T : INumber<T>
    {
        var pi = T.CreateChecked(System.Math.PI);
        var oneEighty = T.CreateChecked(180.0);
        return degrees * pi / oneEighty;
    }
}