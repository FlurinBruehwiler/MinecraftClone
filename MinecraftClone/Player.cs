namespace RayLib3dTest;

public class Player
{
    public Vector3 Position = new(0, 100, 0);
    public Vector3 VelocityPerSecond;
    public Vector3 Direction = new(0, 0, 1);
    public Camera3D Camera;
    private float _sensitivity = 1;
    public static Vector3 CameraOffset = new(0, -0.18f, 0);
    public float yaw; // around world Y
    public float pitch; // around camera local X
    public Vector3 Forward => Direction;
    public Vector3 Right => new(-Direction.Z, 0, Direction.X);
    public Vector3 Up => new(0, 1, 0); //ToDo

    private BlockDefinition _selectedBlock = Blocks.Air;

    public Hitbox GetHitBox()
    {
        var minVector = new Vector3();
        
        minVector.X = Position.X - Physics.PlayerWidth / 2;
        minVector.Y = Position.Y - Physics.PlayerHeight + Physics.SkinWidth;
        minVector.Z = Position.Z - Physics.PlayerWidth / 2;
       
        var maxVector = new Vector3();
        
        maxVector.X = Position.X + Physics.PlayerWidth / 2;
        maxVector.Y = Position.Y;
        maxVector.Z = Position.Z + Physics.PlayerWidth / 2;
        
        
        return new Hitbox(minVector, maxVector);
    }

    public void Move(Vector3 direction)
    {
        var isJumping = direction.Y > 0;
        direction.Y = 0;

        if (direction.Length() != 0)
            direction = Vector3.Normalize(direction);

        float gravity = -0.5f;
        float jumpVelocity = 11.5f;

        const float speed = 6;

        var posChange = new Vector3(direction.X * speed, VelocityPerSecond.Y, direction.Z * speed) * GetFrameTime();

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

        HandleHotBarInput();
        HandleBlockDestroy();
        HandleBlockPlacement();
    }

    public Player()
    {
        Camera = new Camera3D(Vector3.Zero, new Vector3(0, 0, 1), new Vector3(0, 1, 0), 100,
            CameraProjection.CAMERA_PERSPECTIVE);
    }

    public void Update()
    {
        HandleInput();
        UpdateCamera();
        Chunkloader.LoadChunksIfNeccesary(Position);
    }

    private void UpdateCamera()
    {
        Camera.position = Position + CameraOffset;
        Camera.target = Position + CameraOffset + Direction;
    }

    private void HandleInput()
    {
        var rotationInput = GetMouseDelta() * 0.2f;

        yaw += rotationInput.X * DEG2RAD;
        pitch += -rotationInput.Y * DEG2RAD;
        pitch = Math.Clamp(pitch, -MathF.PI / 2 + 0.001f, MathF.PI / 2 - 0.001f);

        var cosPitch = MathF.Cos(pitch);
        Direction = new Vector3(
            MathF.Cos(yaw) * cosPitch,
            MathF.Sin(pitch),
            MathF.Sin(yaw) * cosPitch
        );

        // Direction = Vector3RotateByAxisAngle(Direction, -Right, rotationVector.Y);
        // Direction = Vector3RotateByAxisAngle(Direction, new Vector3(0, 1, 0), rotationVector.X);

        // var right = Vector3.Normalize(Vector3.Cross(new Vector3(0,1,0), Direction));


        // Direction = Direction with { Y = Math.Clamp(Direction.Y, -0.99f, +0.99f) };


        var moveDelta = GetMovementInput();

        var right = Vector3.Normalize(new Vector3(Right.X, 0, Right.Z));
        var forward = Vector3.Normalize(new Vector3(-Right.Z, 0, Right.X));

        var xComponent = right * moveDelta.X;
        var zComponent = forward * moveDelta.Z;

        var globalMoveDelta = xComponent + zComponent;
        globalMoveDelta.Y = moveDelta.Y;

        Move(globalMoveDelta);

        DevTools.Print(Position, "Player_Pos");
        DevTools.Print(Direction, "Player_Direction");
        DevTools.Print(GetChunkCoordinate(Position.ToIntVector3()), "Chunk");

        var col = Physics.Raycast(Position + CameraOffset, Direction, 10, out _, out _, true);
        DevTools.Print(col, "Looking at Block");

        DevTools.Print(GetFPS(), "FPS");
    }

    // private void HandleSpeedChange()
    // {
    //     if (GetMouseWheelMoveV().Y > 0)
    //     {
    //         _playerSpeed *= 1.1f;
    //     }
    //     else if (GetMouseWheelMoveV().Y < 0)
    //     {
    //         _playerSpeed *= 0.9f;
    //     }
    //
    //     _playerSpeed = Math.Max(_playerSpeed, 0);
    // }

    private Vector3 GetMovementInput()
    {
        var inputDirection = new Vector3();

        if (IsKeyDown(KeyboardKey.KEY_W))
        {
            inputDirection.Z -= 1;
        }

        if (IsKeyDown(KeyboardKey.KEY_S))
        {
            inputDirection.Z += 1;
        }

        if (IsKeyDown(KeyboardKey.KEY_D))
        {
            inputDirection.X += 1;
        }

        if (IsKeyDown(KeyboardKey.KEY_A))
        {
            inputDirection.X -= 1;
        }

        if (IsKeyDown(KeyboardKey.KEY_SPACE))
        {
            inputDirection.Y += 1;
        }

        if (IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL))
        {
            inputDirection.Y -= 1;
        }

        return inputDirection;
    }

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
            var col = Physics.Raycast(Position + CameraOffset, Direction, 10, out var previousBlock, out _, true);
            if (col is not null)
            {
                var blockHitbox = new Hitbox(previousBlock.ToVector3(), previousBlock.ToVector3() + Vector3.One);
                
                if (!Physics.AreOverlapping(GetHitBox(), blockHitbox))
                {
                    ref var b = ref CurrentWorld.TryGetBlockAtPos(previousBlock, out var wasFound);
                    if (wasFound)
                    {
                        b.BlockId = _selectedBlock.Id;

                        CurrentWorld.InformBlockUpdate(previousBlock);
                    }
                }
            }
        }
    }

    private void HandleBlockDestroy()
    {
        if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
        {
            var col = Physics.Raycast(Position + CameraOffset, Direction, 10, out _, out _, true);
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