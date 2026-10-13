using static System.Math;

namespace VoxelSharp.Core.Helpers;

/// <summary>
/// Generates Perlin noise.
/// Based on Ken Perlin's improved noise reference implementation.
/// </summary>
public class PerlinNoiseGenerator
{
    // Permutation array - A shuffled array of 0-255, duplicated for convenience.
    private int[] _p;

    /// <summary>
    /// Initializes a new instance of the PerlinNoiseGenerator class.
    /// Uses a default random seed.
    /// </summary>
    public PerlinNoiseGenerator() : this(new Random())
    {
    }

    /// <summary>
    /// Initializes a new instance of the PerlinNoiseGenerator class with a specific seed.
    /// </summary>
    /// <param name="seed">The random seed to use for permutation.</param>
    public PerlinNoiseGenerator(int seed) : this(new Random(seed))
    {
    }

    /// <summary>
    /// Initializes a new instance of the PerlinNoiseGenerator class with a Random object.
    /// </summary>
    /// <param name="random">The Random object to use for permutation.</param>
    public PerlinNoiseGenerator(Random random)
    {
        // Initialize base permutation array with values 0-255
        var permutation = Enumerable.Range(0, 256).ToArray();

        // Shuffle the array using the provided Random object
        for (var i = permutation.Length - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (permutation[i], permutation[j]) = (permutation[j], permutation[i]);
        }

        // Duplicate the permutation array to avoid modulo operations in the noise function
        _p = new int[512];
        for (var i = 0; i < 256; i++)
        {
            _p[i] = permutation[i];
            _p[i + 256] = permutation[i];
        }
    }

    /// <summary>
    /// Generates 3D Perlin noise for the given coordinates.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <param name="z">The z-coordinate.</param>
    /// <param name="scale"></param>
    /// <param name="offsetX"></param>
    /// <param name="offsetY"></param>
    /// <param name="offsetZ"></param>
    /// <returns>The noise value between -1.0 and 1.0.</returns>
    public double Generate(double x, double y, double z, double scale = 0.1, double offsetX = 25, double offsetY = 50, double offsetZ = 75)
    {
        // Apply offset and scale to the input coordinates
        double scaledX = (x + offsetX) * scale;
        double scaledY = (y + offsetY) * scale;
        double scaledZ = (z + offsetZ) * scale;

        // Find the unit cube that contains the point
        int X = (int)Floor(scaledX) & 255;
        int Y = (int)Floor(scaledY) & 255;
        int Z = (int)Floor(scaledZ) & 255;

        // Find relative x, y, z of point in cube
        scaledX -= Floor(scaledX);
        scaledY -= Floor(scaledY);
        scaledZ -= Floor(scaledZ);

        // Compute fade curves for each of x, y, z
        double u = Fade(scaledX);
        double v = Fade(scaledY);
        double w = Fade(scaledZ);

        // Hash coordinates of the 8 cube corners
        int A = _p[X] + Y;
        int AA = _p[A] + Z;
        int AB = _p[A + 1] + Z;
        int B = _p[X + 1] + Y;
        int BA = _p[B] + Z;
        int BB = _p[B + 1] + Z;

        // And add blended results from 8 corners of cube
        double result = Lerp(w, Lerp(v, Lerp(u, Grad(_p[AA], scaledX, scaledY, scaledZ),
                    Grad(_p[BA], scaledX - 1, scaledY, scaledZ)),
                Lerp(u, Grad(_p[AB], scaledX, scaledY - 1, scaledZ),
                    Grad(_p[BB], scaledX - 1, scaledY - 1, scaledZ))),
            Lerp(v, Lerp(u, Grad(_p[AA + 1], scaledX, scaledY, scaledZ - 1),
                    Grad(_p[BA + 1], scaledX - 1, scaledY, scaledZ - 1)),
                Lerp(u, Grad(_p[AB + 1], scaledX, scaledY - 1, scaledZ - 1),
                    Grad(_p[BB + 1], scaledX - 1, scaledY - 1, scaledZ - 1))));

        // The result is typically in the range [-1, 1].
        // It might slightly exceed this range in some implementations, but for most
        // practical purposes, it's within this range.
        return result;
    }

    /// <summary>
    /// Fade function as defined by Ken Perlin.
    /// 6t^5 - 15t^4 + 10t^3
    /// </summary>
    private static double Fade(double t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    /// <summary>
    /// Linear interpolation between a and b based on t.
    /// </summary>
    private static double Lerp(double t, double a, double b)
    {
        return a + t * (b - a);
    }

    /// <summary>
    /// Calculates the dot product of a pseudo-random gradient vector and the vector
    /// from the input point to the cube corner.
    /// </summary>
    /// <param name="hash">The hash value for the corner.</param>
    /// <param name="x">The x-coordinate relative to the corner.</param>
    /// <param name="y">The y-coordinate relative to the corner.</param>
    /// <param name="z">The z-coordinate relative to the corner.</param>
    /// <returns>The dot product.</returns>
    private static double Grad(int hash, double x, double y, double z)
    {
        // The hash determines the gradient direction.
        // This is a common way to map the hash to one of the 12 gradient vectors.
        int h = hash & 15;
        double u = h < 8 ? x : y;
        double v = h < 4 ? y : h == 12 || h == 14 ? x : z;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }
}