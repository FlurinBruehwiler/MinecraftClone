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
                var uvs = Textures.GetUvCoordinatesForTexture(t, face.UvVector);

                AddQuadFor(pos, uvs, direction, element.BlockDev, verticesList, surroundingBlocks, face.CullfaceDirection != JsonBlockFaceDirection.None);
            }
        }
    }

    private static void AddQuadFor(IntVector3 block, UvCoordinates uvCoordinates, JsonBlockFaceDirection blockFace, BlockDev blockDev, List<Vertex> vertices, JsonBlockFaceDirection surroundingBlocks, bool cullFace)
    {
        //is solid block
        if (cullFace && (surroundingBlocks & blockFace) != 0)
            return;

        switch (blockFace)
        {
            case JsonBlockFaceDirection.West:
                AddBetterVertices(block, blockDev.TopLeftFront(), blockDev.BottomLeftBack(), blockDev.TopLeftBack(), blockDev.BottomLeftFront(), vertices,
                    uvCoordinates);
                break;
            case JsonBlockFaceDirection.East:
                AddBetterVertices(block, blockDev.TopRightBack(), blockDev.BottomRightFront(), blockDev.TopRightFront(), blockDev.BottomRightBack(), vertices,
                    uvCoordinates);
                break;
            case JsonBlockFaceDirection.Down:
                AddBetterVertices(block, blockDev.BottomRightFront(), blockDev.BottomLeftBack(), blockDev.BottomLeftFront(), blockDev.BottomRightBack(),
                    vertices, uvCoordinates);
                break;
            case JsonBlockFaceDirection.Up:
                AddBetterVertices(block, blockDev.TopRightBack(), blockDev.TopLeftFront(), blockDev.TopLeftBack(), blockDev.TopRightFront(), vertices, uvCoordinates);
                break;
            case JsonBlockFaceDirection.South:
                AddBetterVertices(block, blockDev.TopLeftBack(), blockDev.BottomRightBack(), blockDev.TopRightBack(), blockDev.BottomLeftBack(), vertices,
                    uvCoordinates);
                break;
            case JsonBlockFaceDirection.North:
                AddBetterVertices(block, blockDev.TopRightFront(), blockDev.BottomLeftFront(), blockDev.TopLeftFront(), blockDev.BottomRightFront(), vertices,
                    uvCoordinates);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }


    private static void AddBetterVertices(IntVector3 block, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4,
        List<Vertex> vertices,
        UvCoordinates uvCoordinates)
    {
        AddVertices(block, p1, vertices, uvCoordinates.topLeft);
        AddVertices(block, p2, vertices, uvCoordinates.bottomRight);
        AddVertices(block, p3, vertices, uvCoordinates.topRight);

        AddVertices(block, p4, vertices, uvCoordinates.bottomLeft);
        AddVertices(block, p2, vertices, uvCoordinates.bottomRight);
        AddVertices(block, p1, vertices, uvCoordinates.topLeft);
    }

    private static void AddVertices(IntVector3 blockPos, Vector3 corner, List<Vertex> vertices, Vector2 texCoord)
    {
        vertices.Add(new Vertex(blockPos.ToVector3NonCenter() + corner, texCoord, new Color(255, 255, 255, 255)));
    }
}