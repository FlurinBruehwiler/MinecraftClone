namespace RayLib3dTest;

public static class TextureAtlas
{
    public static Texture2D Create()
    {
        RenderTexture2D renderTarget = Raylib.LoadRenderTexture(160, 160);

        Raylib.BeginTextureMode(renderTarget);

        using var enumerator = Textures.TextureList.GetEnumerator();

        Raylib.ClearBackground(new Color(0, 0, 0, 0));

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

    public static Texture2D GenerateBlockPreviews(Texture2D textureAtlas, Shader shader)
    {

        var camera = new Camera3D
        {
            Projection = CameraProjection.Orthographic,
            Position = new Vector3(0, 0, -400),
            Up = new Vector3(0, 1, 0),
            FovY = 1000,
        };

        camera.Target = new Vector3(0, 0, 0);

        RenderTexture2D renderTarget = Raylib.LoadRenderTexture(1000, 1000);

        Raylib.BeginTextureMode(renderTarget);

        Raylib.ClearBackground(new Color(0, 0, 0, 0));

        Raylib.BeginMode3D(camera);
        Raylib.BeginShaderMode(shader);
        Rlgl.Begin(DrawMode.Triangles);
        Rlgl.SetTexture(textureAtlas.Id);

        int idx = 0;
        foreach (var (_, block) in Blocks.BlockList)
        {
            if(block.Id == 0)
                continue;

            DrawSingleBlockPreview(block, (new Vector2(-(idx % 10), idx / 10) * 100) + new Vector2(+450, -450));
            idx++;
        }

        Rlgl.End();

        Raylib.EndShaderMode();
        Raylib.EndMode3D();
        Raylib.EndDrawing();

        Raylib.EndTextureMode();
        Rlgl.SetTexture(0);

        Image blockPreviewImage = Raylib.LoadImageFromTexture(renderTarget.Texture);
        Raylib.ExportImage(blockPreviewImage , "Resources/block_previews.png");
        return renderTarget.Texture;
    }

    private static void DrawSingleBlockPreview(BlockDefinition blockDefinition, Vector2 position)
    {
        var list = new List<Vertex>();

        MeshGen.GenMeshForBlock(new Block
        {
            BlockId = blockDefinition.Id
        }, new IntVector3(), JsonBlockFaceDirection.None, list);

        Rlgl.PushMatrix();

        Rlgl.Translatef(position.X, position.Y, 0);
        const float scale = 50;
        Rlgl.Scalef(scale, scale, scale);

        Rlgl.Rotatef(-30 , 1, 0, 0);
        Rlgl.Rotatef(-45 , 0, 1, 0);

        Rlgl.Translatef(-0.5f, -0.5f, -0.5f);

        foreach (var vertex in list)
        {
            Rlgl.Color4ub(vertex.Color.R, vertex.Color.G, vertex.Color.B, vertex.Color.A);
            Rlgl.TexCoord2f(vertex.TextCoord.X, vertex.TextCoord.Y);
            Rlgl.Vertex3f(vertex.Pos.X, vertex.Pos.Y, vertex.Pos.Z);
        }

        Rlgl.PopMatrix();

    }
}