﻿namespace RayLib3dTest;

public class Player : IControlable
{
    private readonly SirPhysics _sirPhysics;
    private readonly Colcol _colcol;
    private readonly GlobalBoy _globalBoy;
    private readonly Debuggerus _debuggerus;
    public Vector3 Position { get; set; } = new(0, 100, 0);
    public Vector3 Velocity;
    public Vector3 Direction { get; set; } = new(0, 0, 1);

    public CollisionInfo CollisionInfo;
    // public Camera3D Camera { get; }

    public Player(SirPhysics sirPhysics, Colcol colcol, GlobalBoy globalBoy, Debuggerus debuggerus)
    {
        _sirPhysics = sirPhysics;
        _colcol = colcol;
        _globalBoy = globalBoy;
        _debuggerus = debuggerus;
    }

    public void Move(Vector3 posChangeInWorldSpace)
    {
        CollisionInfo.Reset();

        _debuggerus.Print(posChangeInWorldSpace, "GlobalMoveDelta");

        Velocity.Y += -.2f * GetFrameTime();

        posChangeInWorldSpace += Velocity;

        _sirPhysics.VerticalCollisions(ref posChangeInWorldSpace, Position, out var isHorizontalHit);
        _sirPhysics.ForwardCollisions(ref posChangeInWorldSpace, Position);
        _sirPhysics.SidewardCollisions(ref posChangeInWorldSpace, Position);

        if (isHorizontalHit)
            Velocity.Y = 0;

        Position += posChangeInWorldSpace;

        HandleHotBarInput();
        HandleBlockDestroy();
        HandleBlockPlacement();
    }

    public Vector3 Forward => Direction;

    public Vector3 Right => new(-Direction.Z, 0, Direction.X);

    public Vector3 Up => new(0, 1, 0);//ToDo

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
            var col = _colcol.Raycast(Position, Direction, 10, out var previousBlock, out _);
            if (col is not null)
            {
                ref var b = ref _globalBoy.TryGetBlockAtPos(previousBlock, out var wasFound);
                if (wasFound)
                {
                    b.BlockId = _selectedBlock.Id;

                    var chunk = _globalBoy.GetChunk(previousBlock);
                    chunk.GenMesh();
                }
            }
        }
    }

    private void HandleBlockDestroy()
    {
        if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
        {
            var col = _colcol.Raycast(Position, Direction, 10, out _, out _);
            if (col is not null)
            {
                ref var b = ref _globalBoy.TryGetBlockAtPos(col.Value, out var wasFound);
                if (wasFound)
                {
                    b.BlockId = Blocks.Air.Id;

                    var chunk = _globalBoy.GetChunk(col.Value);
                    chunk.GenMesh();
                }
            }
        }
    }
}
