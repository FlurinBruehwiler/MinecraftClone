namespace RayLib3dTest;

public class Player : IControlable
{
    private readonly SirPhysics _sirPhysics;
    private readonly Colcol _colcol;
    private readonly GlobalBoy _globalBoy;
    public Vector3 Position { get; set; }
    public Vector3 Direction { get; set; }
    // public Camera3D Camera { get; }

    public Player(SirPhysics sirPhysics, Colcol colcol, GlobalBoy globalBoy)
    {
        _sirPhysics = sirPhysics;
        _colcol = colcol;
        _globalBoy = globalBoy;
    }

    public void SetPositionWithPhysics(Vector3 posChangeInWorldSpace)
    {
        _sirPhysics.VerticalCollisions(ref posChangeInWorldSpace, Position);
        _sirPhysics.ForwardCollisions(ref posChangeInWorldSpace, Position);
        _sirPhysics.SidewardCollisions(ref posChangeInWorldSpace, Position);
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