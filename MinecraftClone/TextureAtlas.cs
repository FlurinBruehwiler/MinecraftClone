using System.Runtime.InteropServices;

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

    public static unsafe void GenerateBlockPreviews(Texture2D texture2D)
    {
        var renderTarget = new RenderTexture2D
        {
            Texture = new Texture2D
            {
                Height = 1000,
                Width = 1000,
                Format = PixelFormat.CompressedDxt1Rgba
            }
        };
        BeginTextureMode(renderTarget);

        var camera = new Camera3D
        {
            Projection = CameraProjection.Orthographic
        };

        BeginMode3D(camera);

        foreach (var (_, block) in Blocks.BlockList)
        {
            ReadOnlySpan<BlockFace> faces = [BlockFace.Top, BlockFace.Left, BlockFace.Front];

            var verticesList = new List<Vertex>();

            foreach (var blockFace in faces)
            {
                var uvCoordinates = Textures.GetUvCoordinatesForFace(block.Id, blockFace);

                verticesList.Add(new Vertex(Pos: new Vector3(0, 1, 1), TextCoord: uvCoordinates.topLeft, Color: Color.White));
                verticesList.Add(new Vertex(Pos: new Vector3(0, 1, 0), TextCoord: uvCoordinates.topRight, Color: Color.White));
                verticesList.Add(new Vertex(Pos: new Vector3(0, 0, 1), TextCoord: uvCoordinates.bottomLeft, Color: Color.White));
                verticesList.Add(new Vertex(Pos: new Vector3(0, 0, 0), TextCoord: uvCoordinates.bottomRight, Color: Color.White));
            }

            var mesh = new Mesh();
            mesh.VertexCount = verticesList.Count;
            mesh.TriangleCount = verticesList.Count / 3;

            mesh.Vertices = (float*)NativeMemory.AllocZeroed((UIntPtr)verticesList.Count * 3, sizeof(float));
            Span<float> vertices = new Span<float>(mesh.Vertices, verticesList.Count * 3);

            mesh.TexCoords = (float*)NativeMemory.AllocZeroed((UIntPtr)verticesList.Count * 2, sizeof(float));
            Span<float> texcoords = new Span<float>(mesh.TexCoords, verticesList.Count * 2);

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
            model.Materials[0].Maps->Texture = texture2D;

            DrawModel(model, Vector3.Zero, 1, Color.White);
        }

        EndMode3D();

        EndTextureMode();
    }
}