namespace MinecraftClone;

public struct RotationInfo
{
    public Vector3 Origin;
    public Axis Axis;
    public float Angle;
}

public static class MeshGen
{
    public static float MapTo45(float input)
    {
        float period = 90f;
        float range = 45f;

        float mod = input % period;
        if (mod < 0) mod += period;

        if (mod > range)
            mod = period - mod;

        return mod;
    }

    public static void GenMeshForBlock(Block block, IntVector3 pos, JsonBlockFaceDirection surroundingBlocks, List<Vertex> verticesList)
    {
        var blockDefinition = Blocks.BlockList[block.BlockId];
        foreach (var element in blockDefinition.ParsedModel.Elements)
        {
            var mat = Matrix4x4.Identity;

            if (element.Rotation != null)
            {
                var rotationRadians = element.Rotation.Angle * Raylib.DEG2RAD;

                mat = Matrix4x4.CreateTranslation(- element.Rotation.OriginVector / 16);

                mat *= element.Rotation.Axis switch
                {
                    "x" => Matrix4x4.CreateRotationX(rotationRadians),
                    "y" => Matrix4x4.CreateRotationY(rotationRadians),
                    "z" => Matrix4x4.CreateRotationZ(rotationRadians),
                    _ => throw new ArgumentOutOfRangeException()
                };

                if (element.Rotation.Rescale)
                {
                    var scale = new Vector3(MathF.Sqrt(1 + MathF.Pow(MathF.Sin(MapTo45(element.Rotation.Angle) * Raylib.DEG2RAD), 2)));

                    if (element.Rotation.Axis == "x")
                        scale.X = 1;
                    else if (element.Rotation.Axis == "y")
                        scale.Y = 1;
                    else if (element.Rotation.Axis == "z")
                        scale.Z = 1;

                    mat *= Matrix4x4.CreateScale(scale);
                }

                mat *= Matrix4x4.CreateTranslation(element.Rotation.OriginVector / 16);
            }

            foreach (var (direction, face) in element.Faces)
            {
                var t = blockDefinition.Textures[face.Texture];
                var (uvs, color) = Textures.GetUvCoordinatesForTexture(t, face.UvVector);

                MeshBlockType meshBlockType = MeshBlockType.Normal;
                if (direction == JsonBlockFaceDirection.Up &&  block.BlockId == Blocks.Water.Id)
                {
                    meshBlockType = MeshBlockType.Water;
                }

                if (block.BlockId == Blocks.ShortGrass.Id)
                {

                }

                bool cullFace = face.CullfaceDirection != JsonBlockFaceDirection.None;

                //is solid block
                if (!cullFace || (surroundingBlocks & direction) == 0)
                {
                    AddQuadFor(pos, uvs, color, direction, element.BlockDev, verticesList, mat, meshBlockType);
                }
            }
        }
    }

    private static void AddQuadFor(IntVector3 block, UvCoordinates uvCoordinates, Color color, JsonBlockFaceDirection blockFace, BlockDev blockDev, List<Vertex> vertices, Matrix4x4 mat, MeshBlockType meshBlockType)
    {
        switch (blockFace)
        {
            case JsonBlockFaceDirection.West:
                AddVertices(block, blockDev.TopLeftFront(), blockDev.BottomLeftBack(), blockDev.TopLeftBack(), blockDev.BottomLeftFront(), vertices,
                    uvCoordinates, new Vector3(-1, 0, 0), color, mat, meshBlockType);
                break;
            case JsonBlockFaceDirection.East:
                AddVertices(block, blockDev.TopRightBack(), blockDev.BottomRightFront(), blockDev.TopRightFront(), blockDev.BottomRightBack(), vertices,
                    uvCoordinates, new Vector3(1, 0, 0), color, mat, meshBlockType);
                break;
            case JsonBlockFaceDirection.Down:
                AddVertices(block, blockDev.BottomRightFront(), blockDev.BottomLeftBack(), blockDev.BottomLeftFront(), blockDev.BottomRightBack(),
                    vertices, uvCoordinates, new Vector3(0, -1, 0), color, mat, meshBlockType);
                break;
            case JsonBlockFaceDirection.Up:
                AddVertices(block, blockDev.TopRightBack(), blockDev.TopLeftFront(), blockDev.TopLeftBack(), blockDev.TopRightFront(),
                    vertices, uvCoordinates, new Vector3(0, 1, 0), color, mat, meshBlockType);
                break;
            case JsonBlockFaceDirection.South:
                AddVertices(block, blockDev.TopLeftBack(), blockDev.BottomRightBack(), blockDev.TopRightBack(), blockDev.BottomLeftBack(), vertices,
                    uvCoordinates, new Vector3(0, 0, 1), color, mat, meshBlockType);
                break;
            case JsonBlockFaceDirection.North:
                AddVertices(block, blockDev.TopRightFront(), blockDev.BottomLeftFront(), blockDev.TopLeftFront(), blockDev.BottomRightFront(), vertices,
                    uvCoordinates, new Vector3(0, 0, -1), color, mat, meshBlockType);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }


    private static void AddVertices(IntVector3 block, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4,
        List<Vertex> vertices,
        UvCoordinates uvCoordinates, Vector3 normal, Color color, Matrix4x4 mat, MeshBlockType meshBlockType)
    {
        p1 = Vector3.Transform(p1, mat);
        p2 = Vector3.Transform(p2, mat);
        p3 = Vector3.Transform(p3, mat);
        p4 = Vector3.Transform(p4, mat);

        AddVertex(block, p1, vertices, uvCoordinates.topLeft(), normal, color, meshBlockType);
        AddVertex(block, p2, vertices, uvCoordinates.bottomRight(), normal, color, meshBlockType);
        AddVertex(block, p3, vertices, uvCoordinates.topRight(), normal, color, meshBlockType);

        AddVertex(block, p4, vertices, uvCoordinates.bottomLeft(), normal, color, meshBlockType);
        AddVertex(block, p2, vertices, uvCoordinates.bottomRight(), normal, color, meshBlockType);
        AddVertex(block, p1, vertices, uvCoordinates.topLeft(), normal, color, meshBlockType);
    }

    private static void AddVertex(IntVector3 blockPos, Vector3 corner, List<Vertex> vertices, Vector2 texCoord, Vector3 normal, Color color, MeshBlockType blockType)
    {
        vertices.Add(new Vertex(blockPos.ToVector3NonCenter() + corner, texCoord, color, normal, blockType));
    }
}

public enum MeshBlockType
{
    Normal = 0,
    Water = 1
}