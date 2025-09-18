namespace RayLib3dTest;

public class Player
{
    public Vector3 Position = new(0, 100, 0);
    public Vector3 VelocityPerSecond;
    public Vector3 Direction = new(0, 0, 1);

    public static Vector3 CameraOffset = new Vector3(0, -0.18f, 0);

    // public Camera3D Camera { get; }

    public void Move(Vector3 direction)
    {
        var isJumping = direction.Y > 0;
        direction.Y = 0;

        if (direction.Length() != 0)
            direction = Vector3.Normalize(direction);

        const float acceleration = 5 * 0.098f;
        // const float frictionPerTick = 0.546f;


        // float jumpHeight = 4;
        // float timeToJumpApex = 0.4f;

        float gravity = -0.5f;
        float jumpVelocity = 11.5f;

        // VelocityPerSecond += acceleration * direction;// new Vector3(VelocityPerSecond.X + acceleration * direction.X, VelocityPerSecond.Y, VelocityPerSecond.Z + acceleration * direction.Z);

        const float speed = 6;

        var posChange = new Vector3(direction.X * speed, VelocityPerSecond.Y, direction.Z * speed) * GetFrameTime();

        //var frictionPerDt = MathF.Pow(frictionPerTick, 20 * GetFrameTime());
        // VelocityPerSecond *= frictionPerDt;// new Vector3(VelocityPerSecond.X * frictionPerDt, VelocityPerSecond.Y, VelocityPerSecond.Z * frictionPerDt);

        var colInfo = new CollisionInfo();
        Physics.VerticalCollisions(ref posChange, ref colInfo, Position);
        Physics.ForwardCollisions(ref posChange, ref colInfo, Position);
        Physics.SidewardCollisions(ref posChange, ref colInfo, Position);

        if (colInfo.Up || colInfo.Down)
            VelocityPerSecond.Y = 0;

        VelocityPerSecond.Y += gravity;

        if (isJumping && colInfo.Down)
        {
            VelocityPerSecond.Y = jumpVelocity;
        }

        Position += posChange;

        // DevTools.RenderActions.Add(() =>
        // {
        //     Raylib.DrawCubeWires(Position with {Y = Position.Y - Physics.PlayerHeight / 2}, Physics.PlayerWidth, Physics.PlayerHeight, Physics.PlayerWidth, Color.BLACK);
        // });

        HandleHotBarInput();
        HandleBlockDestroy();
        HandleBlockPlacement();
    }

    public float yaw;   // around world Y
    public float pitch; // around camera local X

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
            var col = Physics.Raycast(Position + Player.CameraOffset, Direction, 10, out var previousBlock, out _, true);
            if (col is not null)
            {
                ref var b = ref CurrentWorld.TryGetBlockAtPos(previousBlock, out var wasFound);
                if (wasFound)
                {
                    b.BlockId = _selectedBlock.Id;

                    CurrentWorld.InformBlockUpdate(previousBlock);
                    // var chunk = CurrentWorld.GetChunk(previousBlock);
                    // chunk.GenMesh();
                }
            }
        }
    }

    private void HandleBlockDestroy()
    {
        if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
        {
            var col = Physics.Raycast(Position + Player.CameraOffset, Direction, 10, out _, out _, true);
            if (col is not null)
            {
                ref var b = ref CurrentWorld.TryGetBlockAtPos(col.Value, out var wasFound);
                if (wasFound)
                {
                    b.BlockId = Blocks.Air.Id;

                    CurrentWorld.InformBlockUpdate(col.Value);
                    // var chunk = CurrentWorld.GetChunk(col.Value);
                    // chunk.GenMesh();
                }
            }
        }
    }
}