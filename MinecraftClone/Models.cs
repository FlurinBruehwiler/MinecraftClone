using System.Text.Json;
using System.Text.Json.Serialization;

namespace RayLib3dTest;

public class JemFile
{
    public string Texture = string.Empty;
    public float[] TextureSize = [];
    public JemModel[] Models = [];

    [JsonIgnore] public Vector2 TextureSizeV;
    [JsonIgnore] public Texture2D Texture2D;
}

public class JemModel
{
    public string Id = string.Empty;
    public float[] Translate = [];
    public JemBox[] Boxes = [];

    [JsonIgnore] public Vector3 TranslateV;
}

public class JemBox
{
    public float[] Coordinates = [];
    public float[] TextureOffset = [];

    [JsonIgnore] public Vector3 Position;
    [JsonIgnore] public Vector3 Size;
    [JsonIgnore] public Vector2 TextureOffsetV;
}

public static class Models
{
    public static JemFile LoadModel(string resourceId)
    {
        var str = File.ReadAllText(resourceId.GetResourcesPath());
        var options = new JsonSerializerOptions();
        options.IncludeFields = true;
        options.PropertyNameCaseInsensitive = true;
        var jemFile = JsonSerializer.Deserialize<JemFile>(str, options) ?? throw new Exception("Parsing failed");

        if (jemFile.TextureSize.Length != 2)
            throw new Exception();
        jemFile.TextureSizeV = new Vector2(jemFile.TextureSize[0], jemFile.TextureSize[1]);

        if (jemFile.Texture != "")
            jemFile.Texture2D = Raylib.LoadTexture($"Resources/{jemFile.Texture}");

        foreach (var model in jemFile.Models)
        {
            if (model.Translate.Length is not (0 or 3))
                throw new Exception();

            model.TranslateV = new Vector3(model.Translate[0], model.Translate[1], model.Translate[2]);

            foreach (var box in model.Boxes)
            {
                if (box.Coordinates.Length != 6)
                    throw new Exception();

                if (box.TextureOffset.Length != 2)
                    throw new Exception();

                box.Position = new Vector3(box.Coordinates[0], box.Coordinates[1], box.Coordinates[2]);
                box.Size = new Vector3(box.Coordinates[3], box.Coordinates[4], box.Coordinates[5]);
                box.TextureOffsetV = new Vector2(box.TextureOffset[0], box.TextureOffset[1]);
            }
        }

        return jemFile;
    }

    public static void RenderModel(JemFile jemFile, Vector3 pos, Vector3 direction, Hitbox hitBox)
    {
        pos.Y -= hitBox.GetSize().Y;

        Rlgl.PushMatrix();

        Rlgl.Translatef(pos.X, pos.Y, pos.Z);
        Rlgl.Rotatef(MathF.Atan2(direction.X, direction.Z) * Raylib.RAD2DEG , 0, 1, 0);


        foreach (var model in jemFile.Models)
        {
            foreach (var box in model.Boxes)
            {
                var posOffset = (model.TranslateV * 0) + (box.Position / 16);
                var size = box.Size / 16;

                var p = posOffset + size / 2;
                DrawTexturedCube(p, new Vector3(size.X * 16, size.Y * 16, size.Z * 16), box.TextureOffsetV, jemFile.TextureSizeV, jemFile.Texture2D);
                // TexCoord2f(source.x / width, source.y / height);
            }
        }

        Rlgl.PopMatrix();

    }

    private static void DrawTexturedCube(Vector3 position, Vector3 size, Vector2 textureOffset, Vector2 textureSize, Texture2D texture)
    {
        float width = size.X;
        float height = size.Y;
        float length = size.Z;

        float x = 0.0f;
        float y = 0.0f;
        float z = 0.0f;


        Rlgl.PushMatrix();

        Rlgl.Translatef(position.X, position.Y, position.Z);

        Rlgl.Scalef(1.0f / 16, 1.0f / 16, 1.0f / 16);

        Rlgl.SetTexture(texture.Id);
        
        Rlgl.Begin(DrawMode.Quads);

        var white = Color.White;
        Rlgl.Color4ub(white.R, white.G, white.B, white.A);

        // Front face
        {
            var origin = new Vector2(textureOffset.X + length, textureOffset.Y + length);
            var (bottomLeft, topLeft, topRight, bottomRight) = GetTextureCoordinates(origin, textureSize, width, height);

            Rlgl.Normal3f(0.0f, 0.0f, 1.0f);

            Rlgl.TexCoord2f(bottomLeft.X, bottomLeft.Y);
            Rlgl.Vertex3f(x - width / 2, y - height / 2, z + length / 2); // Bottom Left

            Rlgl.TexCoord2f(bottomRight.X, bottomRight.Y);
            Rlgl.Vertex3f(x + width / 2, y - height / 2, z + length / 2); // Bottom Right

            Rlgl.TexCoord2f(topRight.X, topRight.Y);
            Rlgl.Vertex3f(x + width / 2, y + height / 2, z + length / 2); // Top Right

            Rlgl.TexCoord2f(topLeft.X, topLeft.Y);
            Rlgl.Vertex3f(x - width / 2, y + height / 2, z + length / 2); // Top Left
        }

        // Back face
        {
            var origin = new Vector2(textureOffset.X + 2 * length + width, textureOffset.Y + length);
            var (bottomLeft, topLeft, topRight, bottomRight) = GetTextureCoordinates(origin, textureSize, width, height);

            Rlgl.Normal3f(0.0f, 0.0f, -1.0f);

            Rlgl.TexCoord2f(bottomLeft.X, bottomLeft.Y);
            Rlgl.Vertex3f(x - width / 2, y - height / 2, z - length / 2); // Bottom Left

            Rlgl.TexCoord2f(topLeft.X, topLeft.Y);
            Rlgl.Vertex3f(x - width / 2, y + height / 2, z - length / 2); // Top Left

            Rlgl.TexCoord2f(topRight.X, topRight.Y);
            Rlgl.Vertex3f(x + width / 2, y + height / 2, z - length / 2); // Top Right

            Rlgl.TexCoord2f(bottomRight.X, bottomRight.Y);
            Rlgl.Vertex3f(x + width / 2, y - height / 2, z - length / 2); // Bottom Right

        }

        // Top face
        {
            var origin = new Vector2(textureOffset.X + length, textureOffset.Y);
            var (bottomLeft, topLeft, topRight, bottomRight) = GetTextureCoordinates(origin, textureSize, width, length);

            Rlgl.Normal3f(0.0f, 1.0f, 0.0f);

            Rlgl.TexCoord2f(topLeft.X, topLeft.Y);
            Rlgl.Vertex3f(x - width / 2, y + height / 2, z - length / 2); // Top Left

            Rlgl.TexCoord2f(bottomLeft.X, bottomLeft.Y);
            Rlgl.Vertex3f(x - width / 2, y + height / 2, z + length / 2); // Bottom Left

            Rlgl.TexCoord2f(bottomRight.X, bottomRight.Y);
            Rlgl.Vertex3f(x + width / 2, y + height / 2, z + length / 2); // Bottom Right

            Rlgl.TexCoord2f(topRight.X, topRight.Y);
            Rlgl.Vertex3f(x + width / 2, y + height / 2, z - length / 2); // Top Right
        }

        // Bottom face
        {
            var origin = new Vector2(textureOffset.X + 2 * length, textureOffset.Y);
            var (bottomLeft, topLeft, topRight, bottomRight) = GetTextureCoordinates(origin, textureSize, width, length);

            Rlgl.Normal3f(0.0f, -1.0f, 0.0f);

            Rlgl.TexCoord2f(topLeft.X, topLeft.Y);
            Rlgl.Vertex3f(x - width / 2, y - height / 2, z - length / 2); // Top Left

            Rlgl.TexCoord2f(topRight.X, topRight.Y);
            Rlgl.Vertex3f(x + width / 2, y - height / 2, z - length / 2); // Top Right

            Rlgl.TexCoord2f(bottomRight.X, bottomRight.Y);
            Rlgl.Vertex3f(x + width / 2, y - height / 2, z + length / 2); // Bottom Right

            Rlgl.TexCoord2f(bottomLeft.X, bottomLeft.Y);
            Rlgl.Vertex3f(x - width / 2, y - height / 2, z + length / 2); // Bottom Left
        }

        // Right face
        {
            var origin = new Vector2(textureOffset.X + length + width , textureOffset.Y + length);
            var (bottomLeft, topLeft, topRight, bottomRight) = GetTextureCoordinates(origin, textureSize, length, height);

            Rlgl.Normal3f(1.0f, 0.0f, 0.0f);

            Rlgl.TexCoord2f(bottomRight.X, bottomRight.Y);
            Rlgl.Vertex3f(x + width / 2, y - height / 2, z - length / 2); // Bottom Right

            Rlgl.TexCoord2f(topRight.X, topRight.Y);
            Rlgl.Vertex3f(x + width / 2, y + height / 2, z - length / 2); // Top Right

            Rlgl.TexCoord2f(topLeft.X, topLeft.Y);
            Rlgl.Vertex3f(x + width / 2, y + height / 2, z + length / 2); // Top Left

            Rlgl.TexCoord2f(bottomLeft.X, bottomRight.Y);
            Rlgl.Vertex3f(x + width / 2, y - height / 2, z + length / 2); // Bottom Left
        }

        // Left face
        {
            var origin = new Vector2(textureOffset.X, textureOffset.Y + length);
            var (bottomLeft, topLeft, topRight, bottomRight) = GetTextureCoordinates(origin, textureSize, length, height);

            Rlgl.Normal3f(-1.0f, 0.0f, 0.0f);

            Rlgl.TexCoord2f(topRight.X, topRight.Y);
            Rlgl.Vertex3f(x - width / 2, y + height / 2, z + length / 2); // Top Left

            Rlgl.TexCoord2f(topLeft.X, topLeft.Y);
            Rlgl.Vertex3f(x - width / 2, y + height / 2, z - length / 2); // Top Right

            Rlgl.TexCoord2f(bottomLeft.X, bottomLeft.Y);
            Rlgl.Vertex3f(x - width / 2, y - height / 2, z - length / 2); // Bottom Right

            Rlgl.TexCoord2f(bottomRight.X, bottomRight.Y);
            Rlgl.Vertex3f(x - width / 2, y - height / 2, z + length / 2); // Bottom Left

        }

        Rlgl.End();
        Rlgl.SetTexture(0);
        Rlgl.PopMatrix();
    }

    public static (Vector2 bottomLeft, Vector2 topLeft, Vector2 topRight, Vector2 bottomRight) GetTextureCoordinates(Vector2 topLeft, Vector2 textureSize, float width, float height)
    {
        return (
            bottomLeft: new Vector2(topLeft.X / textureSize.X, (topLeft.Y + height) / textureSize.Y),
            topLeft: new Vector2(topLeft.X / textureSize.X, topLeft.Y / textureSize.Y),
            topRight: new Vector2((topLeft.X + width) / textureSize.X, topLeft.Y / textureSize.Y),
            bottomRight: new Vector2((topLeft.X + width) / textureSize.X, (topLeft.Y + height) / textureSize.Y));
    }
}