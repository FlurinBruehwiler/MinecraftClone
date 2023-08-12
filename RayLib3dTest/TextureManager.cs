using SkiaSharp;

namespace RayLib3dTest;

public class TextureManager
{
    private readonly Textures _textures;

    public TextureManager(Textures textures)
    {
        _textures = textures;
    }

    public void Merge()
    {
        var info = new SKImageInfo(16 * 10, 16 * 10);
        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Black);
        Draw(canvas);
        using var img = surface.Snapshot();
        using var data = img.Encode(SKEncodedImageFormat.Png, 80);
        using var stream = File.OpenWrite("Resources/textureatlas.png");
        data.SaveTo(stream);
    }

    private void Draw(SKCanvas canvas)
    {
        using var enumerator = _textures.TextureList.GetEnumerator();
        
        for (var y = 0; y < 10; y++)
        {
            for (var x = 0; x < 10; x++)
            {
                if (!enumerator.MoveNext())
                    return;
                
                var texture = enumerator.Current;
                var img = SKImage.FromEncodedData($"Resources/{texture.Key}.png");
                var bitmap = SKBitmap.FromImage(img);
                canvas.DrawBitmap(bitmap, x * 16, y * 16);
            }  
        }
    }
}