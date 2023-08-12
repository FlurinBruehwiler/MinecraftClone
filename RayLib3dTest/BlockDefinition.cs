namespace RayLib3dTest;

public class BlockDefinition
{
    public string LeftTexture { get; set; } = null!;
    public string RightTexture { get; set; } = null!;
    public required string TopTexture { get; set; }
    public required string BottomTexture { get; set; }
    public string BackTexture { get; set; } = null!;
    public string FrontTexture { get; set; } = null!;
    public required string Name { get; set; }
    public required int ID { get; set; }

    public string SideTexture
    {
        set
        {
            LeftTexture = value;
            RightTexture = value;
            BackTexture = value;
            FrontTexture = value;
        }
    }
}