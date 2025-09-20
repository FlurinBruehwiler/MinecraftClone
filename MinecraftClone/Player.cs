namespace RayLib3dTest;

public class Player
{
    public Vector3 Position = new(0, 100, 0);
    public Vector3 LastPosition = new(0, 100, 0);
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

    //once we add more entities, this needs to be able to operate on general entities, not just the player...
    public CollisionInfo MoveWithCollision(ref Vector3 velocity)
    {
        var colInfo = new CollisionInfo();
        Physics.VerticalCollisions(ref velocity, ref colInfo, Position);
        Physics.ForwardCollisions(ref velocity, ref colInfo, Position);
        Physics.SidewardCollisions(ref velocity, ref colInfo, Position);

        return colInfo;
    }

    public Player()
    {
        Camera = new Camera3D(Vector3.Zero, new Vector3(0, 0, 1), new Vector3(0, 1, 0), 100,
            CameraProjection.CAMERA_PERSPECTIVE);
    }

    public void Tick()
    {
        LastPosition = Position;

        //player movement is done in tick

        var direction = GetMovementInput();

        var right = Vector3.Normalize(new Vector3(Right.X, 0, Right.Z));
        var forward = Vector3.Normalize(new Vector3(-Right.Z, 0, Right.X));

        var xComponent = right * direction.X;
        var zComponent = forward * direction.Z;

        var globalMoveDelta = xComponent + zComponent;
        globalMoveDelta.Y = direction.Y;

        var isJumping = globalMoveDelta.Y > 0;
        globalMoveDelta.Y = 0;

        if (globalMoveDelta.Length() != 0)
            globalMoveDelta = Vector3.Normalize(globalMoveDelta);

        float gravity = -5f;
        float jumpVelocity = 35f;

        const float speed = 40;

        var posChange = new Vector3(globalMoveDelta.X * speed, VelocityPerSecond.Y, globalMoveDelta.Z * speed) * GetFrameTime();

        var colInfo = MoveWithCollision(ref posChange);

        if (colInfo.Up || colInfo.Down)
            VelocityPerSecond.Y = 0;

        VelocityPerSecond.Y += gravity;

        if (isJumping && colInfo.Down)
        {
            VelocityPerSecond.Y = jumpVelocity;
        }

        Position += posChange;
    }

    public void Update()
    {
        HandleDirectionChange();
        UpdateCamera();
        Chunkloader.LoadChunksIfNeccesary(Position);

        HandleHotBarInput();
        HandleBlockDestroy();
        HandleBlockPlacement();

        //Debug
        {
            DevTools.Print(Position, "Player_Pos");
            DevTools.Print(Direction, "Player_Direction");
            DevTools.Print(GetChunkCoordinate(Position.ToIntVector3()), "Chunk");

            lookingAtBlock = Physics.Raycast(Camera.position, Direction, 10, out _, out _, true);
            DevTools.Print(lookingAtBlock, "Looking at Block");

            DevTools.Print(GetFPS(), "FPS");
        }
    }

    private IntVector3? lookingAtBlock;

    public void Render()
    {
        if (lookingAtBlock.HasValue)
        {
            DrawCubeWiresV(lookingAtBlock.Value.ToVector3() + Vector3.One / 2, Vector3.One * 1.001f, Color.BLACK);
        }
    }

    private void UpdateCamera()
    {
        var t = 1 / Game.TickRateMs * Game.MsSinceLastTick();

        var cameraPos = Vector3.Lerp(LastPosition, Position, t);

        //this needs to be smoothed
        Camera.position = cameraPos + CameraOffset;
        Camera.target = cameraPos + CameraOffset + Direction;
    }

    private void HandleDirectionChange()
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
            var col = Physics.Raycast(Camera.position, Direction, 10, out var previousBlock, out _, true);
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
            var col = Physics.Raycast(Camera.position, Direction, 10, out _, out _, true);
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