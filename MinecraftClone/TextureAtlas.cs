using System.Runtime.InteropServices;
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

    public static unsafe void GenerateBlockPreviews(Texture2D texture2D)
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

        foreach (var (_, block) in Blocks.BlockList)
        {
            ReadOnlySpan<BlockFace> faces = [BlockFace.Top, BlockFace.Left, BlockFace.Front];

            var verticesList = new List<Vertex>();

            foreach (var blockFace in faces)
            {
                var uvCoordinates = Textures.GetUvCoordinatesForFace(block.Id, blockFace);

                verticesList.Add(new Vertex(Pos: new Vector3(0, 1, 1), TextCoord: uvCoordinates.topLeft, Color: Color.WHITE));
                verticesList.Add(new Vertex(Pos: new Vector3(0, 1, 0), TextCoord: uvCoordinates.topRight, Color: Color.WHITE));
                verticesList.Add(new Vertex(Pos: new Vector3(0, 0, 1), TextCoord: uvCoordinates.bottomLeft, Color: Color.WHITE));
                verticesList.Add(new Vertex(Pos: new Vector3(0, 0, 0), TextCoord: uvCoordinates.bottomRight, Color: Color.WHITE));
            }

            var mesh = new Mesh();
            mesh.vertexCount = verticesList.Count;
            mesh.triangleCount = verticesList.Count / 3;

            mesh.vertices = (float*)NativeMemory.AllocZeroed((UIntPtr)verticesList.Count * 3, sizeof(float));
            Span<float> vertices = new Span<float>(mesh.vertices, verticesList.Count * 3);

            mesh.texcoords = (float*)NativeMemory.AllocZeroed((UIntPtr)verticesList.Count * 2, sizeof(float));
            Span<float> texcoords = new Span<float>(mesh.texcoords, verticesList.Count * 2);

            for (var i = 0; i < verticesList.Count; i++)
            {
                var vertex = verticesList[i];
                vertices[i * 3] = vertex.Pos.X;
                vertices[i * 3 + 1] = vertex.Pos.Y;
                vertices[i * 3 + 2] = vertex.Pos.Z;

                texcoords[i * 2] = vertex.TextCoord.X;
                texcoords[i * 2 + 1] = vertex.TextCoord.Y;
            }

            var model = LoadModelFromMesh(mesh);
            model.materials[0].maps->texture = texture2D;

            DrawModel(model, Vector3.Zero, 1, Color.WHITE);
        }

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