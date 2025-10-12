namespace RayLib3dTest;

public static class MeshGen
{
    public static void GenMeshForBlock(Block block, IntVector3 pos, JsonBlockFaceDirection surroundingBlocks, List<Vertex> verticesList)
    {
        var blockDefinition = Blocks.BlockList[block.BlockId];
        foreach (var element in blockDefinition.ParsedModel.Elements)
        {
            foreach (var (direction, face) in element.Faces)
            {
                var t = blockDefinition.Textures[face.Texture];
                var (uvs, color) = Textures.GetUvCoordinatesForTexture(t, face.UvVector);

                AddQuadFor(pos, uvs, color, direction, element.BlockDev, verticesList, surroundingBlocks, face.CullfaceDirection != JsonBlockFaceDirection.None);
            }
        }
    }

    private static void AddQuadFor(IntVector3 block, UvCoordinates uvCoordinates, Color color, JsonBlockFaceDirection blockFace, BlockDev blockDev, List<Vertex> vertices, JsonBlockFaceDirection surroundingBlocks, bool cullFace)
    {
        //is solid block
        if (cullFace && (surroundingBlocks & blockFace) != 0)
            return;

        switch (blockFace)
        {
            case JsonBlockFaceDirection.West:
                AddVertices(block, blockDev.TopLeftFront(), blockDev.BottomLeftBack(), blockDev.TopLeftBack(), blockDev.BottomLeftFront(), vertices,
                    uvCoordinates, new Vector3(-1, 0, 0), color);
                break;
            case JsonBlockFaceDirection.East:
                AddVertices(block, blockDev.TopRightBack(), blockDev.BottomRightFront(), blockDev.TopRightFront(), blockDev.BottomRightBack(), vertices,
                    uvCoordinates, new Vector3(1, 0, 0), color);
                break;
            case JsonBlockFaceDirection.Down:
                AddVertices(block, blockDev.BottomRightFront(), blockDev.BottomLeftBack(), blockDev.BottomLeftFront(), blockDev.BottomRightBack(),
                    vertices, uvCoordinates, new Vector3(0, -1, 0), color);
                break;
            case JsonBlockFaceDirection.Up:
                AddVertices(block, blockDev.TopRightBack(), blockDev.TopLeftFront(), blockDev.TopLeftBack(), blockDev.TopRightFront(),
                    vertices, uvCoordinates, new Vector3(0, 1, 0), color);
                break;
            case JsonBlockFaceDirection.South:
                AddVertices(block, blockDev.TopLeftBack(), blockDev.BottomRightBack(), blockDev.TopRightBack(), blockDev.BottomLeftBack(), vertices,
                    uvCoordinates, new Vector3(0, 0, 1), color);
                break;
            case JsonBlockFaceDirection.North:
                AddVertices(block, blockDev.TopRightFront(), blockDev.BottomLeftFront(), blockDev.TopLeftFront(), blockDev.BottomRightFront(), vertices,
                    uvCoordinates, new Vector3(0, 0, -1), color);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }


    private static void AddVertices(IntVector3 block, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4,
        List<Vertex> vertices,
        UvCoordinates uvCoordinates, Vector3 normal, Color color)
    {
        AddVertex(block, p1, vertices, uvCoordinates.topLeft, normal, color);
        AddVertex(block, p2, vertices, uvCoordinates.bottomRight, normal, color);
        AddVertex(block, p3, vertices, uvCoordinates.topRight, normal, color);

        AddVertex(block, p4, vertices, uvCoordinates.bottomLeft, normal, color);
        AddVertex(block, p2, vertices, uvCoordinates.bottomRight, normal, color);
        AddVertex(block, p1, vertices, uvCoordinates.topLeft, normal, color);
    }

    private static void AddVertex(IntVector3 blockPos, Vector3 corner, List<Vertex> vertices, Vector2 texCoord, Vector3 normal, Color color)
    {
        vertices.Add(new Vertex(blockPos.ToVector3NonCenter() + corner, texCoord, color, normal));
    }
}