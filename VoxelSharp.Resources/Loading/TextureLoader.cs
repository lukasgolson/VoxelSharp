using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace VoxelSharp.Resources.Loading;

public class TextureLoader
{
    
    public Texture2D LoadTextureFromImage(string path)
    {
        var image = Image.Load<Rgba32>(path);
        var texture = new Texture2D(image.Width, image.Height);
        
        var span = texture.Pixels.Span;
        
        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                var pixel = image[x, y];
                span[y * image.Width + x] = new Rgba(pixel.R, pixel.G, pixel.B, pixel.A);
            }
        }

        return texture;
    }
    
    public Texture2D SaveTextureToImage(Texture2D texture, string path)
    {
        var image = new Image<Rgba32>(texture.Width, texture.Height);
        
        var span = texture.Pixels.Span;
        
        for (var y = 0; y < texture.Height; y++)
        {
            for (var x = 0; x < texture.Width; x++)
            {
                var pixel = span[y * texture.Width + x];
                image[x, y] = new Rgba32(pixel.R, pixel.G, pixel.B, pixel.A);
            }
        }

        image.Save(path);
        
        return texture;
    }
    
    
    
}