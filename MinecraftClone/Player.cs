using SourceGen;

namespace RayLib3dTest;

public struct InventorySlot
{
    public ushort BlockId;
    public int Count;
}

public class Player
{
    public Vector3 Position = new(0, 100, 0);
    public Vector3 LastPosition = new(0, 100, 0);
    public Vector3 Velocity;
    public Vector3 Direction = new(0, 0, 1);
    public static Camera3D C3d;
    public Camera3D Camera
    {
        get => C3d;
        set => C3d = value;
    }
    private float _sensitivity = 1;
    public static Vector3 CameraOffset = new(0, -0.18f, 0);
    public float yaw; // around world Y
    public float pitch; // around camera local X
    public Vector3 Right => new(-Direction.Z, 0, Direction.X);
    public Vector3 Up => new(0, 1, 0); //ToDo
    public InventorySlot[] Inventory = new InventorySlot[9];
    public int SelectedHotbarSlot;

    // public BlockDefinition SelectedBlock = Blocks.Air;

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



    public Player()
    {
        Camera = new Camera3D(Vector3.Zero, new Vector3(0, 0, 1), new Vector3(0, 1, 0), 100,
            CameraProjection.Perspective);

        int i = 0;

        IEnumerable<BlockDefinition> blocks = [Blocks.Dirt, Blocks.Grass, Blocks.ShortGrass, Blocks.DiamondBlock, Blocks.WoodenPlank, Blocks.LeaveBlock];

        foreach (var block in blocks)
        {
            if (block != Blocks.Air)
            {
                Inventory[i] = new InventorySlot
                {
                    BlockId = block.Id,
                    Count = 64
                };
                i++;
            }
        }
    }

    public void Tick()
    {
        LastPosition = Position;

        //player movement is done in tick

        var direction = GetHorizontal();

        const float speedMultiplier = 1.3f;
        var config = MovementConfig.Default;

        if (Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.Q))
            config.HorizontalAcceleration *= speedMultiplier;

        var halfWidth = Physics.PlayerWidth / 2f;
        var hitbox = new Hitbox(new Vector3(-halfWidth, -Physics.PlayerHeight, -halfWidth),
            new Vector3(halfWidth, 0, halfWidth));

        Movement.Move(ref Velocity, ref Position,
            Direction, direction, hitbox, isJumpPressed, config);

        isJumpPressed = false;

        // DevTools.Print(colInfo.Down, "OnGround");
        DevTools.Print(Velocity, "velocity");
    }

    private bool isJumpPressed;

    public void Update()
    {
        if (Raylib.IsKeyDown(KeyboardKey.Space))
            isJumpPressed = true;

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

            lookingAtBlock = Physics.Raycast(Camera.Position, Direction, 50, out var before, out _, false);
            if (lookingAtBlock != null)
            {
                lookingAtBlockBefore = before;
            }
            else
            {
                lookingAtBlockBefore = null;
            }

            DevTools.Print(lookingAtBlock, "Looking at Block");
            DevTools.Print(lookingAtBlockBefore, "Looking at Block before");

            DevTools.Print(Raylib.GetFPS(), "FPS");

            //handle bot target
            if (Raylib.IsMouseButtonPressed(MouseButton.Middle) && lookingAtBlockBefore != null)
            {
                foreach (var bot in CurrentWorld.bots)
                {
                    bot.Target = lookingAtBlockBefore.Value;
                }
            }

            if (Raylib.IsKeyPressed(KeyboardKey.E) && lookingAtBlockBefore != null)
            {
                CurrentWorld.bots.Add(new Bot
                {
                    Model = CurrentWorld.Game.HuskModel,
                    Position = (lookingAtBlockBefore.Value with { Y = lookingAtBlockBefore.Value.Y + 1 }).ToVector3(),
                });
            }
        }
    }

    private IntVector3? lookingAtBlock;
    private IntVector3? lookingAtBlockBefore;

    public void Render()
    {
        // DrawCubeWiresV(Position with {Y= Position.Y - Physics.PlayerHeight / 2 }, new Vector3(Physics.PlayerWidth, Physics.PlayerHeight, Physics.PlayerWidth), Color.BLUE);

        if (lookingAtBlock.HasValue)
        {
            Raylib.DrawCubeWiresV(lookingAtBlock.Value.ToVector3(), Vector3.One * 1.001f, Color.Black);
        }
    }

    private void UpdateCamera()
    {
        var t = 1 / Game.TickRateMs * Game.MsSinceLastTick();

        var cameraPos = Vector3.Lerp(LastPosition, Position, t);

        //this needs to be smoothed
        C3d.Position = cameraPos + CameraOffset;
        C3d.Target = cameraPos + CameraOffset + Direction;
        DevTools.Print(Camera.Position, "camera_pos");
    }

    private void HandleDirectionChange()
    {
        var rotationInput = Raylib.GetMouseDelta() * 0.2f;

        yaw += rotationInput.X * Raylib.DEG2RAD;
        pitch += -rotationInput.Y * Raylib.DEG2RAD;
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

    private Vector3 GetHorizontal()
    {
        var inputDirection = new Vector3();

        if (Raylib.IsKeyDown(KeyboardKey.W))
        {
            inputDirection.Z -= 1;
        }

        if (Raylib.IsKeyDown(KeyboardKey.S))
        {
            inputDirection.Z += 1;
        }

        if (Raylib.IsKeyDown(KeyboardKey.D))
        {
            inputDirection.X += 1;
        }

        if (Raylib.IsKeyDown(KeyboardKey.A))
        {
            inputDirection.X -= 1;
        }

        return inputDirection;
    }

    private void HandleHotBarInput()
    {
        var x = Raylib.GetKeyPressed();

        if (x is >= 49 and <= 57)
        {
            SelectedHotbarSlot = x - 49;
        }

        var input = Raylib.GetMouseWheelMove();
        if (input < 0)
        {
            SelectedHotbarSlot++;
        }
        else if (input > 0)
        {
            SelectedHotbarSlot--;
        }

        SelectedHotbarSlot = Math.Min(SelectedHotbarSlot, 8);
        SelectedHotbarSlot = Math.Max(SelectedHotbarSlot, 0);
    }

    private void HandleBlockPlacement()
    {
        if (Raylib.IsMouseButtonPressed(MouseButton.Right))
        {
            var col = Physics.Raycast(Camera.Position, Direction, 10, out var previousBlock, out _, true);
            if (col is not null)
            {
                var blockHitbox = new Hitbox(previousBlock.ToVector3NonCenter(), previousBlock.ToVector3NonCenter() + Vector3.One);
                
                if (!Physics.AreOverlapping(GetHitBox(), blockHitbox))
                {
                    ref var b = ref CurrentWorld.TryGetBlockAtPos(previousBlock, out var wasFound);
                    if (wasFound)
                    {
                        ref var res = ref Inventory[SelectedHotbarSlot];
                        if (res.Count > 0)
                        {
                            b.BlockId = res.BlockId;
                            res.Count--;

                            if(res.Count == 0)
                                res = new InventorySlot();
                        }

                        CurrentWorld.InformBlockUpdate(previousBlock);
                    }
                }
            }
        }
    }

    private void HandleBlockDestroy()
    {
        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            var col = Physics.Raycast(Camera.Position, Direction, 10, out _, out _, true);
            if (col is not null)
            {
                ref var b = ref CurrentWorld.TryGetBlockAtPos(col.Value, out var wasFound);
                if (wasFound)
                {
                    for (var i = 0; i < Inventory.Length; i++)
                    {
                        ref var inventorySlot = ref Inventory[i];
                        if (inventorySlot.BlockId == b.BlockId)
                        {
                            inventorySlot.Count++;
                            inventorySlot.Count = Math.Min(inventorySlot.Count, 64);
                            break;
                        }
                    }

                    b.BlockId = Blocks.Air.Id;

                    CurrentWorld.InformBlockUpdate(col.Value);
                    // var chunk = CurrentWorld.GetChunk(col.Value);
                    // chunk.GenMesh();
                }
            }
        }
    }
}