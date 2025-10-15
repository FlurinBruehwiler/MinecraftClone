using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
using PixelFormat = Raylib_cs.PixelFormat;
using Shader = Raylib_cs.Shader;

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
                var dest = new Rectangle((x + 1) * 16, 160 - y * 16, 16, 16);

                if (!enumerator.MoveNext())
                {
                    Raylib.DrawRectanglePro(dest, Vector2.Zero, 180, Color.Red);
                }
                else
                {
                    var texture = enumerator.Current;
                    var blockTexture = Raylib.LoadTexture($"Resources/{texture.Key}");

                    Raylib.DrawTexturePro(blockTexture, new Rectangle(0, 0, blockTexture.Width, blockTexture.Height),
                        dest, new Vector2(0, 0), 180, Color.White);
                }
            }
        }

        Raylib.EndTextureMode();

        // Raylib.GenTextureMipmaps(ref renderTarget.Texture);

        var image = Raylib.LoadImageFromTexture(renderTarget.Texture);
        Game.Gl.BindTexture(GLEnum.Texture2D, renderTarget.Texture.Id);

        for (int i = 1; i <= 4; i++)
        {
            GenerateCustomMipMap(i, image);
        }

        Game.Gl.BindTexture(GLEnum.Texture2D, renderTarget.Texture.Id);
        Game.Gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMaxLevel, 4);

        Rlgl.TextureParameters(renderTarget.Texture.Id, Rlgl.TEXTURE_MIN_FILTER, Rlgl.TEXTURE_FILTER_MIP_NEAREST);

        return renderTarget.Texture;
    }

    private static unsafe void GenerateCustomMipMap(int level, Image image)
    {
        var factor = (int)Math.Pow(2, level);

        var width = image.Width / factor;
        var height = image.Height / factor;

        var ptr = NativeMemory.AllocZeroed((UIntPtr)(width * height * 4), sizeof(byte));
        var targetData = new Span<CustomColor>(ptr, width * height);

        var originalImageData = new Span<CustomColor>(image.Data, image.Width * image.Height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                targetData[x + y * width] = GetAverageColor(originalImageData, image.Width, x * factor, y * factor, factor);
            }
        }

        Game.Gl.TexImage2D(TextureTarget.Texture2D, level, InternalFormat.Rgba8, (uint)width, (uint)height, 0, Silk.NET.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
    }

    private static CustomColor GetAverageColor(Span<CustomColor> data, int width, int sampleX, int sampleY, int sampleSize)
    {
        int sumR = 0;
        int sumG = 0;
        int sumB = 0;
        int sumA = 0;

        int colorSamples = 0;
        int alphaSamples = 0;

        for (int i = 0; i < sampleSize; i++)
        {
            for (int j = 0; j < sampleSize; j++)
            {
                var x = sampleX + i;
                var y = sampleY + j;
                var color = data[x + y * width];

                if (color.A == 255)
                {
                    sumR += color.R;
                    sumG += color.G;
                    sumB += color.B;
                    colorSamples++;
                }

                sumA += color.A;
                alphaSamples++;
            }
        }

        return new CustomColor
        {
            R = colorSamples == 0 ? (byte)0 : (byte)(sumR / colorSamples),
            G = colorSamples == 0 ? (byte)0 : (byte)(sumG / colorSamples),
            B = colorSamples == 0 ? (byte)0 : (byte)(sumB / colorSamples),
            A = alphaSamples == 0 ? (byte)0 : (byte)(sumA / alphaSamples),
        };
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

        var dir = Vector3.Normalize(new Vector3(-1.0f, -1.0f, -1.0f));
        Span<float> direction = [dir.X, dir.Y, dir.Z];

        Raylib.SetShaderValue(shader, Game.ShaderLocSunDirection, direction, ShaderUniformDataType.Vec3);


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

[StructLayout(LayoutKind.Sequential)]
public struct CustomColor
{
    public byte R;
    public byte G;
    public byte B;
    public byte A;
}