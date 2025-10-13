using System.Text.Json;
using System.Text.Json.Serialization;

namespace RayLib3dTest;

public class JsonBlockModel
{
    public List<JsonBlockElement> Elements;
    public Dictionary<string, string> Textures = [];
}

public enum Axis
{
    X,
    Y,
    Z
}

public class JsonBlockElementRotation
{
    public float Angle;
    public string Axis;
    public float[] Origin;
    public bool Rescale;

    [JsonIgnore] public Vector3 OriginVector;
}

public class JsonBlockElement
{
    public float[] From;
    public float[] To;
    public Dictionary<JsonBlockFaceDirection, JsonBlockFace> Faces;
    public JsonBlockElementRotation? Rotation;

    [JsonIgnore] public BlockDev BlockDev;
}

[Flags]
public enum JsonBlockFaceDirection : byte
{
    None = 0,
    North = 1,
    East = 2,
    South = 4,
    West = 8,
    Up = 16,
    Down = 32,
}

public class JsonBlockFace
{
    public float[] Uv;
    [JsonIgnore]
    public Vector4 UvVector;
    public string Texture;
    public string Cullface;
    [JsonIgnore]
    public JsonBlockFaceDirection CullfaceDirection;
}

public static class ResourcesExtension
{
    private static string resDir = Path.Combine(Directory.GetParent(typeof(ResourcesExtension).Assembly.FullName)!.FullName, "Resources");

    public static string GetResourcesPath(this string resourceId)
    {
        return Path.Combine(resDir, resourceId);
    }
}

public class BlockModels
{
    public static Dictionary<string, JsonBlockModel> Cache = [];

    public static JsonBlockModel Get(string? file)
    {
        file ??= "defaultBlockModel";

        if (Cache.TryGetValue(file, out var value))
            return value;

        string content = EmptyModel;

        var path = file.GetResourcesPath();

        if (File.Exists(path))
        {
            content = File.ReadAllText(path);
        }

        var options = new JsonSerializerOptions();
        options.IncludeFields = true;
        options.PropertyNameCaseInsensitive = true;
        var blockModel = JsonSerializer.Deserialize<JsonBlockModel>(content, options) ?? throw new Exception("Parsing failed");

        foreach (var element in blockModel.Elements)
        {
            element.BlockDev = new BlockDev
            {
                From = new Vector3(element.From[0], element.From[1], element.From[2]),
                To = new Vector3(element.To[0], element.To[1], element.To[2])
            };

            if (element.Rotation != null)
            {
                element.Rotation.OriginVector = new Vector3(element.Rotation.Origin[0], element.Rotation.Origin[1], element.Rotation.Origin[2]);
            }

            foreach (var (_, face) in element.Faces)
            {
                face.UvVector = new Vector4(face.Uv[0], face.Uv[1], face.Uv[2], face.Uv[3]);

                face.CullfaceDirection = face.Cullface switch
                {
                    "north" => JsonBlockFaceDirection.North,
                    "east" => JsonBlockFaceDirection.East,
                    "south" => JsonBlockFaceDirection.South,
                    "west" => JsonBlockFaceDirection.West,
                    "up" => JsonBlockFaceDirection.Up,
                    "down" => JsonBlockFaceDirection.Down,
                    _ => JsonBlockFaceDirection.None
                };

                face.Texture = face.Texture.Replace("#", "");
            }
        }

        // if (blockModel.Elements.Count == 1)
        // {
        //     if (blockModel.Elements[0].BlockDev.From == new Vector3())
        //     {
        //         if (blockModel.Elements[0].BlockDev.To == new Vector3(16, 16, 16))
        //         {
        //             blockModel.IsFullBlock = true;
        //         }
        //     }
        // }

        Cache.Add(file, blockModel);

        return blockModel;
    }

    public static string EmptyModel = """
                                      {
                                      	"elements": []
                                      }
                                      """;
}