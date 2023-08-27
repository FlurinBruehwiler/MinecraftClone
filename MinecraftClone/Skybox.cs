namespace RayLib3dTest;

public class Skybox : I3DDrawable
{
    private Model _skyBox;

    public Skybox()
    {
        Start();
    }

    private unsafe void Start()
    {
        var cube = GenMeshCube(1, 1, 1);
        _skyBox = LoadModelFromMesh(cube);

        var shader = LoadShader("resources/shaders/skybox.vs", "resources/shaders/skybox.fs");

        _skyBox.materials[0].shader = shader;

        int[] doGamma = { 0 };
        int[] vflipped = { 0 };
        int[] environmentMap = { (int)MaterialMapIndex.MATERIAL_MAP_CUBEMAP };

        SetShaderValue(shader, GetShaderLocation(shader, "environmentMap"),  environmentMap , ShaderUniformDataType.SHADER_UNIFORM_INT);
        SetShaderValue(shader, GetShaderLocation(shader, "doGamma"),  doGamma, ShaderUniformDataType.SHADER_UNIFORM_INT);
        SetShaderValue(shader, GetShaderLocation(shader, "vflipped"), vflipped, ShaderUniformDataType.SHADER_UNIFORM_INT);

        var img = LoadImage("Resources/skybox.png");
        _skyBox.materials[0].maps[(int)MaterialMapIndex.MATERIAL_MAP_CUBEMAP].texture = LoadTextureCubemap(img, CubemapLayout.CUBEMAP_LAYOUT_AUTO_DETECT);
        UnloadImage(img);
    }

    public void Draw3d()
    {
        rlDisableBackfaceCulling();
        rlDisableDepthMask();

        DrawModel(_skyBox, Vector3.Zero, 1, Color.WHITE);

        rlEnableBackfaceCulling();
        rlEnableDepthMask();
    }
}
