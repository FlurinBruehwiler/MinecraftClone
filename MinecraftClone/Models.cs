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
    public static JemFile LoadModel()
    {
        var str = File.ReadAllText("Resources/creeper.jem");
        var options = new JsonSerializerOptions();
        options.IncludeFields = true;
        options.PropertyNameCaseInsensitive = true;
        var jemFile = JsonSerializer.Deserialize<JemFile>(str, options) ?? throw new Exception("Parsing failed");

        if (jemFile.TextureSize.Length != 2)
            throw new Exception();
        jemFile.TextureSizeV = new Vector2(jemFile.TextureSize[0], jemFile.TextureSize[1]);

        if (jemFile.Texture != "")
            jemFile.Texture2D = LoadTexture($"Resources/{jemFile.Texture}.png");

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
            }
        }

        return jemFile;
    }

    public static void RenderModel(JemFile jemFile, Vector3 pos, Hitbox hitBox)
    {
        pos.Y -= hitBox.GetSize().Y;

        
        DrawBillboard2(Player.C3d, jemFile.Texture2D, pos, 5, Color.WHITE);
        
        foreach (var model in jemFile.Models)
        {
            foreach (var box in model.Boxes)
            {
                var posOffset = (model.TranslateV * 0) + (box.Position / 16);
                var size = box.Size / 16;

                var p = pos + posOffset + size / 2;
                DrawTexturedCube(p, size.X * 16, size.Y * 16, size.Z * 16, box.TextureOffsetV, jemFile.TextureSizeV, jemFile.Texture2D);
                // rlTexCoord2f(source.x / width, source.y / height);
            }
        }
    }

    public static void DrawBillboard2(Camera3D camera, Texture2D texture, Vector3 position, float scale, Color tint)
    {
        Rectangle source = new Rectangle( 0.0f, 0.0f, (float)texture.width, (float)texture.height );

        DrawBillboardRec2(camera, texture, source, position, new Vector2( scale* Math.Abs((float)source.width/source.height), scale ), tint);
    }

// Draw a billboard (part of a texture defined by a rectangle)
    public static void DrawBillboardRec2(Camera3D camera, Texture2D texture, Rectangle source, Vector3 position, Vector2 size, Color tint)
    {
        // NOTE: Billboard locked on axis-Y
        Vector3 up = new Vector3 ( 0.0f, 1.0f, 0.0f );

        DrawBillboardPro2(camera, texture, source, position, up, size, Vector2Scale(size, 0.5f), 0.0f, tint);
    }


    public static void DrawBillboardPro2(Camera3D camera, Texture2D texture, Rectangle source, Vector3 position, Vector3 up, Vector2 size, Vector2 origin, float rotation, Color tint)
    {
        // Compute the up vector and the right vector
        Matrix4x4 matView = MatrixLookAt(camera.position, camera.target, camera.up);
        Vector3 right = new Vector3(matView.M11, matView.M21, matView.M31);
        right = Vector3Scale(right, size.X);
        up = Vector3Scale(up, size.Y);

        // Flip the content of the billboard while maintaining the counterclockwise edge rendering order
        if (size.X < 0.0f)
        {
            source.x += size.X;
            source.width *= -1.0f;
            right = Vector3Negate(right);
            origin.X *= -1.0f;
        }

        if (size.Y < 0.0f)
        {
            source.y += size.Y;
            source.height *= -1.0f;
            up = Vector3Negate(up);
            origin.Y *= -1.0f;
        }

        // Draw the texture region described by source on the following rectangle in 3D space:
        //
        //                size.x          <--.
        //  3 ^---------------------------+ 2 \ rotation
        //    |                           |   /
        //    |                           |
        //    |   origin.x   position     |
        // up |..............             | size.y
        //    |             .             |
        //    |             . origin.y    |
        //    |             .             |
        //  0 +---------------------------> 1
        //                right
        Vector3 forward = new Vector3();
        if (rotation != 0.0) forward = Vector3CrossProduct(right, up);

        Vector3 origin3D = Vector3Add(Vector3Scale(Vector3Normalize(right), origin.X), Vector3Scale(Vector3Normalize(up), origin.Y));

        Vector3[] points = new Vector3[4];
        points[0] = Vector3Zero();
        points[1] = right;
        points[2] = Vector3Add(up, right);
        points[3] = up;

        for (int i = 0; i < 4; i++)
        {
            points[i] = Vector3Subtract(points[i], origin3D);
            if (rotation != 0.0) points[i] = Vector3RotateByAxisAngle(points[i], forward, rotation * DEG2RAD);
            points[i] = Vector3Add(points[i], position);
        }

        Vector2[] texcoords = new Vector2[4];
        texcoords[0] = new Vector2((float)source.x / texture.width, (float)(source.y + source.height) / texture.height);
        texcoords[1] = new Vector2((float)(source.x + source.width) / texture.width, (float)(source.y + source.height) / texture.height);
        texcoords[2] = new Vector2((float)(source.x + source.width) / texture.width, (float)source.y / texture.height);
        texcoords[3] = new Vector2((float)source.x / texture.width, (float)source.y / texture.height);

        rlSetTexture(texture.id);
        rlBegin(DrawMode.QUADS);

        rlColor4ub(tint.r, tint.g, tint.b, tint.a);
        for (int i = 0; i < 4; i++)
        {
            rlTexCoord2f(texcoords[i].X, texcoords[i].Y);
            rlVertex3f(points[i].X, points[i].Y, points[i].Z);
        }

        rlEnd();
        rlSetTexture(0);
    }

    private static void DrawTexturedCube(Vector3 position, float width, float height, float length, Vector2 textureOffset, Vector2 textureSize, Texture2D texture)
    {
        float x = 0.0f;
        float y = 0.0f;
        float z = 0.0f;


        rlPushMatrix();
        // NOTE: Transformation is applied in inverse order (scale -> rotate -> translate)
        rlTranslatef(position.X, position.Y, position.Z);

        
        //rlRotatef(45, 0, 1, 0);
        rlScalef(1.0f / 16, 1.0f / 16, 1.0f / 16); // NOTE: Vertices are directly scaled on definition

        rlSetTexture(texture.id);
        
        rlBegin(DrawMode.TRIANGLES);

        var white = Color.WHITE;
        rlColor4ub(white.r, white.g, white.b, white.a);

        // Front face
        rlNormal3f(0.0f, 0.0f, 1.0f);
        rlTexCoord2f((textureOffset.X + length) / textureSize.X, (textureOffset.Y + length + height) / textureSize.Y);
        rlVertex3f(x - width / 2, y - height / 2, z + length / 2); // Bottom Left
        rlTexCoord2f((textureOffset.X + length + width) / textureSize.X, (textureOffset.Y + length + height) / textureSize.Y);
        rlVertex3f(x + width / 2, y - height / 2, z + length / 2); // Bottom Right
        rlTexCoord2f((textureOffset.X + length) / textureSize.X, (textureOffset.Y + length) / textureSize.Y);
        rlVertex3f(x - width / 2, y + height / 2, z + length / 2); // Top Left

        rlTexCoord2f((textureOffset.X + length + width) / textureSize.X, (textureOffset.Y + length) / textureSize.Y);
        rlVertex3f(x + width / 2, y + height / 2, z + length / 2); // Top Right
        
        // rlTexCoord2f((textureOffset.X + length) / textureSize.X, (textureOffset.Y + length) / textureSize.Y);
        // rlVertex3f(x - width / 2, y + height / 2, z + length / 2); // Top Left
        // rlTexCoord2f((textureOffset.X + length + width) / textureSize.X, (textureOffset.Y + length + height) / textureSize.Y);
        // rlVertex3f(x + width / 2, y - height / 2, z + length / 2); // Bottom Right

        rlEnd();
        rlSetTexture(0);
        rlPopMatrix();


        //
        // // Back face
        // rlNormal3f(0.0f, 0.0f, -1.0f);
        // rlTexCoord2f(0, 0);
        // rlVertex3f(x - width / 2, y - height / 2, z - length / 2); // Bottom Left
        // rlTexCoord2f(0, 1);
        // rlVertex3f(x - width / 2, y + height / 2, z - length / 2); // Top Left
        // rlTexCoord2f(1, 0);
        // rlVertex3f(x + width / 2, y - height / 2, z - length / 2); // Bottom Right
        //
        // rlTexCoord2f(1, 1);
        // rlVertex3f(x + width / 2, y + height / 2, z - length / 2); // Top Right
        // rlTexCoord2f(1, 0);
        // rlVertex3f(x + width / 2, y - height / 2, z - length / 2); // Bottom Right
        // rlTexCoord2f(0, 1);
        // rlVertex3f(x - width / 2, y + height / 2, z - length / 2); // Top Left
        //
        // // Top face
        // rlNormal3f(0.0f, 1.0f, 0.0f);
        // rlTexCoord2f((textureOffset.X + length) / textureSize.X, (textureOffset.Y + length + height) / textureSize.Y);
        // rlVertex3f(x - width / 2, y + height / 2, z - length / 2); // Top Left
        // rlTexCoord2f((textureOffset.X + length) / textureSize.X, (textureOffset.Y + length + height) / textureSize.Y);
        // rlVertex3f(x - width / 2, y + height / 2, z + length / 2); // Bottom Left
        // rlTexCoord2f((textureOffset.X + length) / textureSize.X, (textureOffset.Y + length + height) / textureSize.Y);
        // rlVertex3f(x + width / 2, y + height / 2, z + length / 2); // Bottom Right
        //
        // rlTexCoord2f((textureOffset.X + length) / textureSize.X, (textureOffset.Y + length + height) / textureSize.Y);
        // rlVertex3f(x + width / 2, y + height / 2, z - length / 2); // Top Right
        // rlTexCoord2f((textureOffset.X + length) / textureSize.X, (textureOffset.Y + length + height) / textureSize.Y);
        // rlVertex3f(x - width / 2, y + height / 2, z - length / 2); // Top Left
        // rlTexCoord2f((textureOffset.X + length) / textureSize.X, (textureOffset.Y + length + height) / textureSize.Y);
        // rlVertex3f(x + width / 2, y + height / 2, z + length / 2); // Bottom Right
        //
        // // Bottom face
        // rlNormal3f(0.0f, -1.0f, 0.0f);
        // rlTexCoord2f((textureOffset.X + length) / textureSize.X, (textureOffset.Y + length + height) / textureSize.Y);
        // rlVertex3f(x - width / 2, y - height / 2, z - length / 2); // Top Left
        // rlTexCoord2f((textureOffset.X + length) / textureSize.X, (textureOffset.Y + length + height) / textureSize.Y);
        // rlVertex3f(x + width / 2, y - height / 2, z + length / 2); // Bottom Right
        // rlTexCoord2f((textureOffset.X + length) / textureSize.X, (textureOffset.Y + length + height) / textureSize.Y);
        // rlVertex3f(x - width / 2, y - height / 2, z + length / 2); // Bottom Left
        //
        // rlTexCoord2f((textureOffset.X + length) / textureSize.X, (textureOffset.Y + length + height) / textureSize.Y);
        // rlVertex3f(x + width / 2, y - height / 2, z - length / 2); // Top Right
        // rlTexCoord2f((textureOffset.X + length) / textureSize.X, (textureOffset.Y + length + height) / textureSize.Y);
        // rlVertex3f(x + width / 2, y - height / 2, z + length / 2); // Bottom Right
        // rlTexCoord2f((textureOffset.X + length) / textureSize.X, (textureOffset.Y + length + height) / textureSize.Y);
        // rlVertex3f(x - width / 2, y - height / 2, z - length / 2); // Top Left
        //
        // // Right face
        // rlNormal3f(1.0f, 0.0f, 0.0f);
        // rlTexCoord2f((textureOffset.X + length) / textureSize.X, (textureOffset.Y + length + height) / textureSize.Y);
        // rlVertex3f(x + width / 2, y - height / 2, z - length / 2); // Bottom Right
        // rlTexCoord2f((textureOffset.X + length) / textureSize.X, (textureOffset.Y + length + height) / textureSize.Y);
        // rlVertex3f(x + width / 2, y + height / 2, z - length / 2); // Top Right
        // rlTexCoord2f((textureOffset.X + length) / textureSize.X, (textureOffset.Y + length + height) / textureSize.Y);
        // rlVertex3f(x + width / 2, y + height / 2, z + length / 2); // Top Left
        //
        // rlTexCoord2f((textureOffset.X + length) / textureSize.X, (textureOffset.Y + length + height) / textureSize.Y);
        // rlVertex3f(x + width / 2, y - height / 2, z + length / 2); // Bottom Left
        // rlTexCoord2f((textureOffset.X + length) / textureSize.X, (textureOffset.Y + length + height) / textureSize.Y);
        // rlVertex3f(x + width / 2, y - height / 2, z - length / 2); // Bottom Right
        // rlTexCoord2f((textureOffset.X + length) / textureSize.X, (textureOffset.Y + length + height) / textureSize.Y);
        // rlVertex3f(x + width / 2, y + height / 2, z + length / 2); // Top Left
        //
        // // Left face
        // rlNormal3f(-1.0f, 0.0f, 0.0f);
        // rlTexCoord2f((textureOffset.X + length) / textureSize.X, (textureOffset.Y + length + height) / textureSize.Y);
        // rlVertex3f(x - width / 2, y - height / 2, z - length / 2); // Bottom Right
        // rlTexCoord2f((textureOffset.X + length) / textureSize.X, (textureOffset.Y + length + height) / textureSize.Y);
        // rlVertex3f(x - width / 2, y + height / 2, z + length / 2); // Top Left
        // rlTexCoord2f((textureOffset.X + length) / textureSize.X, (textureOffset.Y + length + height) / textureSize.Y);
        // rlVertex3f(x - width / 2, y + height / 2, z - length / 2); // Top Right
        //
        // rlTexCoord2f((textureOffset.X + length) / textureSize.X, (textureOffset.Y + length + height) / textureSize.Y);
        // rlVertex3f(x - width / 2, y - height / 2, z + length / 2); // Bottom Left
        // rlTexCoord2f((textureOffset.X + length) / textureSize.X, (textureOffset.Y + length + height) / textureSize.Y);
        // rlVertex3f(x - width / 2, y + height / 2, z + length / 2); // Top Left
        // rlTexCoord2f((textureOffset.X + length) / textureSize.X, (textureOffset.Y + length + height) / textureSize.Y);
        // rlVertex3f(x - width / 2, y - height / 2, z - length / 2); // Bottom Right


    }
}