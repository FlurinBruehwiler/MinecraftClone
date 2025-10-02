using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

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

    public static unsafe void GenerateBlockPreviews()
    {

        var camera = new Camera3D
        {
            Projection = CameraProjection.Orthographic,
            Position = new Vector3(-20, 30, -20),
            Up = new Vector3(0, 1, 0),
            FovY = 20,
        };

        camera.Target = new Vector3(0, 10, 0);

        Rlgl.SetTexture(CurrentWorld.TextureAtlas.Id);
        
        Raylib.BeginMode3D(camera);

        Rlgl.Begin(DrawMode.Quads);

        UvCoordinates coords;

        var white = Color.White;
        
        coords = Textures.GetUvCoordinatesForFace(Blocks.WoodenPlank.Id, BlockFace.Left);
        
        Rlgl.Color4ub(white.R, white.G, white.B, white.A);
        Rlgl.TexCoord2f(coords.bottomLeft.X, coords.bottomLeft.Y);
        Rlgl.Vertex3f(10, 10, 0); // Bottom Left
        Rlgl.TexCoord2f(coords.bottomRight.X, coords.bottomRight.Y);
        Rlgl.Vertex3f(10, 0, 0); // Bottom Right
        Rlgl.TexCoord2f(coords.topLeft.X, coords.topLeft.Y);
        Rlgl.Vertex3f(0, 0, 0); // Top Left
        Rlgl.TexCoord2f(coords.topRight.X, coords.topRight.Y);
        Rlgl.Vertex3f(0, 10, 0); // Top Right

        coords = Textures.GetUvCoordinatesForFace(Blocks.WoodenPlank.Id, BlockFace.Right);

        var red = Color.Red;
        Rlgl.Color4ub(red.R, red.G, red.B, red.A);
        Rlgl.TexCoord2f(coords.bottomLeft.X, coords.bottomLeft.Y);
        Rlgl.Vertex3f(0, 10, 10); // Bottom Left
        Rlgl.TexCoord2f(coords.bottomRight.X, coords.bottomRight.Y);
        Rlgl.Vertex3f(0, 10, 0); // Bottom Right
        Rlgl.TexCoord2f(coords.topLeft.X, coords.topLeft.Y);
        Rlgl.Vertex3f(0, 0, 0); // Top Left
        Rlgl.TexCoord2f(coords.topRight.X, coords.topRight.Y);
        Rlgl.Vertex3f(0, 0, 10); // Top Right

        coords = Textures.GetUvCoordinatesForFace(Blocks.WoodenPlank.Id, BlockFace.Top);

        var blue = Color.Blue;
        Rlgl.Color4ub(blue.R, blue.G, blue.B, blue.A);
        Rlgl.TexCoord2f(coords.bottomLeft.X, coords.bottomLeft.Y);
        Rlgl.Vertex3f(10, 10, 10); // Bottom Left
        Rlgl.TexCoord2f(coords.bottomRight.X, coords.bottomRight.Y);
        Rlgl.Vertex3f(10, 10, 0); // Bottom Right
        Rlgl.TexCoord2f(coords.topLeft.X, coords.topLeft.Y);
        Rlgl.Vertex3f(0, 10, 0); // Top Left
        Rlgl.TexCoord2f(coords.topRight.X, coords.topRight.Y);
        Rlgl.Vertex3f(0, 10, 10); // Top Right


        Rlgl.End();

        Raylib.EndMode3D();
        
        Rlgl.SetTexture(0);
    }
}