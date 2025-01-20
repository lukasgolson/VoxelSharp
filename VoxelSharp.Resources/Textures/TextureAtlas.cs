using VoxelSharp.Resources.Resources;

namespace VoxelSharp.Resources;

public class TextureAtlas
{
    // This class is used to store textures in a single texture.
    // The textures are stored in a grid.
    // The grid is divided into cells.
    // Each cell contains a texture.
    // The textures are stored in a single texture.
    
    
    public Texture2D Texture { get; private set; }
    
    public int CellWidth { get; private set; }
    public int CellHeight { get; private set; }
    
    public int CellCountX { get; private set; }
    public int CellCountY { get; private set; }

    private readonly Dictionary<Address, (int X, int Y)> TexturePosition;


    public void Build(Resource<Texture2D>[] texture2Ds)
    {
        // Calculate the number of cells in the x and y directions
        CellCountX = (int)Math.Ceiling(Math.Sqrt(texture2Ds.Length));
        CellCountY = (int)Math.Ceiling((float)texture2Ds.Length / CellCountX);

        // Ensure that the texture is a power of 2
        var width = (int)Math.Pow(2, (int)Math.Ceiling(Math.Log2(CellCountX * CellWidth)));
        var height = (int)Math.Pow(2, (int)Math.Ceiling(Math.Log2(CellCountY * CellHeight)));

        // Create a new texture
        Texture = new Texture2D(width, height);

        // Clear the texture with a transparent colour
        Texture.Clear(new Rgba(0, 0, 0, 0));

        // Get the span of the atlas texture
        var atlasSpan = Texture.Pixels.Span;

        for (var i = 0; i < texture2Ds.Length; i++)
        {
            Texture2D inTexture = texture2Ds[i];
            var address = texture2Ds[i].Address;

            var cellX = i % CellCountX;
            var cellY = i / CellCountX;

            TexturePosition.Add(address, (cellX, cellY));

            // Calculate the position in the atlas
            var atlasStartIndex = (cellY * CellHeight * width) + (cellX * CellWidth);

            // Get the span of the input texture
            var inSpan = inTexture.Pixels.Span;

            // Copy the pixels into the atlas
            var lengthToCopy = inTexture.Width * inTexture.Height;

            // Only copy if the dimensions match
            if (lengthToCopy <= atlasSpan.Length - atlasStartIndex)
            {
                inSpan.Slice(0, lengthToCopy).CopyTo(atlasSpan.Slice(atlasStartIndex));
            }
        }
    }

}