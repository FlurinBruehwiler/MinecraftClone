using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
using Shader = Raylib_cs.Shader;

namespace MinecraftClone;

public static class TextureAtlas
{
    public static unsafe void ChangeAnimationFrame(Texture2D textureAtlas, TextureDefinition textureDefinition)
    {
        var image = new ReadOnlySpan<byte>(textureDefinition.Image.Data, textureDefinition.Image.Width * textureDefinition.Image.Height * 4);

        var offset = textureDefinition.Image.Width * textureDefinition.Image.Width * 4 * textureDefinition.CurrentAnimationFrame;
        var frameSlice = image.Slice(offset);

        var subImage = new Image
        {
            Width = textureDefinition.Image.Width,
            Height = textureDefinition.Image.Width,
            Format = textureDefinition.Image.Format,
            Data = (byte*)textureDefinition.Image.Data + offset,
            Mipmaps = 1
        };

        Raylib.UpdateTextureRec(textureAtlas, textureDefinition.TextureAtlasRec, frameSlice);

        Game.Gl.BindTexture(GLEnum.Texture2D, textureAtlas.Id);

        for (int m = 1; m <= 4; m++)
        {
            GenerateCustomMipMap(m, subImage, textureAtlas, textureDefinition.TextureAtlasRec);
        }
    }

    public static Texture2D Create()
    {
        RenderTexture2D renderTarget = Raylib.LoadRenderTexture(160, 160);

        Raylib.BeginTextureMode(renderTarget);

        Raylib.ClearBackground(new Color(0, 0, 0, 0));

        int i = 0;

        foreach (var (key, texture) in Textures.TextureList)
        {
            var img = Raylib.LoadImage($"Resources/{key}");
            texture.Image = img;

            var blockTexture = Raylib.LoadTextureFromImage(img);

            var x = i % 10;
            var y = i / 10;
            i++;

            var dest = new Rectangle((x + 1) * 16, 160 - y * 16, 16, 16);

            texture.TextureAtlasRec = new Rectangle
            {
                Width = 16,
                Height = 16,
                X = x * 16,
                Y = y * 16
            };

            var src = new Rectangle(0, 0, blockTexture.Width, blockTexture.Width); //both here are with in case it is an animation

            texture.UvCoordinates = new UvCoordinates(new Vector2((float)x / 10, (float)y / 10), new Vector2(1.0f / 10, 1.0f/10));

            Raylib.DrawTexturePro(blockTexture, src, dest, new Vector2(0, 0), 180, Color.White);

            // Raylib.UnloadTexture(blockTexture);
        }

        Raylib.EndTextureMode();

        // Raylib.GenTextureMipmaps(ref renderTarget.Texture);

        var image = Raylib.LoadImageFromTexture(renderTarget.Texture);

        Raylib.ExportImage(image, "TextureAtlas.png");

        Game.Gl.BindTexture(GLEnum.Texture2D, renderTarget.Texture.Id);

        for (int m = 1; m <= 4; m++)
        {
            GenerateCustomMipMap(m, image, renderTarget.Texture, new Rectangle(0, 0, 160, 160));
        }

        Game.Gl.BindTexture(GLEnum.Texture2D, renderTarget.Texture.Id);
        Game.Gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMaxLevel, 4);

        Rlgl.TextureParameters(renderTarget.Texture.Id, Rlgl.TEXTURE_MIN_FILTER, Rlgl.TEXTURE_FILTER_MIP_NEAREST);

        return renderTarget.Texture;
    }

    private static unsafe void GenerateCustomMipMap(int level, Image image, Texture2D textureToUpdate, Rectangle targetRec)
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

        if (targetRec.X == 0 && targetRec.Y == 0 && (int)targetRec.Width == textureToUpdate.Width && (int)targetRec.Height == textureToUpdate.Height)
        {
            Game.Gl.TexImage2D(TextureTarget.Texture2D, level, InternalFormat.Rgba8, (uint)width, (uint)height, 0, Silk.NET.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
        }
        else
        {
            Game.Gl.TexSubImage2D(GLEnum.Texture2D, level, (int)targetRec.X / factor, (int)targetRec.Y / factor, (uint)(targetRec.Width / factor), (uint)(targetRec.Height / factor), GLEnum.Rgba, GLEnum.UnsignedByte, ptr);
            var err = Game.Gl.GetError();
            if (err != GLEnum.None)
                throw new Exception(err.ToString());
        }

        NativeMemory.Free(ptr);
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

                if (color.A > 0)
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

        Raylib.SetShaderValue(shader, Game.ShaderLocAlphaCutout, [0.5f], ShaderUniformDataType.Float);
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