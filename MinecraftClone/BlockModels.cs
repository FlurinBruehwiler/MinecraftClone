using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RayLib3dTest;

public class JsonBlockModel
{
    public bool IsFullBlock = false;
    public List<JsonBlockElement> Elements;
}

public class JsonBlockElement
{
    public float[] From;
    public float[] To;
    public Dictionary<JsonBlockFaceDirection, JsonBlockFace> Faces;

    [JsonIgnore] public BlockDev BlockDev;
}

public enum JsonBlockFaceDirection
{
    None,
    North,
    East,
    South,
    West,
    Up,
    Down,
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

public static class Resources
{
    public static string GetPath(string fileNameAndExtension)
    {
        return Path.Combine(Path.Combine(Directory.GetParent(typeof(Resources).Assembly.FullName)!.FullName, "Resources", fileNameAndExtension));
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

        var path = Resources.GetPath(file);

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
            }
        }

        if (blockModel.Elements.Count == 1)
        {
            if (blockModel.Elements[0].BlockDev.From == new Vector3())
            {
                if (blockModel.Elements[0].BlockDev.To == new Vector3(16, 16, 16))
                {
                    blockModel.IsFullBlock = true;
                }
            }
        }

        Cache.Add(file, blockModel);

        return blockModel;
    }

    public static string EmptyModel = """
                                      {
                                      	"elements": []
                                      }
                                      """;
}