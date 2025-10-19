namespace MinecraftClone;

public class BlockDefinition
{
    public Dictionary<string, string> Textures = [];
    public string Model;
    public required string Name;
    public required ushort Id;
    public JsonBlockModel ParsedModel;
    public bool IsTransparent;

    public static Dictionary<string, string> ConstructBlockTextures(
        string all = "",
        string sides = "",
        string top = "",
        string bottom = "",
        string north = "",
        string west = "",
        string east = "",
        string south = ""
    )
    {
        var dict = new Dictionary<string, string>();

        dict.Add("top", all);
        dict.Add("bottom", all);
        dict.Add("north", all);
        dict.Add("west", all);
        dict.Add("east", all);
        dict.Add("south", all);

        if (sides != "")
        {
            dict["north"] = sides;
            dict["west"] = sides;
            dict["east"] = sides;
            dict["south"] = sides;
        }

        if (top != "") dict["top"] = top;
        if (bottom != "") dict["bottom"] = bottom;
        if (north != "") dict["north"] = north;
        if (west != "") dict["west"] = west;
        if (east != "") dict["east"] = east;
        if (south != "") dict["south"] = south;

        return dict;
    }
}