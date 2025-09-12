using SkiaSharp;

namespace RayLib3dTest;

public static class TextureAtlas
{
    public static void Create()
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

    public static void GenerateBlockPreviews(Texture2D texture2D)
    {
        var renderTarget = new RenderTexture2D
        {
            texture = new Texture2D
            {
                height = 1000,
                width = 1000,
                format = PixelFormat.PIXELFORMAT_COMPRESSED_DXT1_RGBA
            }
        };
        BeginTextureMode(renderTarget);

        var camera = new Camera3D
        {
            projection = CameraProjection.CAMERA_ORTHOGRAPHIC
        };

        BeginMode3D(camera);
        
        
        
        EndMode3D();
        
        EndTextureMode();

    }

    private static void Draw(SKCanvas canvas)
    {
        using var enumerator = Textures.TextureList.GetEnumerator();
        
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