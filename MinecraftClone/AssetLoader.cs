using System.Reflection;
using SourceGen;

namespace RayLib3dTest;

public static class AssetLoader
{
    private static readonly string[] foliageTextures =
    [
        Resources.Block.grass_block_side_overlay,
        Resources.Block.grass_block_top,
        Resources.Block.short_grass,
        Resources.Block.tall_grass_top,
        Resources.Block.tall_grass_bottom,
        Resources.Block.oak_leaves,
    ];

    public static void LoadAssets()
    {
        //Textures
        {
            var blockTextureDir = Path.Combine(Path.Combine(Directory.GetParent(typeof(AssetLoader).Assembly.FullName)!.FullName, "Resources", "block"));

            Textures.TextureList = [];

            int id = 0;
            foreach (var file in Directory.GetFiles(blockTextureDir))
            {
                var resourceIdentifier = file.Substring(file.LastIndexOf("Resources", StringComparison.InvariantCulture) + 10).Replace("\\", "/");

                Color color = Color.White;

                if (foliageTextures.Contains(resourceIdentifier))
                {
                    color = new Color(146, 193, 98);
                }
                else if(resourceIdentifier == Resources.Block.water)
                {
                    color = new Color(63, 118, 228);
                }

                Textures.TextureList.Add(resourceIdentifier, new TextureDefinition
                {
                    ColorOverlay = color,
                    Id = id++
                });
            }
        }

        //Blocks
        {
            Blocks.BlockList = typeof(Blocks)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(x => !x.IsInitOnly && x.FieldType == typeof(BlockDefinition))
                .Select(x => (BlockDefinition)x.GetValue(null)!)
                .ToDictionary(x => x.Id, x => x);

            foreach (var (_, block) in Blocks.BlockList)
            {
                block.ParsedModel = BlockModels.Get(block.Model);
                foreach (var (key, value) in block.ParsedModel.Textures)
                {
                    var val = value;

                    if (!val.EndsWith(".png"))
                        val += ".png";

                    block.Textures.TryAdd(key, val);
                }
            }
        }
    }
}