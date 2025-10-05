namespace RayLib3dTest;

public static class TextureAtlas
{
    public static Texture2D Create()
    {
        RenderTexture2D renderTarget = Raylib.LoadRenderTexture(160, 160);

        Raylib.BeginTextureMode(renderTarget);

        using var enumerator = Textures.TextureList.GetEnumerator();

        Raylib.ClearBackground(Color.Black);

        for (var y = 0; y < 10; y++)
        {
            for (var x = 0; x < 10; x++)
            {
                if (!enumerator.MoveNext())
                    goto end;

                var texture = enumerator.Current;
                var blockTexture = Raylib.LoadTexture($"Resources/{texture.Key}.png");
                Raylib.DrawTexturePro(blockTexture, new Rectangle(0, 0, blockTexture.Width, blockTexture.Height),
                    new Rectangle((x + 1) * 16, 160 - y * 16, 16, 16), new Vector2(0, 0), 180, Color.White);
            }
        }
        end:

        Raylib.EndTextureMode();

        Image textureAtlas = Raylib.LoadImageFromTexture(renderTarget.Texture);
        Raylib.ExportImage(textureAtlas, "Resources/textureatlas.png");

        return renderTarget.Texture;
    }

    public static Texture2D GenerateBlockPreviews(Texture2D textureAtlas)
    {

        var camera = new Camera3D
        {
            Projection = CameraProjection.Orthographic,
            Position = new Vector3(0, 0, -800),
            Up = new Vector3(0, 1, 0),
            FovY = 1000,
        };

        camera.Target = new Vector3(0, 0, 0);

        RenderTexture2D renderTarget = Raylib.LoadRenderTexture(1000, 1000);

        Raylib.BeginTextureMode(renderTarget);

        Raylib.ClearBackground(new Color(0, 0, 0, 0));

        Raylib.BeginMode3D(camera);

        Rlgl.SetTexture(textureAtlas.Id);

        Rlgl.Begin(DrawMode.Quads);

        int idx = 0;
        foreach (var (_, block) in Blocks.BlockList)
        {
            if(block.Id == 0)
                continue;

            DrawSingleBlockPreview(block, (new Vector2(-(idx % 10), idx / 10) * 100) + new Vector2(+450, -450));
            idx++;
        }

        Raylib.EndMode3D();

        Rlgl.SetTexture(0);

        Raylib.EndDrawing();

        Raylib.EndTextureMode();

        Image blockPreviewImage = Raylib.LoadImageFromTexture(renderTarget.Texture);
        Raylib.ExportImage(blockPreviewImage , "Resources/block_previews.png");
        return renderTarget.Texture;
    }

    private static void DrawSingleBlockPreview(BlockDefinition blockDefinition, Vector2 position)
    {
        //todo make working with non full blocks
        return;

        // Rlgl.PushMatrix();
        //
        //
        // Rlgl.Translatef(position.X, position.Y, 0);
        // const float scale = -5;
        // Rlgl.Scalef(scale, scale, scale);
        //
        // Rlgl.Rotatef(-30 , 1, 0, 0);
        // Rlgl.Rotatef(-45 , 0, 1, 0);
        //
        // Rlgl.Translatef(-5, -5, -5);
        //
        // var coords = Textures.GetUvCoordinatesForFace(blockDefinition.Id, BlockFace.Left);
        //
        // var white = Color.White;
        //
        // Rlgl.Color4ub(white.R, white.G, white.B, white.A);
        // Rlgl.TexCoord2f(coords.topRight.X, coords.topRight.Y);
        // Rlgl.Vertex3f(10, 10, 0); // Bottom Right
        // Rlgl.TexCoord2f(coords.bottomRight.X, coords.bottomRight.Y);
        // Rlgl.Vertex3f(10, 0, 0); // Bottom Left
        // Rlgl.TexCoord2f(coords.bottomLeft.X, coords.bottomLeft.Y);
        // Rlgl.Vertex3f(0, 0, 0); // Top Left
        // Rlgl.TexCoord2f(coords.topLeft.X, coords.topLeft.Y);
        // Rlgl.Vertex3f(0, 10, 0); // Top Right
        //
        // coords = Textures.GetUvCoordinatesForFace(blockDefinition.Id, BlockFace.Right);
        //
        // var red = Color.White;
        // Rlgl.Color4ub(red.R, red.G, red.B, red.A);
        // Rlgl.TexCoord2f(coords.topLeft.X, coords.topLeft.Y);
        // Rlgl.Vertex3f(0, 10, 10); // Bottom Right
        // Rlgl.TexCoord2f(coords.topRight.X, coords.topRight.Y);
        // Rlgl.Vertex3f(0, 10, 0); // Bottom Left
        // Rlgl.TexCoord2f(coords.bottomRight.X, coords.bottomRight.Y);
        // Rlgl.Vertex3f(0, 0, 0); // Top Left
        // Rlgl.TexCoord2f(coords.bottomLeft.X, coords.bottomLeft.Y);
        // Rlgl.Vertex3f(0, 0, 10); // Top Right
        //
        // coords = Textures.GetUvCoordinatesForFace(blockDefinition.Id, BlockFace.Top);
        //
        // var blue = Color.White;
        // Rlgl.Color4ub(blue.R, blue.G, blue.B, blue.A);
        // Rlgl.TexCoord2f(coords.topLeft.X, coords.topLeft.Y);
        // Rlgl.Vertex3f(10, 10, 10); // Bottom Right
        // Rlgl.TexCoord2f(coords.topRight.X, coords.topRight.Y);
        // Rlgl.Vertex3f(10, 10, 0); // Bottom Left
        // Rlgl.TexCoord2f(coords.bottomRight.X, coords.bottomRight.Y);
        // Rlgl.Vertex3f(0, 10, 0); // Top Left
        // Rlgl.TexCoord2f(coords.bottomLeft.X, coords.bottomLeft.Y);
        // Rlgl.Vertex3f(0, 10, 10); // Top Right
        //
        // Rlgl.PopMatrix();
        //
        // Rlgl.End();
    }
}