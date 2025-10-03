namespace RayLib3dTest;

public static class TextureAtlas
{
    public static Texture2D Create()
    {
        RenderTexture2D renderTarget = LoadRenderTexture(160, 160);

        BeginTextureMode(renderTarget);

        using var enumerator = Textures.TextureList.GetEnumerator();

        ClearBackground(Color.Black);

        for (var y = 0; y < 10; y++)
        {
            for (var x = 0; x < 10; x++)
            {
                if (!enumerator.MoveNext())
                    goto end;

                var texture = enumerator.Current;
                var blockTexture = LoadTexture($"Resources/{texture.Key}.png");
                DrawTexturePro(blockTexture, new Rectangle(0, 0, blockTexture.Width, blockTexture.Height),
                    new Rectangle((x + 1) * 16, 160 - y * 16, 16, 16), new Vector2(0, 0), 180, Color.White);
            }
        }
        end:

        EndTextureMode();

        Image textureAtlas = LoadImageFromTexture(renderTarget.Texture);
        ExportImage(textureAtlas, "Resources/textureatlas.png");

        return renderTarget.Texture;
    }

    public static unsafe void GenerateBlockPreviews(Texture2D textureAtlas)
    {

        var camera = new Camera3D
        {
            Projection = CameraProjection.Orthographic,
            Position = new Vector3(-20, 30, -20),
            Up = new Vector3(0, 1, 0),
            FovY = 20,
        };

        camera.Target = new Vector3(0, 10, 0);

        RenderTexture2D renderTarget = LoadRenderTexture(160, 160);

        BeginTextureMode(renderTarget);
        
        BeginMode3D(camera);

        SetTexture(textureAtlas.Id);
        
        Begin(DrawMode.Quads);

        UvCoordinates coords;

        var white = Color.White;
        
        coords = Textures.GetUvCoordinatesForFace(Blocks.WoodenPlank.Id, BlockFace.Left);
        
        Color4ub(white.R, white.G, white.B, white.A);
        TexCoord2f(coords.topRight.X, coords.topRight.Y);
        Vertex3f(10, 10, 0); // Bottom Right 
        TexCoord2f(coords.bottomRight.X, coords.bottomRight.Y);
        Vertex3f(10, 0, 0); // Bottom Left 
        TexCoord2f(coords.bottomLeft.X, coords.bottomLeft.Y);
        Vertex3f(0, 0, 0); // Top Left
        TexCoord2f(coords.topLeft.X, coords.topLeft.Y);
        Vertex3f(0, 10, 0); // Top Right

        coords = Textures.GetUvCoordinatesForFace(Blocks.WoodenPlank.Id, BlockFace.Right);

        var red = Color.White;
        Color4ub(red.R, red.G, red.B, red.A);
        TexCoord2f(coords.topLeft.X, coords.topLeft.Y);
        Vertex3f(0, 10, 10); // Bottom Right 
        TexCoord2f(coords.topRight.X, coords.topRight.Y);
        Vertex3f(0, 10, 0); // Bottom Left 
        TexCoord2f(coords.bottomRight.X, coords.bottomRight.Y);
        Vertex3f(0, 0, 0); // Top Left
        TexCoord2f(coords.bottomLeft.X, coords.bottomLeft.Y);
        Vertex3f(0, 0, 10); // Top Right

        coords = Textures.GetUvCoordinatesForFace(Blocks.WoodenPlank.Id, BlockFace.Top);

        var blue = Color.White;
        Color4ub(blue.R, blue.G, blue.B, blue.A);
        TexCoord2f(coords.topLeft.X, coords.topLeft.Y);
        Vertex3f(10, 10, 10); // Bottom Right  
        TexCoord2f(coords.topRight.X, coords.topRight.Y);
        Vertex3f(10, 10, 0); // Bottom Left 
        TexCoord2f(coords.bottomRight.X, coords.bottomRight.Y);
        Vertex3f(0, 10, 0); // Top Left
        TexCoord2f(coords.bottomLeft.X, coords.bottomLeft.Y);
        Vertex3f(0, 10, 10); // Top Right

        End();

        EndMode3D();
        
        SetTexture(0);
        
        EndTextureMode();
        
        Image blockPreviewImage = LoadImageFromTexture(renderTarget.Texture);
        ExportImage(blockPreviewImage , "Resources/block_previews.png");
    }
}