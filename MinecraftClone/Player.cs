namespace RayLib3dTest;

public class Player
{
    public Vector3 Position = new(0, 100, 0);
    public Vector3 Velocity;
    public Vector3 Direction = new(0, 0, 1);

    public CollisionInfo CollisionInfo;
    // public Camera3D Camera { get; }

    public void Move(Vector3 posChangeInWorldSpace)
    {
        CollisionInfo.Reset();

        DevTools.Print(posChangeInWorldSpace, "GlobalMoveDelta");

        Velocity.Y += -.2f * GetFrameTime();

        posChangeInWorldSpace += Velocity;


        Physics.VerticalCollisions(ref posChangeInWorldSpace, Position, out var isVerticalHit);
        Physics.ForwardCollisions(ref posChangeInWorldSpace, Position);
        Physics.SidewardCollisions(ref posChangeInWorldSpace, Position);


        if (isVerticalHit)
            Velocity.Y = 0;

        Position += posChangeInWorldSpace;

        DevTools.RenderActions.Add(() =>
        {
            Raylib.DrawCubeWires(Position with {Y = Position.Y - Physics.PlayerHeight / 2}, Physics.PlayerWidth, Physics.PlayerHeight, Physics.PlayerWidth, Color.BLACK);
        });

        HandleHotBarInput();
        HandleBlockDestroy();
        HandleBlockPlacement();

        var result = Physics.Raycast(Position, Direction, 100, out _, out var distance);
        if (result.HasValue)
        {
            // DevTools.Debug3dInstructions.Add(new Debug3dInstruction
            // {
            //     PointA = result.Value.ToVector3() + new Vector3(0.5f),
            //     Color = Color.RED,
            //     Scalar = 1.01f,
            //     Type = Debug3InstructionType.Cube
            // });
            DevTools.Debug3dInstructions.Add(new Debug3dInstruction
            {
                PointA = Position + Direction * distance,
                Scalar = 0.05f,
                Color = Color.BLUE,
                Type = Debug3InstructionType.Sphere
            });
        }
    }

    public Vector3 Forward => Direction;

    public Vector3 Right => new(-Direction.Z, 0, Direction.X);

    public Vector3 Up => new(0, 1, 0); //ToDo

    private BlockDefinition _selectedBlock = Blocks.Air;

    private void HandleHotBarInput()
    {
        var x = GetKeyPressed();

        if (x is >= 48 and <= 57)
        {
            var idx = x - 49;
            if (Blocks.BlockList.TryGetValue((ushort)idx, out var bd))
            {
                _selectedBlock = bd;
            }
        }
    }

    private void HandleBlockPlacement()
    {
        if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT))
        {
            var col = Physics.Raycast(Position, Direction, 10, out var previousBlock, out _, true);
            if (col is not null)
            {
                ref var b = ref CurrentWorld.TryGetBlockAtPos(previousBlock, out var wasFound);
                if (wasFound)
                {
                    b.BlockId = _selectedBlock.Id;

                    var chunk = CurrentWorld.GetChunk(previousBlock);
                    chunk.GenMesh();
                }
            }
        }
    }

    private void HandleBlockDestroy()
    {
        if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
        {
            var col = Physics.Raycast(Position, Direction, 10, out _, out _, true);
            if (col is not null)
            {
                ref var b = ref CurrentWorld.TryGetBlockAtPos(col.Value, out var wasFound);
                if (wasFound)
                {
                    b.BlockId = Blocks.Air.Id;

                    var chunk = CurrentWorld.GetChunk(col.Value);
                    chunk.GenMesh();
                }
            }
        }
    }
}