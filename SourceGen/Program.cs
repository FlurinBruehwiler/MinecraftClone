using System.Runtime.CompilerServices;
using System.Text;

var filePath = GetCurrentFile();
var minecraftCloneDir = Path.GetFullPath(Path.Combine(Directory.GetParent(filePath).FullName, "../MinecraftClone"));

var builder = new StringBuilder();
builder.AppendLine("namespace SourceGen;");
int indentation = 0;

GenerateCodeForDirectory(Path.Combine(minecraftCloneDir, "Resources"), builder, indentation);

File.WriteAllText(Path.Combine(minecraftCloneDir, "Resources.cs"), builder.ToString());

void GenerateCodeForDirectory(string dir, StringBuilder sb, int i)
{
    var dirName = Path.GetFileName(dir);

    sb.AppendLine(i, $"public static class {dirName.ToUpperFirst()}");
    sb.AppendLine(i, "{");

    i++;
    foreach (var entry in Directory.GetFiles(dir))
    {
        var fileName = Path.GetFileNameWithoutExtension(entry);

        var stringLiteral = entry.Substring(entry.LastIndexOf("Resources", StringComparison.InvariantCulture) + 10).Replace("\\", "/");

        sb.AppendLine(i, $"public const string {fileName} = \"{stringLiteral}\";");
    }

    sb.AppendLine();

    foreach (var directory in Directory.GetDirectories(dir))
    {
        GenerateCodeForDirectory(directory, sb, indentation + 1);
    }

    i--;

    sb.AppendLine(i, "}");
}



string GetCurrentFile([CallerFilePath] string filePath = "")
{
    return filePath;
}

public static class Extensions
{
    public static string ToUpperFirst(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        return $"{char.ToUpper(str[0])}{str.Substring(1)}";
    }

    public static void AppendLine(this StringBuilder sb, int indentation, string str)
    {
        sb.AppendLine(new string(' ', 4 * indentation) + str);
    }
}