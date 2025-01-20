
namespace VoxelSharp.Resources;

public class Texture2D(int width, int height)
{
    public int Width { get; private set; } = width;
    public int Height { get; private set; } = height;
    private Rgba[] PixelArray { get; set; } = new Rgba[width * height];

    public Memory<Rgba> Pixels => PixelArray; // Expose as Memory<Rgba>



    private void ValidateCoordinates(int x, int y)
    {
        const string message = "Pixel coordinates are out of bounds.";

        if (x < 0 || x >= Width)
            throw new ArgumentOutOfRangeException(nameof(x), message);
        if (y < 0 || y >= Height)
            throw new ArgumentOutOfRangeException(nameof(y), message);
    }

    // Set a pixel at (x, y)
    public void SetPixel(int x, int y, Rgba rgba)
    {
        ValidateCoordinates(x, y);

        PixelArray[y * Width + x] = rgba;
    }

    // Get a pixel at (x, y)
    public Rgba GetPixel(int x, int y)
    {
        ValidateCoordinates(x, y);
        return PixelArray[y * Width + x];
    }

    // Clear the texture with a single colour
    public void Clear(Rgba rgba)
    {
        PixelArray.AsSpan().Fill(rgba); // Efficiently fill the array
    }

}